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
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.D2.Graphics;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.D2.Traits.Render
{
	[Desc("Creates Dune 2 style screen-space distortion while an actor moves.")]
	public class D2DistortionTrailInfo : ConditionalTraitInfo
	{
		public readonly D2DistortionStyle Style = D2DistortionStyle.Sand;
		public readonly int Duration = 12;
		public readonly HashSet<string> TerrainTypes = new();
		public readonly string Image = "sandworm";
		[SequenceReference(nameof(Image))]
		public readonly string Sequence = "idle";

		public override object Create(ActorInitializer init) { return new D2DistortionTrail(this); }
	}

	public class D2DistortionTrail : ConditionalTrait<D2DistortionTrailInfo>, ITick, INotifyAddedToWorld, IRender
	{
		WPos cachedPosition;
		CPos activeCell;
		WVec movementDirection = new(1024, 0, 0);
		int idleTicks;
		int animationTicks;
		bool hasMoved;
		Sprite sprite;

		// The original Sandworm is drawn three times per frame: at its
		// current position, at its last recent cell, and at the cell
		// before that (see viewport.c's dedicated sandworm block, which
		// issues three separate DRAWSPRITE_FLAG_BLUR calls). Each of those
		// draws advances the same global blur-offset cycle, so the three
		// copies show different shift magnitudes at once -- that, not any
		// per-pixel randomness, is what makes the original read as
		// rippling ground rather than a single patch sliding as a block.
		readonly CPos[] cellHistory = new CPos[2];
		int validHistoryCount;

		public D2DistortionTrail(D2DistortionTrailInfo info)
			: base(info) { }

		protected override void Created(Actor self)
		{
			Reset(self);
			sprite = self.World.Map.Sequences.GetSequence(Info.Image, Info.Sequence).GetSprite(0);
			base.Created(self);
		}

		void ITick.Tick(Actor self)
		{
			if (!self.IsInWorld || IsTraitDisabled)
				return;

			animationTicks++;

			var previousPosition = cachedPosition;
			var currentPosition = self.CenterPosition;
			cachedPosition = currentPosition;

			var currentCell = self.World.Map.CellContaining(currentPosition);
			var previousCell = self.World.Map.CellContaining(previousPosition);

			if (currentPosition == previousPosition)
			{
				if (idleTicks <= Info.Duration)
					idleTicks++;
				return;
			}

			movementDirection = currentPosition - previousPosition;
			idleTicks = 0;
			hasMoved = true;

			if (currentCell != previousCell)
			{
				movementDirection = self.World.Map.CenterOfCell(currentCell) - self.World.Map.CenterOfCell(previousCell);

				for (var i = cellHistory.Length - 1; i > 0; i--)
					cellHistory[i] = cellHistory[i - 1];
				cellHistory[0] = activeCell;
				validHistoryCount = Math.Min(validHistoryCount + 1, cellHistory.Length);
			}

			activeCell = currentCell;
		}

		protected override void TraitEnabled(Actor self)
		{
			Reset(self);
		}

		void INotifyAddedToWorld.AddedToWorld(Actor self)
		{
			Reset(self);
		}

		void Reset(Actor self)
		{
			cachedPosition = self.CenterPosition;
			activeCell = self.World.Map.CellContaining(cachedPosition);
			movementDirection = new WVec(1024, 0, 0);
			idleTicks = Info.Duration + 1;
			animationTicks = 0;
			hasMoved = false;
			validHistoryCount = 0;
			for (var i = 0; i < cellHistory.Length; i++)
				cellHistory[i] = activeCell;
		}

		bool ValidTerrain(Actor self, CPos cell)
		{
			if (!self.World.Map.Contains(cell))
				return false;

			var terrainType = self.World.Map.GetTerrainInfo(cell).Type;
			return Info.TerrainTypes.Count == 0 || Info.TerrainTypes.Contains(terrainType);
		}

		public IEnumerable<IRenderable> Render(Actor self, WorldRenderer wr)
		{
			if (IsTraitDisabled || !hasMoved || idleTicks > Info.Duration)
				yield break;

			// Current position, then up to two recently-occupied cells --
			// mirroring the original's current/targetLast/targetPreLast
			// triple-draw. Each copy gets its own step in the blur cycle
			// via a small Age offset, so they don't all show the identical
			// shift at the same instant.
			// The lead point follows the actor's exact continuous position
			// (matching DuneLegacy's lastLocs_[0], which uses realX_/realY_
			// rather than a tile-snapped coordinate); only the trailing
			// history below is snapped to cell centres.
			if (ValidTerrain(self, activeCell))
			{
				var pos = self.CenterPosition;
				if (!self.World.FogObscures(pos))
					yield return new D2DistortionRenderable(pos, sprite, Info.Style);
			}

			for (var i = 0; i < validHistoryCount; i++)
			{
				var cell = cellHistory[i];
				if (!ValidTerrain(self, cell))
					continue;

				var pos = self.World.Map.CenterOfCell(cell);
				if (self.World.FogObscures(pos))
					continue;

				yield return new D2DistortionRenderable(pos, sprite, Info.Style);
			}
		}

		public IEnumerable<Rectangle> ScreenBounds(Actor self, WorldRenderer wr)
		{
			if (IsTraitDisabled || !hasMoved || idleTicks > Info.Duration)
				yield break;

			if (ValidTerrain(self, activeCell))
			{
				var pos = self.CenterPosition;
				if (!self.World.FogObscures(pos))
					yield return new D2DistortionRenderable(pos, sprite, Info.Style).ScreenBounds(wr);
			}

			for (var i = 0; i < validHistoryCount; i++)
			{
				var cell = cellHistory[i];
				if (!ValidTerrain(self, cell))
					continue;

				var pos = self.World.Map.CenterOfCell(cell);
				if (self.World.FogObscures(pos))
					continue;

				yield return new D2DistortionRenderable(pos, sprite, Info.Style).ScreenBounds(wr);
			}
		}
	}
}
