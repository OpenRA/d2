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

using System.Collections.Generic;
using System.Diagnostics;
using OpenRA.Graphics;
using OpenRA.Mods.Common.LoadScreens;
using OpenRA.Mods.D2.Graphics;
using OpenRA.Mods.D2.ImportData;
using OpenRA.Mods.D2.SpriteLoaders;
using OpenRA.Primitives;

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
		PaletteReference pr;

		ModData modData;
		Dictionary<string, string> info;

		void ImportOriginalMaps()
		{
			D2ImportOriginalMaps.ImportOriginalMaps(modData, info);

			// run only once
			Game.OnShellmapLoaded -= ImportOriginalMaps;
		}

		void Done()
		{
			D2ChromeProvider.Deinitialize();
		}

		public override void Init(ModData modData, Dictionary<string, string> info)
		{
			base.Init(modData, info);

			Game.OnQuit += Done;

			// Can't find better place for initialization
			D2ChromeProvider.Initialize(modData);

			this.modData = modData;
			this.info = info;

			/*
			 * Unpack files needed, because in some PAK files, some VOC files can have prefix 'Z'
			 * Unpacking files will unpack such files and rename. so no modifications in yaml needed.
			 * LoadScreen.Init, possibly not the best place to do this, but need to do that early, before
			 * data will be used. and do this in LoadScreen.Init just works fine.
			 */
			if (D2UnpackContent.UnpackFiles(modData, info) > 0)
			{
				// Some files unpacked. need to reload mod packages
				modData.ModFiles.LoadFromManifest(modData.Manifest);
			}

			/*
			 * Like for unpack files, OpenRA engine do not have proper place for import maps.
			 * And can't import in LoadScreen.Init, because engine not ready.
			 * but Game.OnShellmapLoaded just works.
			 */
			Game.OnShellmapLoaded += ImportOriginalMaps;

			// Avoid standard loading mechanisms so it possible to display the loadscreen as early as possible
			r = Game.Renderer;
			if (r == null)
				return;

			if (info.ContainsKey("Text"))
				messages = info["Text"].Split(',');

			if (info.ContainsKey("Palette"))
			{
				using (var stream = modData.DefaultFileSystem.Open(info["Palette"]))
					palette = new ImmutablePalette(stream, new int[] { });

				hardwarePalette = new HardwarePalette();
				hardwarePalette.AddPalette("loadscreen", palette, false);
				hardwarePalette.Initialize();
				r.SetPalette(hardwarePalette);
				var pal = hardwarePalette.GetPalette("loadscreen");
				pr = new PaletteReference("loadscreenref", hardwarePalette.GetPaletteIndex("loadscreen"), pal, hardwarePalette);
			}

			if (info.ContainsKey("Image"))
			{
				using (var stream = modData.DefaultFileSystem.Open(info["Image"]))
				{
					CpsD2Loader loader = new CpsD2Loader();
					TypeDictionary metadata;

					if (!loader.TryParseSprite(stream, out frames, out metadata))
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

			// Render blank screen
			r.BeginUI();

			if (logo != null)
				r.SpriteRenderer.DrawSprite(logo, logoPos, pr, logo.Size);

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
