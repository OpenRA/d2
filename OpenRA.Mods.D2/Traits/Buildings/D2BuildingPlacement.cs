#region Copyright & License Information
/*
 * Copyright 2007-2018 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.D2.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.D2.Traits
{

	public class D2BuildingPlacementInfo : ITraitInfo, IPlaceBuildingDecorationInfo
	{
		float t = 0.0f;
		float step = 0.03f;

		public object Create(ActorInitializer init) { return new D2BuildingPlacement(init.Self); }

		public Rectangle CellBounds(ActorInfo ai)
		{
			var bi = ai.TraitInfoOrDefault<D2BuildingInfo>();

			var cols = bi.Dimensions.X;
			var rows = bi.Dimensions.Y;

			var width = cols * 1024;
			var height = rows * 1024;
			var halfWidth = width / 2;
			var halfHeight = height / 2;

			return new Rectangle(-halfWidth, -halfHeight, width - halfWidth - 1, height - halfHeight - 1);
		}

		public bool IsCloseEnoughAndBuildable(World w, ActorInfo ai, WPos centerPosition)
		{
			var bi = ai.TraitInfoOrDefault<D2BuildingInfo>();

			var cols = bi.Dimensions.X;
			var rows = bi.Dimensions.Y;

			var width = cols * 1024;
			var height = rows * 1024;

			var bounds = CellBounds(ai);
			var topLeft = new WPos(centerPosition.X - width / 2 + 512, centerPosition.Y - height / 2 + 512, 0);
			var topLeftCell = w.Map.CellContaining(topLeft);
			var isCloseEnough = bi.IsCloseEnoughToBase(w, w.LocalPlayer, ai, topLeftCell);
			var isBuildable = true;

			for (var y = 0; y < rows; y++)
			{
				for (var x = 0; x < cols; x++)
				{
					var cellPos = new CPos(topLeftCell.X + x, topLeftCell.Y + y);
					if (!w.IsCellBuildable(cellPos, ai, bi))
					{
						isBuildable = false;
						break;
					}
				}
				if (!isBuildable)
				{
					break;
				}
			}

			return isCloseEnough && isBuildable;
		}

		void Tick()
		{
			t += step;
			if (t >= 1.0f || t <= 0) step = -step;
			if (t > 1.0f) t = 1.0f;
			if (t < 0.0f) t = 0.0f;
		}

		public IEnumerable<IRenderable> Render(WorldRenderer wr, World w, ActorInfo ai, WPos centerPosition)
		{
			Tick();

			var colorComponent = (int)Math.Round(127 + 127 * t);
			var color = Color.FromArgb(255, colorComponent, colorComponent, colorComponent);

			var cross = !IsCloseEnoughAndBuildable(w, ai, centerPosition);

			return new IRenderable[] { new D2BuildingPlacementRenderable(centerPosition, CellBounds(ai), color, cross) };
		}

	}

	public class D2BuildingPlacement
	{
		public D2BuildingPlacement(Actor self) { }
	}
}
