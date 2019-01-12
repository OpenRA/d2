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
using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Widgets;
using OpenRA.Mods.Common.Graphics;


namespace OpenRA.Mods.D2.Graphics
{
	public struct D2BuildingPlacementRenderable : IRenderable, IFinalizedRenderable
	{
		readonly WPos pos;
		readonly Rectangle decorationBounds;
		readonly Color color;
		readonly bool crossEnabled;

		public D2BuildingPlacementRenderable(Actor actor, Rectangle decorationBounds, Color color, bool crossEnabled)
			: this(actor.CenterPosition, decorationBounds, color, crossEnabled) { }

		public D2BuildingPlacementRenderable(WPos pos, Rectangle decorationBounds, Color color, bool crossEnabled)
		{
			this.pos = pos;
			this.decorationBounds = decorationBounds;
			this.color = color;
			this.crossEnabled = crossEnabled;
		}

		public WPos Pos { get { return pos; } }

		public PaletteReference Palette { get { return null; } }
		public int ZOffset { get { return 0; } }
		public bool IsDecoration { get { return true; } }

		public IRenderable WithPalette(PaletteReference newPalette) { return this; }
		public IRenderable WithZOffset(int newOffset) { return this; }
		public IRenderable OffsetBy(WVec vec) { return new D2BuildingPlacementRenderable(pos + vec, decorationBounds, color, crossEnabled); }
		public IRenderable AsDecoration() { return this; }

		public IFinalizedRenderable PrepareRender(WorldRenderer wr) { return this; }

		public void Render(WorldRenderer wr)
		{
			var tl = new WPos(pos.X + decorationBounds.Left, pos.Y + decorationBounds.Top, 0);
			var br = new WPos(pos.X + decorationBounds.Width, pos.Y + decorationBounds.Height, 0);
			var tlOffset = wr.Screen3DPxPosition(tl);
			var brOffset = wr.Screen3DPxPosition(br);
			var width = 1.0f;// / wr.Viewport.Zoom;
			Game.Renderer.WorldRgbaColorRenderer.DrawRect(tlOffset, brOffset, width, color);
			if (crossEnabled)
			{
				Game.Renderer.WorldRgbaColorRenderer.DrawLine(tlOffset, brOffset, width, color);
				var trOffset = new float3(brOffset.X, tlOffset.Y, tlOffset.Z);
				var blOffset = new float3(tlOffset.X, brOffset.Y, tlOffset.Z);
				Game.Renderer.WorldRgbaColorRenderer.DrawLine(trOffset, blOffset, width, color);
			}
		}

		public void RenderDebugGeometry(WorldRenderer wr) { } 

		public Rectangle ScreenBounds(WorldRenderer wr) { return Rectangle.Empty; }
	}
}
