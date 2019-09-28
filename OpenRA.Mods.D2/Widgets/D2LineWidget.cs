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
using OpenRA.Primitives;
using OpenRA.Widgets;

namespace OpenRA.Mods.D2.Widgets
{
	public class D2LineWidget : Widget
	{
		public int Size = 3;
		public Func<Color> GetColor;

		public D2LineWidget()
		{
			GetColor = () => Color.FromArgb(251, 255, 203);
		}

		protected D2LineWidget(D2LineWidget widget)
			: base(widget)
		{
			GetColor = widget.GetColor;
		}

		public override Widget Clone()
		{
			return new D2LineWidget(this);
		}

		public override void Draw()
		{
			var rb = RenderBounds;
			D2WidgetUtils.DrawLine(
				new float2(rb.Left, rb.Top),
				new float2(rb.Right, rb.Bottom), Size, GetColor());
		}
	}
}
