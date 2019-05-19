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
using System.Drawing;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Lint;
using OpenRA.Mods.Common.Orders;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Network;
using OpenRA.Widgets;

namespace OpenRA.Mods.D2.Widgets
{
	public class D2SpriteWidget : Widget
	{
		public readonly World World;

		public Sprite Sprite = null;
		public PaletteReference Palette = null;
		public float2 Offset = float2.Zero;
		public float Scale = 1.0f;

		readonly WorldRenderer worldRenderer;

		[ObjectCreator.UseCtor]
		public D2SpriteWidget(World world, WorldRenderer worldRenderer)
		{
			World = world;
			this.worldRenderer = worldRenderer;
		}

		public override void Draw()
		{
			if (Sprite == null)
				return;

			var pos = new float2(RenderBounds.Location);
			var f = Scale / 2.0f;
			var center = new float2(Sprite.Size.X * f, Sprite.Size.Y * f);

			WidgetUtils.DrawSHPCentered(Sprite, pos + center + Offset, Palette, Scale);
		}
	}
}
