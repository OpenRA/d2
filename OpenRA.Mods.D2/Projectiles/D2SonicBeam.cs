#region Copyright & License Information
/*
 * Copyright 2007-2020 The d2 mod Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.GameRules;
using OpenRA.Graphics;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.D2.Graphics;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.D2.Projectiles
{
	[Desc("Dune 2 sonic beam that damages like AreaBeam but renders as screen-space distortion.")]
	public class D2SonicBeamInfo : IProjectileInfo
	{
		[Desc("Projectile speed in WDist / tick, two values indicate a randomly picked velocity per beam.")]
		public readonly WDist[] Speed = { new(128) };

		[Desc("The maximum duration (in ticks) of each beam burst.")]
		public readonly int Duration = 10;

		[Desc("The number of ticks between the beam causing warhead impacts in its area of effect.")]
		public readonly int DamageInterval = 3;

		[Desc("The width of the beam.")]
		public readonly WDist Width = new(512);

		[Desc("The shape of the beam. Kept for AreaBeam YAML compatibility.")]
		public readonly BeamRenderableShape Shape = BeamRenderableShape.Cylindrical;

		[Desc("How far beyond the target the projectile keeps on travelling.")]
		public readonly WDist BeyondTargetRange = new(0);

		[Desc("The minimum distance the beam travels.")]
		public readonly WDist MinDistance = WDist.Zero;

		[Desc("Damage modifier applied at each range step.")]
		public readonly int[] Falloff = { 100, 100 };

		[Desc("Ranges at which each Falloff step is defined.")]
		public readonly WDist[] Range = { WDist.Zero, new(int.MaxValue) };

		[Desc("The maximum/constant/incremental inaccuracy used in conjunction with the InaccuracyType property.")]
		public readonly WDist Inaccuracy = WDist.Zero;

		[Desc("Controls the way inaccuracy is calculated. Possible values are " +
			"'Maximum' - scale from 0 to max with range, " +
			"'PerCellIncrement' - scale from 0 with range, " +
			"'Absolute' - use set value regardless of range.")]
		public readonly InaccuracyType InaccuracyType = InaccuracyType.Maximum;

		[Desc("Can this projectile be blocked when hitting actors with an IBlocksProjectiles trait.")]
		public readonly bool Blockable = false;

		[Desc("Does the beam follow the target.")]
		public readonly bool TrackTarget = false;

		[Desc("Should the screen distortion be visually rendered?")]
		public readonly bool RenderBeam = true;

		[Desc("Equivalent to sequence ZOffset. Kept for AreaBeam YAML compatibility.")]
		public readonly int ZOffset = 0;

		[Desc("Kept for AreaBeam YAML compatibility. D2SonicBeam does not draw a colored beam.")]
		public readonly Color Color = Color.Cyan;

		[Desc("Kept for AreaBeam YAML compatibility. D2SonicBeam does not draw a colored beam.")]
		public readonly bool UsePlayerColor = false;

		public readonly string Image = "sonic_blast";
		[SequenceReference(nameof(Image))]
		public readonly string Sequence = "idle";

		public IProjectile Create(ProjectileArgs args) { return new D2SonicBeam(this, args); }
	}

	public class D2SonicBeam : IProjectile, ISync
	{
		// This is intentionally a copy of AreaBeam's movement and damage model with only the
		// visual Render() path swapped out. Keeping the tick logic local avoids changing Common
		// projectile behavior while letting D2 draw the original-style screen distortion.
		readonly D2SonicBeamInfo info;
		readonly ProjectileArgs args;
		readonly AttackBase actorAttackBase;
		readonly WDist speed;
		readonly WDist weaponRange;
		readonly Sprite sprite;

		[Sync]
		WPos headPos;

		[Sync]
		WPos tailPos;

		[Sync]
		WPos target;

		int length;
		WAngle towardsTargetFacing;
		int headTicks;
		int tailTicks;
		bool isHeadTravelling = true;
		bool isTailTravelling;
		bool continueTracking = true;

		bool IsBeamComplete => !isHeadTravelling && headTicks >= length && !isTailTravelling && tailTicks >= length;

		public D2SonicBeam(D2SonicBeamInfo info, ProjectileArgs args)
		{
			this.info = info;
			this.args = args;
			actorAttackBase = args.SourceActor.Trait<AttackBase>();
			sprite = args.SourceActor.World.Map.Sequences.GetSequence(info.Image, info.Sequence).GetSprite(0);

			var world = args.SourceActor.World;
			if (info.Speed.Length > 1)
				speed = new WDist(world.SharedRandom.Next(info.Speed[0].Length, info.Speed[1].Length));
			else
				speed = info.Speed[0];

			headPos = args.Source;
			tailPos = headPos;

			target = args.PassiveTarget;
			if (info.Inaccuracy.Length > 0)
			{
				var maxInaccuracyOffset = OpenRA.Mods.Common.Util.GetProjectileInaccuracy(info.Inaccuracy.Length, info.InaccuracyType, args);
				target += WVec.FromPDF(world.SharedRandom, 2) * maxInaccuracyOffset / 1024;
			}

			towardsTargetFacing = (target - headPos).Yaw;
			UpdateTargetForOvershoot();

			length = Math.Max((target - headPos).Length / speed.Length, 1);
			weaponRange = new WDist(OpenRA.Mods.Common.Util.ApplyPercentageModifiers(args.Weapon.Range.Length, args.RangeModifiers));
		}

		void UpdateTargetForOvershoot()
		{
			var dir = new WVec(0, -1024, 0).Rotate(WRot.FromYaw(towardsTargetFacing));
			var dist = (args.SourceActor.CenterPosition - target).Length;
			int extraDist;
			if (info.MinDistance.Length > dist)
			{
				if (info.MinDistance.Length - dist < info.BeyondTargetRange.Length)
					extraDist = info.BeyondTargetRange.Length;
				else
					extraDist = info.MinDistance.Length - dist;
			}
			else
				extraDist = info.BeyondTargetRange.Length;

			target += dir * extraDist / 1024;
		}

		void TrackTarget()
		{
			if (!continueTracking)
				return;

			if (args.GuidedTarget.IsValidFor(args.SourceActor))
			{
				var guidedTargetPos = args.Weapon.TargetActorCenter ? args.GuidedTarget.CenterPosition : args.GuidedTarget.Positions.ClosestToIgnoringPath(args.Source);
				var targetDistance = new WDist((guidedTargetPos - args.Source).Length);

				if (targetDistance > weaponRange + info.BeyondTargetRange)
					StopTargeting();
				else
				{
					target = guidedTargetPos;
					towardsTargetFacing = (target - args.Source).Yaw;

					var dir = new WVec(0, -1024, 0).Rotate(WRot.FromYaw(towardsTargetFacing));
					target += dir * info.BeyondTargetRange.Length / 1024;
				}
			}
		}

		void StopTargeting()
		{
			continueTracking = false;
			isTailTravelling = true;
		}

		public void Tick(World world)
		{
			if (info.TrackTarget)
				TrackTarget();

			if (++headTicks >= length)
			{
				headPos = target;
				isHeadTravelling = false;
			}
			else if (isHeadTravelling)
				headPos = WPos.LerpQuadratic(args.Source, target, WAngle.Zero, headTicks, length);

			if (tailTicks <= 0 && args.SourceActor.IsInWorld && !args.SourceActor.IsDead)
			{
				args.Source = args.CurrentSource();
				tailPos = args.Source;
			}

			var outOfWeaponRange = weaponRange + info.BeyondTargetRange < new WDist((args.PassiveTarget - args.Source).Length);
			if ((headTicks >= info.Duration && !isTailTravelling) || args.SourceActor.IsDead ||
				!actorAttackBase.IsAiming || outOfWeaponRange)
				StopTargeting();

			if (isTailTravelling)
			{
				if (++tailTicks >= length)
				{
					tailPos = target;
					isTailTravelling = false;
				}
				else
					tailPos = WPos.LerpQuadratic(args.Source, target, WAngle.Zero, tailTicks, length);
			}

			if (info.Blockable && BlocksProjectiles.AnyBlockingActorsBetween(world, args.SourceActor.Owner, tailPos, headPos, info.Width, out var blockedPos))
			{
				headPos = blockedPos;
				target = headPos;
				length = Math.Min(headTicks, length);
			}

			if (headTicks % info.DamageInterval == 0)
			{
				var actors = world.FindActorsOnLine(tailPos, headPos, info.Width);
				foreach (var a in actors)
				{
					var adjustedModifiers = args.DamageModifiers.Append(GetFalloff((args.Source - a.CenterPosition).Length));

					var warheadArgs = new WarheadArgs(args)
					{
						ImpactOrientation = new WRot(WAngle.Zero, OpenRA.Mods.Common.Util.GetVerticalAngle(args.Source, target), args.CurrentMuzzleFacing()),
						ImpactPosition = a.CenterPosition,
						DamageModifiers = adjustedModifiers.ToArray(),
					};

					args.Weapon.Impact(Target.FromActor(a), warheadArgs);
				}
			}

			if (IsBeamComplete)
				world.AddFrameEndTask(w => w.Remove(this));
		}

		public IEnumerable<IRenderable> Render(WorldRenderer wr)
		{
			if (isHeadTravelling && info.RenderBeam && !wr.World.FogObscures(headPos))
			{
				// OpenDUNE renders UNIT_SONIC_BLAST as a moving blurTile sprite
				// (groundSpriteID 160, DISPLAYMODE_SINGLE_FRAME), not as a continuous
				// colored beam. Keep the AreaBeam damage model above, but render one
				// screen-space blur mask at the travelling projectile head.
				return new[]
				{
					(IRenderable)new D2DistortionRenderable(headPos, sprite, D2DistortionStyle.Sonic)
				};
			}

			return SpriteRenderable.None;
		}

		int GetFalloff(int distance)
		{
			var inner = info.Range[0].Length;
			for (var i = 1; i < info.Range.Length; i++)
			{
				var outer = info.Range[i].Length;
				if (outer > distance)
					return int2.Lerp(info.Falloff[i - 1], info.Falloff[i], distance - inner, outer - inner);

				inner = outer;
			}

			return 0;
		}
	}
}
