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
using OpenRA.Mods.Common.Widgets;
using OpenRA.Primitives;
using OpenRA.Widgets;

namespace OpenRA.Mods.D2.Widgets
{
	public class D2ButtonWidget : ButtonWidget
	{
		public int BarMargin = 3;

		public Func<Color> GetBackgroundColor;

		[ObjectCreator.UseCtor]
		public D2ButtonWidget(ModData modData)
			: base(modData)
		{
			GetBackgroundColor = () => Color.FromArgb(186, 190, 150);
		}

		protected D2ButtonWidget(D2ButtonWidget other)
			: base(other)
		{
			GetBackgroundColor = other.GetBackgroundColor;
		}

		public override Widget Clone() { return new D2ButtonWidget(this); }

		public override void DrawBackground(Rectangle rect, bool disabled, bool pressed, bool hover, bool highlighted)
		{
			D2DrawBackground(GetBackgroundColor(), BarMargin, rect, disabled, pressed, hover, highlighted);
		}

		public static void D2DrawBackground(Color color, int margin, Rectangle rect, bool disabled, bool pressed, bool hover, bool highlighted)
		{
			WidgetUtils.FillRectWithColor(rect, color);
			D2WidgetUtils.DrawPanelBorder(rect, margin, pressed || highlighted);
		}
	}
}
