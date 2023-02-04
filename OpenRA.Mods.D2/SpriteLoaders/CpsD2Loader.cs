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

using System.IO;
using OpenRA.Graphics;
using OpenRA.Mods.D2.FileFormats;
using OpenRA.Primitives;

namespace OpenRA.Mods.D2.SpriteLoaders
{
	public class CpsD2Loader : ISpriteLoader
	{
		public const int TileWidth = 320;
		public const int TileHeight = 200;

		public const int TileSize = TileWidth * TileHeight;
		const int NumTiles = 1;
		uint palSize;

		class CpsD2Tile : ISpriteFrame
		{
			public SpriteFrameType Type { get { return SpriteFrameType.Indexed8; } }
			public Size Size { get; private set; }
			public Size FrameSize { get { return Size; } }
			public float2 Offset { get { return float2.Zero; } }
			public byte[] Data { get; set; }
			public bool DisableExportPadding { get { return false; } }

			public CpsD2Tile(Stream s)
			{
				Size = new Size(TileWidth, TileHeight);
				var tempData = StreamExts.ReadBytes(s, (int)(s.Length - s.Position));
				Data = new byte[TileSize];
				LCWCompression.DecodeInto(tempData, Data);
			}
		}

		bool IsCpsD2(Stream s)
		{
			if (s.Length < 10)
				return false;

			var start = s.Position;

			s.Position += 2;

			var format = s.ReadUInt16();
			if (format != 0x0004)
			{
				s.Position = start;
				return false;
			}

			var sizeXTimeSizeY = s.ReadUInt16();
			sizeXTimeSizeY += s.ReadUInt16();
			if (sizeXTimeSizeY != TileSize)
			{
				s.Position = start;
				return false;
			}

			palSize = s.ReadUInt16();

			s.Position = start;
			return true;
		}

		CpsD2Tile[] ParseFrames(Stream s)
		{
			var start = s.Position;

			s.Position += 10;
			s.Position += palSize;

			var tiles = new CpsD2Tile[NumTiles];
			for (var i = 0; i < tiles.Length; i++)
				tiles[i] = new CpsD2Tile(s);

			s.Position = start;
			return tiles;
		}

		public bool TryParseSprite(Stream s, string filename, out ISpriteFrame[] frames, out TypeDictionary metadata)
		{
			metadata = null;
			if (!IsCpsD2(s))
			{
				frames = null;
				return false;
			}

			s.Position = 0;
			frames = ParseFrames(s);
			return true;
		}
	}
}
