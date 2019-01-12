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
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Mods.D2.Graphics;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.D2.Traits
{

	public class D2BuildingPlacementInfo : ITraitInfo, IPlaceBuildingDecorationInfo
	{
		public object Create(ActorInitializer init) { return new D2BuildingPlacement(init.Self); }

		public IEnumerable<IRenderable> Render(WorldRenderer wr, World w, ActorInfo ai, WPos centerPosition)
		{
			var bi = ai.TraitInfoOrDefault<D2BuildingInfo>();

			var width = bi.Dimensions.X * 1024;
			var height = bi.Dimensions.Y * 1024;
			var halfWidth = width / 2;
			var halfHeight = height / 2;

			var bounds = new Rectangle(-halfWidth, -halfHeight, width - halfWidth, height - halfHeight);
			var topLeft = new WPos(centerPosition.X - width / 2 + 512, centerPosition.Y - height / 2 + 512, 0);
			var isCloseEnough = bi.IsCloseEnoughToBase(w, w.LocalPlayer, ai, w.Map.CellContaining(topLeft));

			return new IRenderable[] { new D2BuildingPlacementRenderable(centerPosition, bounds, Color.White, !isCloseEnough) };
		}
	}

	public class D2BuildingPlacement
	{
		public D2BuildingPlacement(Actor self) { }
	}
}
