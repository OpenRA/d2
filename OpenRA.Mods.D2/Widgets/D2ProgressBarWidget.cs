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
using OpenRA.Mods.Common.Widgets;
using OpenRA.Primitives;
using OpenRA.Widgets;

namespace OpenRA.Mods.D2.Widgets
{
	public class D2ProgressBarWidget : Widget
	{
		public int BarMargin = 3;

		public int Percentage = 0;

		public Func<int> GetPercentage;

		public D2ProgressBarWidget()
		{
			GetPercentage = () => Percentage;
		}

		protected D2ProgressBarWidget(D2ProgressBarWidget other)
			: base(other)
		{
			Percentage = other.Percentage;
			GetPercentage = other.GetPercentage;
		}

		public override void Draw()
		{
			if (GetPercentage == null)
				return;

			var rb = RenderBounds;
			var r = new Rectangle(rb.Left, rb.Top, rb.Width, rb.Height - 3 * BarMargin);

			var percentage = GetPercentage();

			var backgroundColor = Color.FromArgb(186, 190, 150);

			WidgetUtils.FillRectWithColor(rb, backgroundColor);
			D2WidgetUtils.DrawPanelBorder(r, BarMargin);

			D2WidgetUtils.DrawRedTriangle(new int2(r.Left + BarMargin * 2, r.Bottom), BarMargin);
			D2WidgetUtils.DrawYellowTriangle(new int2((r.Left + r.Right) / 2, r.Bottom), BarMargin);
			D2WidgetUtils.DrawGreenTriangle(new int2(r.Right - 2 * BarMargin, r.Bottom), BarMargin);

			var minBarWidth = 1;
			var maxBarWidth = r.Width - BarMargin * 2;
			var barWidth = percentage * maxBarWidth / 100;
			barWidth = Math.Max(barWidth, minBarWidth);

			var barRect = new Rectangle(r.X + BarMargin, r.Y + BarMargin, barWidth, r.Height - 2 * BarMargin);
			var barColor = Color.FromArgb(85, 254, 81);
			if (percentage < 25)
				barColor = Color.FromArgb(168, 0, 0);
			else if (percentage < 50)
				barColor = Color.FromArgb(254, 251, 84);

			WidgetUtils.FillRectWithColor(barRect, barColor);
		}

		public override Widget Clone() { return new D2ProgressBarWidget(this); }
	}
}
