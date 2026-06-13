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
	public readonly struct D2BuildingPlacementRenderable : IRenderable, IFinalizedRenderable
	{
		readonly Rectangle bounds;
		readonly Color color;
		readonly bool crossEnabled;

		public D2BuildingPlacementRenderable(Actor actor, Rectangle bounds, Color color, bool crossEnabled)
			: this(actor.CenterPosition, bounds, color, crossEnabled) { }

		public D2BuildingPlacementRenderable(WPos pos, Rectangle bounds, Color color, bool crossEnabled)
		{
			Pos = pos;
			this.bounds = bounds;
			this.color = color;
			this.crossEnabled = crossEnabled;
		}

		public WPos Pos { get; }

		public PaletteReference Palette { get { return null; } }
		public int ZOffset { get { return 0; } }
		public bool IsDecoration { get { return true; } }

		public IRenderable WithPalette(PaletteReference newPalette) { return this; }
		public IRenderable WithZOffset(int newOffset) { return this; }
		public IRenderable OffsetBy(in WVec vec) { return new D2BuildingPlacementRenderable(Pos + vec, bounds, color, crossEnabled); }
		public IRenderable AsDecoration() { return this; }

		public IFinalizedRenderable PrepareRender(WorldRenderer wr) { return this; }

		public void Render(WorldRenderer wr)
		{
			var rect = ScreenBounds(wr);
			var tl = new float3(rect.Left, rect.Top, 1024.0f);
			var br = new float3(rect.Right, rect.Bottom, 1024.0f);
			const float Width = 1.0f;
			Game.Renderer.WorldRgbaColorRenderer.DrawRect(tl, br, Width, color);
			if (crossEnabled)
			{
				Game.Renderer.WorldRgbaColorRenderer.DrawLine(tl, br, Width, color);
				var tr = new float3(br.X, tl.Y, 1024.0f);
				var bl = new float3(tl.X, br.Y, 1024.0f);
				Game.Renderer.WorldRgbaColorRenderer.DrawLine(tr, bl, Width, color);
			}
		}

		public void RenderDebugGeometry(WorldRenderer wr) { }

		public Rectangle ScreenBounds(WorldRenderer wr)
		{
			var tl = new WPos(Pos.X + bounds.Left, Pos.Y + bounds.Top - 1, 0);
			var br = new WPos(Pos.X + bounds.Right, Pos.Y + bounds.Bottom - 1, 0);
			var tlOffset = wr.ScreenPxPosition(tl);
			var brOffset = wr.ScreenPxPosition(br);

			return new Rectangle(tlOffset.X, tlOffset.Y, brOffset.X - tlOffset.X, brOffset.Y - tlOffset.Y);
		}
	}
}
