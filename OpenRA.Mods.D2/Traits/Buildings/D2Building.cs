#region Copyright & License Information
/*
 * Copyright 2007-2017 The OpenRA Developers (see AUTHORS)
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
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
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
		public readonly bool AllCellsShouldBeVisible = true;

		[Desc("Any cell should be not hidden by fog")]
		public readonly bool AnyCellShouldBeVisible = false;

		public override object Create(ActorInitializer init) { return new Building(init, this); }

		public override bool IsCloseEnoughToBase(World world, Player p, string buildingName, CPos topLeft)
		{
			if (base.IsCloseEnoughToBase(world, p, buildingName, topLeft))
				return true;

			var shroud = p.Shroud;

			var hasVisibleCells = false;
			var hasExploredCells = false;
			for (var y = topLeft.Y; y < topLeft.Y + Dimensions.Y; y++)
			{
				for (var x = topLeft.X; x < topLeft.X + Dimensions.X; x++)
				{
					var pos = new CPos(x, y);

					if (!shroud.IsExplored(pos)) {
						if (AllCellsShouldBeExplored)
							return false;
					} else {
						hasExploredCells = true;
					}

					if (!shroud.IsVisible(pos)) {
						if (AllCellsShouldBeVisible)
							return false;
					} else {
						hasVisibleCells = true;
					}
				}
			}
			if (AnyCellShouldBeExplored && !hasExploredCells)
				return false;
			if (AnyCellShouldBeVisible && !hasVisibleCells)
				return false;

			var scanStart = world.Map.Clamp(topLeft - new CVec(Adjacent, Adjacent));
			var scanEnd = world.Map.Clamp(topLeft + Dimensions + new CVec(Adjacent, Adjacent));

			var nearnessCandidates = new List<CPos>();
			var co = world.WorldActor.Trait<D2ConcreteOwners>();
			var allyBuildEnabled = world.WorldActor.Trait<MapBuildRadius>().AllyBuildRadiusEnabled;

			for (var y = scanStart.Y; y < scanEnd.Y; y++)
			{
				for (var x = scanStart.X; x < scanEnd.X; x++)
				{
					var pos = new CPos(x, y);

					if(shroud.IsExplored(pos) && shroud.IsVisible(pos))
					{
						var ownerAtPos = co.GetOwnerAt(pos);

						if (ownerAtPos != null && (ownerAtPos == p || (allyBuildEnabled && ownerAtPos.Stances[p] == Stance.Ally)))
							nearnessCandidates.Add(pos);
					}
				}
			}

			var buildingTiles = FootprintUtils.Tiles(world.Map.Rules, buildingName, this, topLeft).ToList();
			return nearnessCandidates
				.Any(a => buildingTiles
					.Any(b => Math.Abs(a.X - b.X) <= Adjacent
						&& Math.Abs(a.Y - b.Y) <= Adjacent));

		}

	}
}
