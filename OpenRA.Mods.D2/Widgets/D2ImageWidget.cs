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
using OpenRA.Graphics;
using OpenRA.Mods.Common.Widgets;
using OpenRA.Mods.D2.Graphics;
using OpenRA.Widgets;

namespace OpenRA.Mods.D2.Widgets
{
	public class D2ImageWidget : Widget
	{
		public readonly string TooltipTemplate;
		public readonly string TooltipContainer;

		public string ImageCollection = "";
		public string ImageName = "";
		public string PaletteName = null;
		public bool ClickThrough = true;
		public bool FillBackground = false;
		public Func<string> GetImageName;
		public Func<string> GetImageCollection;
		public Func<string> GetPaletteName;

		readonly World world;
		readonly WorldRenderer worldRenderer;

		[Translate]
		public string TooltipText;

		Lazy<TooltipContainerWidget> tooltipContainer;
		public Func<string> GetTooltipText;

		[ObjectCreator.UseCtor]
		public D2ImageWidget(World world, WorldRenderer worldRenderer)
		{
			this.world = world;
			this.worldRenderer = worldRenderer;

			GetImageName = () => ImageName;
			GetImageCollection = () => ImageCollection;
			GetPaletteName = () => PaletteName;
			GetTooltipText = () => TooltipText;
			tooltipContainer = Exts.Lazy(() =>
				Ui.Root.Get<TooltipContainerWidget>(TooltipContainer));
		}

		protected D2ImageWidget(D2ImageWidget other)
			: base(other)
		{
			ClickThrough = other.ClickThrough;
			ImageName = other.ImageName;
			GetImageName = other.GetImageName;
			ImageCollection = other.ImageCollection;
			GetImageCollection = other.GetImageCollection;
			PaletteName = other.PaletteName;
			GetPaletteName = other.GetPaletteName;
			world = other.world;
			worldRenderer = other.worldRenderer;

			TooltipTemplate = other.TooltipTemplate;
			TooltipContainer = other.TooltipContainer;
			GetTooltipText = other.GetTooltipText;
			tooltipContainer = Exts.Lazy(() =>
				Ui.Root.Get<TooltipContainerWidget>(TooltipContainer));
		}

		public override Widget Clone() { return new D2ImageWidget(this); }

		public override void Draw()
		{
			var name = GetImageName();
			var collection = GetImageCollection();
			var paletteName = GetPaletteName();

			if (paletteName == null || paletteName.Length == 0)
				paletteName = "player" + world.LocalPlayer.InternalName;
			PaletteReference p = null;
			if (paletteName != null)
				p = worldRenderer.Palette(paletteName);

			var sprite = D2ChromeProvider.GetImage(collection, name, p);
			if (sprite == null)
				throw new ArgumentException("Sprite {0}/{1} was not found.".F(collection, name));

			if (FillBackground)
			{
				for (var y = 0; y < Bounds.Height; y += sprite.Bounds.Height)
					for (var x = 0; x < Bounds.Width; x += sprite.Bounds.Width)
						WidgetUtils.DrawRGBA(sprite, RenderOrigin + new int2(x, y));
			}
			else
				WidgetUtils.DrawRGBA(sprite, RenderOrigin);
		}

		public override bool HandleMouseInput(MouseInput mi)
		{
			return !ClickThrough && RenderBounds.Contains(mi.Location);
		}

		public override void MouseEntered()
		{
			if (TooltipContainer == null || GetTooltipText == null)
				return;

			tooltipContainer.Value.SetTooltip(TooltipTemplate, new WidgetArgs() { { "getText", GetTooltipText } });
		}

		public override void MouseExited()
		{
			if (TooltipContainer == null)
				return;

			tooltipContainer.Value.RemoveTooltip();
		}
	}
}
