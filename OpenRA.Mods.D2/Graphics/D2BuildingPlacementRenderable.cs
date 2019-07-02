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

using OpenRA.Graphics;
using OpenRA.Primitives;

namespace OpenRA.Mods.D2.Graphics
{
	public struct D2BuildingPlacementRenderable : IRenderable, IFinalizedRenderable
	{
		readonly WPos pos;
		readonly Rectangle bounds;
		readonly Color color;
		readonly bool crossEnabled;

		public D2BuildingPlacementRenderable(Actor actor, Rectangle bounds, Color color, bool crossEnabled)
			: this(actor.CenterPosition, bounds, color, crossEnabled) { }

		public D2BuildingPlacementRenderable(WPos pos, Rectangle bounds, Color color, bool crossEnabled)
		{
			this.pos = pos;
			this.bounds = bounds;
			this.color = color;
			this.crossEnabled = crossEnabled;
		}

		public WPos Pos { get { return pos; } }

		public PaletteReference Palette { get { return null; } }
		public int ZOffset { get { return 0; } }
		public bool IsDecoration { get { return true; } }

		public IRenderable WithPalette(PaletteReference newPalette) { return this; }
		public IRenderable WithZOffset(int newOffset) { return this; }
		public IRenderable OffsetBy(WVec vec) { return new D2BuildingPlacementRenderable(pos + vec, bounds, color, crossEnabled); }
		public IRenderable AsDecoration() { return this; }

		public IFinalizedRenderable PrepareRender(WorldRenderer wr) { return this; }

		public void Render(WorldRenderer wr)
		{
			var rect = ScreenBounds(wr);
			var tl = new int2(rect.Left, rect.Top);
			var br = new int2(rect.Right, rect.Bottom);
			var width = 1.0f;
			Game.Renderer.WorldRgbaColorRenderer.DrawRect(tl, br, width, color);
			if (crossEnabled)
			{
				Game.Renderer.WorldRgbaColorRenderer.DrawLine(tl, br, width, color);
				var tr = new int2(br.X, tl.Y);
				var bl = new int2(tl.X, br.Y);
				Game.Renderer.WorldRgbaColorRenderer.DrawLine(tr, bl, width, color);
			}
		}

		public void RenderDebugGeometry(WorldRenderer wr) { } 

		public Rectangle ScreenBounds(WorldRenderer wr)
		{
			var tl = new WPos(pos.X + bounds.Left, pos.Y + bounds.Top - 1, 0);
			var br = new WPos(pos.X + bounds.Right, pos.Y + bounds.Bottom - 1, 0);
			var tlOffset = wr.ScreenPxPosition(tl);
			var brOffset = wr.ScreenPxPosition(br);

			return new Rectangle(tlOffset.X, tlOffset.Y, brOffset.X - tlOffset.X, brOffset.Y - tlOffset.Y);
		}
	}
}
