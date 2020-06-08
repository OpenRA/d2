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

		[Desc("Delay between trail updates.")]
		public readonly int UpdateInterval = 100;

		[Desc("Delay before first trail.",
			"Use negative values for falling back to the *Interval values.")]
		public readonly int StartDelay = 0;

		public override object Create(ActorInitializer init) { return new D2LeavesTracks(init.Self, this); }
	}

	public class D2LeavesTracks : ConditionalTrait<D2LeavesTracksInfo>, ITick
	{
		BodyOrientation body;
		IFacing facing;
		int cachedFacing;
		int cachedInterval;

		CPos cachedCell;

		bool previouslySpawned;
		CPos previosSpawnCell;
		int previousSpawnFacing;

		int ticks;
		bool wasStationary;
		bool isMoving;

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
			cachedCell = self.World.Map.CellContaining(self.CenterPosition);

			previouslySpawned = false;

			base.Created(self);
		}

		void ITick.Tick(Actor self)
		{
			if (IsTraitDisabled || string.IsNullOrEmpty(Info.Image))
				return;

			wasStationary = !isMoving;
			var currentCell = self.World.Map.CellContaining(self.CenterPosition);
			isMoving = currentCell != cachedCell;

			if (!isMoving)
				return;

			if (isMoving == wasStationary && (Info.StartDelay > -1))
			{
				cachedInterval = Info.StartDelay;
				ticks = 0;
			}

			if (++ticks >= cachedInterval)
			{
				var type = self.World.Map.GetTerrainInfo(cachedCell).Type;

				if ((Info.TerrainTypes.Count == 0 || Info.TerrainTypes.Contains(type)) && self.World.Map.Contains(cachedCell))
				{
					int spawnFacing = previouslySpawned && previosSpawnCell.Equals(cachedCell) ? previousSpawnFacing : cachedFacing;
					var pos = self.World.Map.CenterOfCell(cachedCell);

					self.World.AddFrameEndTask(w => w.Add(new SpriteEffect(pos, WAngle.FromFacing(spawnFacing), self.World, Info.Image,
						Info.Sequence, Info.Palette, Info.VisibleThroughFog)));

					previouslySpawned = true;
					previosSpawnCell = cachedCell;
					previousSpawnFacing = spawnFacing;
				}

				cachedCell = currentCell;
				cachedFacing = facing != null ? facing.Facing : 0;
				ticks = 0;

				cachedInterval = Info.UpdateInterval;
			}
		}

		protected override void TraitEnabled(Actor self)
		{
			cachedCell = self.World.Map.CellContaining(self.CenterPosition);
		}
	}
}
