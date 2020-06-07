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
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.D2.Traits.Render
{
	[Desc("Renders tracks when vechicle leaving a cell.")]
	public class D2LeavesTracksInfo : ConditionalTraitInfo
	{
		public readonly string Image = null;

		[SequenceReference("Image")]
		public readonly string Sequence = "idle";

		[PaletteReference]
		public readonly string Palette = TileSet.TerrainPaletteInternalName;

		[Desc("Only leave trail on listed terrain types. Leave empty to leave trail on all terrain types.")]
		public readonly HashSet<string> TerrainTypes = new HashSet<string>();

		[Desc("Should the trail be visible through fog.")]
		public readonly bool VisibleThroughFog = false;

		[Desc("Display a trail while stationary.")]
		public readonly bool TrailWhileStationary = false;

		[Desc("Delay between trail updates when stationary.")]
		public readonly int StationaryInterval = 0;

		[Desc("Display a trail while moving.")]
		public readonly bool TrailWhileMoving = true;

		[Desc("Delay between trail updates when moving.")]
		public readonly int MovingInterval = 0;

		[Desc("Delay before first trail.",
			"Use negative values for falling back to the *Interval values.")]
		public readonly int StartDelay = 0;

		[Desc("Should the trail spawn relative to last position or current position?")]
		public readonly bool SpawnAtLastPosition = true;

		public override object Create(ActorInitializer init) { return new D2LeavesTracks(init.Self, this); }
	}

	public class D2LeavesTracks : ConditionalTrait<D2LeavesTracksInfo>, ITick
	{
		BodyOrientation body;
		IFacing facing;
		int cachedFacing;
		int cachedInterval;

		WPos cachedPosition;
		CPos previosSpawnCell = new CPos(-1, -1);
		int previousSpawnFacing;


		public D2LeavesTracks(Actor self, D2LeavesTracksInfo info)
			: base(info)
		{
			cachedInterval = Info.StartDelay;
		}

		protected override void Created(Actor self)
		{
			body = self.Trait<BodyOrientation>();
			facing = self.TraitOrDefault<IFacing>();
			cachedFacing = facing != null ? facing.Facing : 0;
			cachedPosition = self.CenterPosition;

			base.Created(self);
		}

		int ticks;
		bool wasStationary;
		bool isMoving;

		void ITick.Tick(Actor self)
		{
			if (IsTraitDisabled)
				return;

			wasStationary = !isMoving;
			isMoving = self.CenterPosition != cachedPosition;
			if ((isMoving && !Info.TrailWhileMoving) || (!isMoving && !Info.TrailWhileStationary))
				return;

			if (isMoving == wasStationary && (Info.StartDelay > -1))
			{
				cachedInterval = Info.StartDelay;
				ticks = 0;
			}

			if (++ticks >= cachedInterval)
			{
				var spawnCell = Info.SpawnAtLastPosition ? self.World.Map.CellContaining(cachedPosition) : self.World.Map.CellContaining(self.CenterPosition);
				if (!self.World.Map.Contains(spawnCell))
					return;

				var type = self.World.Map.GetTerrainInfo(spawnCell).Type;

				if ((Info.TerrainTypes.Count == 0 || Info.TerrainTypes.Contains(type)) && !string.IsNullOrEmpty(Info.Image))
				{
					int spawnFacing;

					if (previosSpawnCell.Equals(spawnCell))
						spawnFacing = previousSpawnFacing;
					else
						spawnFacing = Info.SpawnAtLastPosition ? cachedFacing : (facing != null ? facing.Facing : 0);

					var spawnPosition = Info.SpawnAtLastPosition ? cachedPosition : self.CenterPosition;
					var pos = self.World.Map.CenterOfCell(spawnCell);

					self.World.AddFrameEndTask(w => w.Add(new SpriteEffect(pos, WAngle.FromFacing(spawnFacing), self.World, Info.Image,
						Info.Sequence, Info.Palette, Info.VisibleThroughFog)));
					previosSpawnCell = spawnCell;
					previousSpawnFacing = spawnFacing;
				}

				cachedPosition = self.CenterPosition;
				cachedFacing = facing != null ? facing.Facing : 0;
				ticks = 0;

				cachedInterval = isMoving ? Info.MovingInterval : Info.StationaryInterval;
			}
		}

		protected override void TraitEnabled(Actor self)
		{
			cachedPosition = self.CenterPosition;
		}
	}
}
