#region Copyright & License Information
/*
 * Copyright 2007-2018 The d2 mod Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Primitives;

namespace OpenRA.Mods.D2.Widgets
{
	public static class D2WidgetUtils
	{
		public static void DrawLine(float2 start, float2 end, float width, Color color)
		{
			// Offset to the edges of the pixels
			var a = new float2(start.X - 0.5f, start.Y - 0.5f);
			var b = new float2(end.X - 0.5f, end.Y - 0.5f);
			Game.Renderer.RgbaColorRenderer.DrawLine(a, b, width, color);
		}

		public static void DrawDisconnectedLine(IEnumerable<float2> points, float width, Color color)
		{
			using (var e = points.GetEnumerator())
			{
				if (!e.MoveNext())
					return;

				var lastPoint = e.Current;
				while (e.MoveNext())
				{
					var point = e.Current;
					DrawLine(lastPoint, point, width, color);
					lastPoint = point;
				}
			}
		}

		public static void DrawColoredPanelBorder(Rectangle bounds, int size, Color highlightColor, Color shadowColor)
		{
			WidgetUtils.FillRectWithColor(new Rectangle(bounds.Left, bounds.Top, size, bounds.Height - size), highlightColor);
			WidgetUtils.FillRectWithColor(new Rectangle(bounds.Left, bounds.Top, bounds.Width - size, size), highlightColor);

			WidgetUtils.FillRectWithColor(new Rectangle(bounds.Left + size, bounds.Bottom - size, bounds.Width - size, size), shadowColor);
			WidgetUtils.FillRectWithColor(new Rectangle(bounds.Right - size, bounds.Top + size, size, bounds.Height - size), shadowColor);
		}

		public static void DrawPanelBorder(Rectangle bounds, int size, bool inverse = false)
		{
			var highlightColor = Color.FromArgb(251, 255, 203);
			var shadowColor = Color.FromArgb(103, 103, 79);

			DrawColoredPanelBorder(bounds, size, inverse ? shadowColor : highlightColor, inverse ? highlightColor : shadowColor);
		}

		public static void DrawColoredTriangle(int2 a, int size, Color normalColor, Color highlightColor, Color shadowColor)
		{
			WidgetUtils.FillRectWithColor(new Rectangle(a.X - size, a.Y, size, size * 2), highlightColor);

			WidgetUtils.FillRectWithColor(new Rectangle(a.X - size * 2, a.Y + size, size, size), normalColor);
			WidgetUtils.FillRectWithColor(new Rectangle(a.X, a.Y + size, size, size), normalColor);

			WidgetUtils.FillRectWithColor(new Rectangle(a.X, a.Y, size, size), shadowColor);
			WidgetUtils.FillRectWithColor(new Rectangle(a.X + size, a.Y + size, size, size * 2), shadowColor);
			WidgetUtils.FillRectWithColor(new Rectangle(a.X - size * 2, a.Y + size * 2, size * 3, size), shadowColor);
		}

		public static void DrawRedTriangle(int2 a, int size)
		{
			var normalColor = Color.FromArgb(124, 0, 0);
			var highlightColor = Color.FromArgb(180, 0, 0);
			var shadowColor = Color.Black;

			DrawColoredTriangle(a, size, normalColor, highlightColor, shadowColor);
		}

		public static void DrawYellowTriangle(int2 a, int size)
		{
			var normalColor = Color.FromArgb(252, 184, 86);
			var highlightColor = Color.FromArgb(252, 252, 84);
			var shadowColor = Color.Black;

			DrawColoredTriangle(a, size, normalColor, highlightColor, shadowColor);
		}

		public static void DrawGreenTriangle(int2 a, int size)
		{
			var normalColor = Color.FromArgb(0, 168, 0);
			var highlightColor = Color.FromArgb(84, 252, 84);
			var shadowColor = Color.Black;

			DrawColoredTriangle(a, size, normalColor, highlightColor, shadowColor);
		}
	}
}
