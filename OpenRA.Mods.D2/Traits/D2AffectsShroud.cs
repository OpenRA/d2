#region Copyright & License Information
/*
 * Copyright 2007-2019 The d2 mod Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.D2.Traits
{
	public abstract class D2AffectsShroudInfo : ConditionalTraitInfo
	{
		[Desc("Range applied then idle.")]
		public readonly WDist Range = WDist.Zero;

		[Desc("Range applied then moving.")]
		public readonly WDist MovingRange = new WDist(1280);

		[Desc("If >= 0, prevent cells that are this much higher than the actor from being revealed.")]
		public readonly int MaxHeightDelta = -1;

		[Desc("Possible values are CenterPosition (measure range from the center) and ",
			"Footprint (measure range from the footprint)")]
		public readonly VisibilityType Type = VisibilityType.Footprint;
	}

	public abstract class D2AffectsShroud : ConditionalTrait<D2AffectsShroudInfo>, ITick, ISync, INotifyAddedToWorld, INotifyRemovedFromWorld
	{
		static readonly PPos[] NoCells = { };

		D2AffectsShroudInfo info;

		[Sync]
		CPos cachedLocation;

		[Sync]
		bool cachedDisabled;

		[Sync]
		bool cachedTraitDisabled;

		[Sync]
		bool cachedIdleRange;

		protected abstract void AddCellsToPlayerShroud(Actor self, Player player, PPos[] uv);
		protected abstract void RemoveCellsFromPlayerShroud(Actor self, Player player);
		protected virtual bool IsDisabled(Actor self) { return false; }

		public D2AffectsShroud(Actor self, D2AffectsShroudInfo info)
			: base(info)
		{
			this.info = info;
		}

		bool IsIdleRange(Actor self)
		{
			/*
			 * Can't use isIdle because some activities like Harvest should not use IdleRange
			 * And can't use IMove.isMoving because it is changed on each turn during one move
			 * Check that CurrentActivity is any activity from Activities/Move directory
			 */

			if (self.CurrentActivity as AttackMoveActivity != null
				|| self.CurrentActivity as Drag != null
				|| self.CurrentActivity as Follow != null
				|| self.CurrentActivity as Move != null
				|| self.CurrentActivity as MoveAdjacentTo != null
				|| self.CurrentActivity as MoveWithinRange != null)
			{
				return false;
			}

			return true;
		}

		PPos[] ProjectedCells(Actor self)
		{
			var map = self.World.Map;
			var range = IsIdleRange(self) ? Range : info.MovingRange;
			if (range == WDist.Zero)
				return NoCells;

			if (Info.Type == VisibilityType.Footprint)
				return self.OccupiesSpace.OccupiedCells()
					.SelectMany(kv => Shroud.ProjectedCellsInRange(map, kv.First, range, Info.MaxHeightDelta))
					.Distinct().ToArray();

			var pos = self.CenterPosition;
			if (Info.Type == VisibilityType.GroundPosition)
				pos -= new WVec(WDist.Zero, WDist.Zero, self.World.Map.DistanceAboveTerrain(pos));

			return Shroud.ProjectedCellsInRange(map, pos, range, range, Info.MaxHeightDelta)
				.ToArray();
		}

		void ITick.Tick(Actor self)
		{
			if (!self.IsInWorld)
				return;

			var centerPosition = self.CenterPosition;
			var projectedPos = centerPosition - new WVec(0, centerPosition.Z, centerPosition.Z);
			var projectedLocation = self.World.Map.CellContaining(projectedPos);
			var disabled = IsDisabled(self);
			var traitDisabled = IsTraitDisabled;
			var idle = IsIdleRange(self);

			if (cachedLocation == projectedLocation && traitDisabled == cachedTraitDisabled && cachedDisabled == disabled && cachedIdleRange == idle)
				return;

			cachedLocation = projectedLocation;
			cachedDisabled = disabled;
			cachedTraitDisabled = traitDisabled;
			cachedIdleRange = idle;

			var cells = ProjectedCells(self);
			foreach (var p in self.World.Players)
			{
				RemoveCellsFromPlayerShroud(self, p);
				AddCellsToPlayerShroud(self, p, cells);
			}
		}

		void INotifyAddedToWorld.AddedToWorld(Actor self)
		{
			var centerPosition = self.CenterPosition;
			var projectedPos = centerPosition - new WVec(0, centerPosition.Z, centerPosition.Z);
			cachedLocation = self.World.Map.CellContaining(projectedPos);
			cachedDisabled = IsDisabled(self);
			cachedTraitDisabled = IsTraitDisabled;
			cachedIdleRange = IsIdleRange(self);
			var cells = ProjectedCells(self);

			foreach (var p in self.World.Players)
				AddCellsToPlayerShroud(self, p, cells);
		}

		void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
		{
			foreach (var p in self.World.Players)
				RemoveCellsFromPlayerShroud(self, p);
		}

		public WDist Range { get { return (cachedDisabled || cachedTraitDisabled) ? WDist.Zero : Info.Range; } }
	}
}
