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

/* Based on ChromeProvider */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenRA.FileSystem;
using OpenRA.Graphics;
using OpenRA.Primitives;

namespace OpenRA.Mods.D2.Graphics
{
	public static class D2ChromeProvider
	{
		public class D2Collection
		{
			public readonly string SpriteImage = null;
			public readonly string Palette = null;

			public readonly Dictionary<string, Rectangle> Regions = new Dictionary<string, Rectangle>();
		}

		public static IReadOnlyDictionary<string, D2Collection> Collections { get; private set; }

		static Dictionary<string, D2Collection> collections;
		static Dictionary<D2Collection, Sheet> cachedCollectionSheets;
		static Dictionary<string, Sheet> cachedSheets;
		static Dictionary<string, Sprite> cachedSprites;

		static IReadOnlyFileSystem fileSystem;

		public static void Initialize(ModData modData)
		{
			Deinitialize();

			fileSystem = modData.DefaultFileSystem;

			collections = new Dictionary<string, D2Collection>();
			Collections = new ReadOnlyDictionary<string, D2Collection>(collections);

			cachedCollectionSheets = new Dictionary<D2Collection, Sheet>();
			cachedSheets = new Dictionary<string, Sheet>();
			cachedSprites = new Dictionary<string, Sprite>();

			var chrome = MiniYaml.Merge(modData.Manifest.Chrome
				.Select(s => MiniYaml.FromStream(fileSystem.Open(s), s)));

			foreach (var c in chrome)
				if (!c.Key.StartsWith("^", StringComparison.Ordinal))
					LoadCollection(c.Key, c.Value);
		}

		public static void Deinitialize()
		{
			collections = null;

			if (cachedSheets != null)
				foreach (var sheet in cachedSheets.Values)
					sheet.Dispose();

			cachedCollectionSheets = null;
			cachedSheets = null;
			cachedSprites = null;
		}

		static void LoadCollection(string name, MiniYaml yaml)
		{
			var collection = FieldLoader.Load<D2Collection>(yaml);
			if (collection.SpriteImage != null)
				collections.Add(name, collection);
		}

		static byte[] IndexedSpriteFrameToData(ISpriteFrame frame, PaletteReference p)
		{
			byte[] data = new byte[4 * frame.FrameSize.Width * frame.FrameSize.Height];

			IPalette palette = p.Palette;
			int k = 0;
			int offset = 0;
			byte[] frameData = frame.Data;
			for (int y = 0; y < frame.FrameSize.Height; y++)
			{
				for (int x = 0; x < frame.FrameSize.Width; x++)
				{
					Color color = Color.FromArgb(palette[frameData[k++]]);
					data[offset++] = color.R;
					data[offset++] = color.G;
					data[offset++] = color.B;
					data[offset++] = color.A;
				}
			}

			return data;
		}

		static Sheet SpriteFrameToSheet(ISpriteFrame frame, PaletteReference p)
		{
			var size = Exts.NextPowerOf2(Math.Max(frame.FrameSize.Width, frame.FrameSize.Height));
			SheetBuilder sheetBuilder = new SheetBuilder(SheetType.BGRA, size);

			byte[] data;
			if (frame.Type == SpriteFrameType.Indexed)
				data = IndexedSpriteFrameToData(frame, p);
			else
				data = frame.Data;

			var sprite = sheetBuilder.Add(data, frame.FrameSize);
			return sprite.Sheet;
		}

		static ISpriteFrame LoadSpriteFrame(Stream stream)
		{
			foreach (ISpriteLoader loader in Game.ModData.SpriteLoaders)
			{
				ISpriteFrame[] frames;
				TypeDictionary metadata;
				if (loader.TryParseSprite(stream, out frames, out metadata))
				{
					if (frames.Length > 0)
					{
						// Will use only first frame if multiple frames available
						return frames[0];
					}
				}
			}

			return null;
		}

		static ISpriteFrame LoadSpriteFrame(string imageName)
		{
			// TODO: cache SpriteFrame to not load again?
			using (var stream = fileSystem.Open(imageName))
				return LoadSpriteFrame(stream);
		}

		static Sheet SheetForCollection(D2Collection c, PaletteReference p = null)
		{
			Sheet sheet;

			// Outer cache avoids recalculating image names
			if (!cachedCollectionSheets.TryGetValue(c, out sheet))
			{
				var imageName = c.SpriteImage;
				var paletteName = p == null ? "null" : p.Name;
				var cacheName = imageName + "|" + paletteName;

				// Inner cache makes sure we share sheets between collections
				if (!cachedSheets.TryGetValue(cacheName, out sheet))
				{
					var frame = LoadSpriteFrame(imageName);
					if (frame == null)
					{
						Log.Write("debug", "Could not find sprite image '{0}'", imageName);
						return null;
					}

					sheet = SpriteFrameToSheet(frame, p);
					cachedSheets.Add(cacheName, sheet);
				}

				cachedCollectionSheets.Add(c, sheet);
			}

			return sheet;
		}

		public static Sprite GetImage(string collectionName, string imageName, PaletteReference p)
		{
			if (string.IsNullOrEmpty(collectionName) || string.IsNullOrEmpty(imageName))
				return null;

			// Cached sprite
			var paletteName = p == null ? "null" : p.Name;
			var cacheName = collectionName + "|" + imageName + "|" + paletteName;
			Sprite sprite = null;
			if (cachedSprites.TryGetValue(cacheName, out sprite))
				return sprite;

			D2Collection collection;
			if (!collections.TryGetValue(collectionName, out collection))
			{
				Log.Write("debug", "Could not find collection '{0}'", collectionName);
				return null;
			}

			Rectangle mi;
			if (!collection.Regions.TryGetValue(imageName, out mi))
				return null;

			var sheet = SheetForCollection(collection, p);
			if (sheet == null)
				return null;

			var image = new Sprite(sheet, mi, TextureChannel.RGBA, 1f);
			cachedSprites.Add(cacheName, image);

			return image;
		}
	}
}
