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
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Primitives;

namespace OpenRA.Mods.D2.Graphics
{
	public enum D2DistortionStyle { Sand, Sonic }

	public readonly struct D2DistortionRenderable : IRenderable, IFinalizedRenderable
	{
		readonly Sprite sprite;
		readonly D2DistortionStyle style;

		public D2DistortionRenderable(WPos pos, Sprite sprite, D2DistortionStyle style)
		{
			Pos = pos;
			this.sprite = sprite;
			this.style = style;
		}

		public WPos Pos { get; }
		public int ZOffset => 0;
		public bool IsDecoration => true;

		public IRenderable WithZOffset(int newOffset) { return this; }
		public IRenderable OffsetBy(in WVec vec)
		{
			return new D2DistortionRenderable(Pos + vec, sprite, style);
		}

		public IRenderable AsDecoration() { return this; }
		public IFinalizedRenderable PrepareRender(WorldRenderer wr)
		{
			// Queue during PrepareRender instead of Render so each renderer has its work ready
			// when its configured post-process pass runs. Sand currently uses AfterActors; sonic
			// remains later on AfterWorld so its wave can bend the completed world image.
			var renderStyle = style;
			var renderer = wr.World.WorldActor.TraitsImplementing<D2DistortionRenderer>().FirstOrDefault(r => r.Accepts(renderStyle));
			if (renderer == null)
				return this;

			renderer.DrawSprite(wr.Screen3DPxPosition(Pos), sprite, style);

			return this;
		}

		public void Render(WorldRenderer wr) { }

		public void RenderDebugGeometry(WorldRenderer wr)
		{
			var bounds = ScreenBounds(wr);
			if (!bounds.IsEmpty)
				Game.Renderer.RgbaColorRenderer.DrawRect(
					new float3(bounds.Left, bounds.Top, 0),
					new float3(bounds.Right, bounds.Bottom, 0), 1, Color.Red);
		}

		public Rectangle ScreenBounds(WorldRenderer wr)
		{
			var center = wr.Viewport.WorldToViewPx(wr.Screen3DPxPosition(Pos));
			var width = (int)Math.Ceiling(sprite.Size.X);
			var height = (int)Math.Ceiling(sprite.Size.Y);
			return new Rectangle(center.X - width / 2, center.Y - height / 2, width, height);
		}
	}
}
