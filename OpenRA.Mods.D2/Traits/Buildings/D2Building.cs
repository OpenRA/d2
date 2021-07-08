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

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.D2.Traits
{
	public class D2BuildingInfo : BuildingInfo
	{
		[Desc("All cells should be explored")]
		public readonly bool AllCellsShouldBeExplored = true;

		[Desc("Any cell should be explored")]
		public readonly bool AnyCellShouldBeExplored = false;

		[Desc("All cells should be not hidden by fog")]
		public readonly bool AllCellsShouldBeVisible = false;

		[Desc("Any cell should be not hidden by fog")]
		public readonly bool AnyCellShouldBeVisible = true;

		public override object Create(ActorInitializer init) { return new Building(init, this); }

		public override bool IsCloseEnoughToBase(World world, Player p, ActorInfo ai, CPos topLeft)
		{
			if (base.IsCloseEnoughToBase(world, p, ai, topLeft))
				return true;

			var requiresBuildableArea = ai.TraitInfoOrDefault<RequiresBuildableAreaInfo>();
			var mapBuildRadius = world.WorldActor.Trait<MapBuildRadius>();

			if (requiresBuildableArea == null || p.PlayerActor.Trait<DeveloperMode>().BuildAnywhere)
				return true;

			if (mapBuildRadius.BuildRadiusEnabled && RequiresBaseProvider && FindBaseProvider(world, p, topLeft) == null)
				return false;

			var adjacent = requiresBuildableArea.Adjacent;
			var buildingMaxBounds = Dimensions;

			var scanStart = world.Map.Clamp(topLeft - new CVec(adjacent, adjacent));
			var scanEnd = world.Map.Clamp(topLeft + buildingMaxBounds + new CVec(adjacent, adjacent));

			var nearnessCandidates = new List<CPos>();
			var allyBuildEnabled = mapBuildRadius.AllyBuildRadiusEnabled;

			var co = world.WorldActor.Trait<D2ConcreteOwners>();
			var shroud = p.Shroud;

			var hasVisibleCells = false;
			var hasExploredCells = false;

			for (var y = topLeft.Y; y < topLeft.Y + Dimensions.Y; y++)
			{
				for (var x = topLeft.X; x < topLeft.X + Dimensions.X; x++)
				{
					var pos = new CPos(x, y);

					if (!shroud.IsExplored(pos))
					{
						if (AllCellsShouldBeExplored)
							return false;
					}
					else
						hasExploredCells = true;

					if (!shroud.IsVisible(pos))
					{
						if (AllCellsShouldBeVisible)
							return false;
					}
					else
						hasVisibleCells = true;
				}
			}

			if (AnyCellShouldBeExplored && !hasExploredCells)
				return false;
			if (AnyCellShouldBeVisible && !hasVisibleCells)
				return false;

			for (var y = scanStart.Y; y < scanEnd.Y; y++)
			{
				for (var x = scanStart.X; x < scanEnd.X; x++)
				{
					var pos = new CPos(x, y);

					if (shroud.IsExplored(pos) && shroud.IsVisible(pos))
					{
						var ownerAtPos = co.GetOwnerAt(pos);

						if (ownerAtPos != null && (ownerAtPos == p || (allyBuildEnabled && ownerAtPos.RelationshipWith(p) == PlayerRelationship.Ally)))
							nearnessCandidates.Add(pos);
					}
				}
			}

			var buildingTiles = Tiles(topLeft).ToList();
			return nearnessCandidates
				.Any(a => buildingTiles
					.Any(b => Math.Abs(a.X - b.X) <= adjacent
						&& Math.Abs(a.Y - b.Y) <= adjacent));
		}
	}
}
