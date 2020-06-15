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

		public override bool HandleMouseInput(MouseInput mi)
		{
			if (mi.Button != MouseButton.Right && mi.Button != MouseButton.Left)
				return false;

			if (mi.Event == MouseInputEvent.Down && !TakeMouseFocus(mi))
				return false;

			var disabled = IsDisabled();
			if (HasMouseFocus && mi.Event == MouseInputEvent.Up && mi.MultiTapCount == 2)
			{
				if (!disabled)
				{
					OnDoubleClick();
					return YieldMouseFocus(mi);
				}
			}
			else if (HasMouseFocus && mi.Event == MouseInputEvent.Up)
			{
				// Only fire the onMouseUp event if we successfully lost focus, and were pressed
				if (Depressed && !disabled)
					OnMouseUp(mi);

				return YieldMouseFocus(mi);
			}

			if (mi.Event == MouseInputEvent.Down)
			{
				// OnMouseDown returns false if the button shouldn't be pressed
				if (!disabled)
				{
					OnMouseDown(mi);
					Depressed = true;
					Game.Sound.PlayNotification(ModRules, null, "Sounds", ClickSound, null);
				}
				else
				{
					YieldMouseFocus(mi);
					Game.Sound.PlayNotification(ModRules, null, "Sounds", ClickDisabledSound, null);
				}
			}
			else if (mi.Event == MouseInputEvent.Move && HasMouseFocus)
				Depressed = RenderBounds.Contains(mi.Location);

			return Depressed;
		}

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
