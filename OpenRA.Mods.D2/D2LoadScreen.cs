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

using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using OpenRA.Graphics;
using OpenRA.Mods.Common.LoadScreens;
using OpenRA.Mods.D2.SpriteLoaders;
using OpenRA.Widgets;

namespace OpenRA.Mods.D2
{
	public sealed class D2LoadScreen : BlankLoadScreen
	{
		Stopwatch lastUpdate = Stopwatch.StartNew();
		Renderer r;

		float2 logoPos;
		SheetBuilder sheetBuilder;
		ISpriteFrame[] frames;
		Sprite logo;
		string[] messages = { "Loading..." };
		ImmutablePalette palette;
		HardwarePalette hardwarePalette;

		public override void Init(ModData modData, Dictionary<string, string> info)
		{
			base.Init(modData, info);

			// Avoid standard loading mechanisms so we
			// can display the loadscreen as early as possible
			r = Game.Renderer;
			if (r == null)
				return;

			if (info.ContainsKey("Text"))
				messages = info["Text"].Split(',');

			if (info.ContainsKey("Palette"))
			{
				using (var stream = modData.DefaultFileSystem.Open(info["Palette"]))
				{
					palette = new ImmutablePalette(stream, new int[] { });
				}

				hardwarePalette = new HardwarePalette();
				hardwarePalette.AddPalette("loadscreen", palette, false);
				hardwarePalette.Initialize();
				r.SetPalette(hardwarePalette);
			}

			if (info.ContainsKey("Image"))
			{
				using (var stream = modData.DefaultFileSystem.Open(info["Image"]))
				{
					CpsD2Loader loader = new CpsD2Loader();
					if (!loader.TryParseSprite(stream, out frames))
						return;
				}

				if (frames.Length == 0)
					return;

				sheetBuilder = new SheetBuilder(SheetType.Indexed, 512);
				logo = sheetBuilder.Add(frames[0]);

				logoPos = new float2((r.Resolution.Width - logo.Size.X) / 2, (r.Resolution.Height - logo.Size.Y) / 2);
			}
		}

		public override void Display()
		{
			if (r == null)
				return;

			// Update text at most every 0.2 seconds
			if (lastUpdate.Elapsed.TotalSeconds < 0.2)
				return;

			if (r.Fonts == null)
				return;

			lastUpdate.Restart();
			var text = messages.Random(Game.CosmeticRandom);
			var textSize = r.Fonts["Bold"].Measure(text);

			r.BeginFrame(int2.Zero, 1f);

			if (logo != null)
				r.SpriteRenderer.DrawSprite(logo, logoPos);

			r.Fonts["Bold"].DrawText(text, new float2(r.Resolution.Width - textSize.X - 20, r.Resolution.Height - textSize.Y - 20), Color.White);
			r.EndFrame(new NullInputHandler());
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && sheetBuilder != null)
				sheetBuilder.Dispose();

			if (disposing && hardwarePalette != null)
				hardwarePalette.Dispose();

			base.Dispose(disposing);
		}
	}
}
