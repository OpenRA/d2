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

using System.Drawing;
using System.IO;
using OpenRA;
using OpenRA.FileFormats;
using OpenRA.Graphics;
using OpenRA.Primitives;

namespace OpenRA.Mods.D2.SpriteLoaders
{
	public class IcnD2Loader : ISpriteLoader
	{
		public const int TileWidth = 16;
		public const int TileHeight = 16;
		public const int TileSize = TileWidth * TileHeight / 2;

		uint ssetOffset, ssetLength;
		uint rpalOffset, rpalLength;
		uint rtblOffset, rtblLength;
		uint numTiles;

		byte[] rtbl;
		byte[][] rpal;

		class IcnD2Tile : ISpriteFrame
		{
			public Size Size { get; private set; }
			public Size FrameSize { get { return Size; } }
			public float2 Offset { get { return float2.Zero; } }
			public byte[] Data { get; set; }
			public bool DisableExportPadding { get { return false; } }

			public IcnD2Tile(Stream s, byte[] palette)
			{
				var tile = StreamExts.ReadBytes(s, TileSize);

				Size = new Size(TileWidth, TileHeight);
				Data = new byte[Size.Width * Size.Height];

				var i = 0;
				for (var y = 0; y < TileHeight; y++)
				{
					for (var x = 0; x < TileWidth; x += 2)
					{
						var val = tile[(y * TileWidth + x) / 2];
						Data[i++] = palette[val >> 4];
						Data[i++] = palette[val & 0x0F];
					}
				}
			}
		}

		bool IsIcnD2(Stream s)
		{
			if (s.Length < 0x20)
				return false;

			var start = s.Position;

			s.Position = 0x18;
			if (s.ReadASCII(4) != "SSET")
			{
				s.Position = start;
				return false;
			}

			ssetLength = int2.Swap(s.ReadUInt32()) - 8;
			s.Position += 3;
			ssetOffset = 0x18 + 16;
			if (s.Length < ssetOffset + ssetLength)
			{
				s.Position = start;
				return false;
			}

			s.Position = ssetOffset + ssetLength;
			if (s.ReadASCII(4) != "RPAL")
			{
				s.Position = start;
				return false;
			}

			rpalLength = int2.Swap(s.ReadUInt32());
			rpalOffset = ssetOffset + ssetLength + 8;
			if (s.Length < rpalOffset + rpalLength)
			{
				s.Position = start;
				return false;
			}

			s.Position = rpalOffset + rpalLength;
			if (s.ReadASCII(4) != "RTBL")
			{
				s.Position = start;
				return false;
			}

			rtblLength = int2.Swap(s.ReadUInt32());
			rtblOffset = rpalOffset + rpalLength + 8;

			if (s.Length < rtblOffset + rtblLength)
			{
				s.Position = start;
				return false;
			}

			numTiles = ssetLength / TileSize;

			if (rtblLength < numTiles)
			{
				s.Position = start;
				return false;
			}

			s.Position = start;
			return true;
		}

		void ReadTables(Stream s)
		{
			var start = s.Position;

			s.Position = rtblOffset;
			rtbl = StreamExts.ReadBytes(s, (int)rtblLength);

			s.Position = rpalOffset;
			rpal = new byte[rpalLength / 16][];
			for (var i = 0; i < rpal.Length; i++)
				rpal[i] = StreamExts.ReadBytes(s, 16);

			s.Position = start;
		}

		IcnD2Tile[] ParseFrames(Stream s)
		{
			var start = s.Position;

			ReadTables(s);

			var tiles = new IcnD2Tile[numTiles];
			s.Position = ssetOffset;
			for (var i = 0; i < tiles.Length; i++)
				tiles[i] = new IcnD2Tile(s, rpal[rtbl[i]]);

			s.Position = start;
			return tiles;
		}

		public bool TryParseSprite(Stream s, out ISpriteFrame[] frames, out TypeDictionary metadata)
		{
			metadata = null;
			if (!IsIcnD2(s))
			{
				frames = null;
				return false;
			}

			frames = ParseFrames(s);
			return true;
		}
	}
}
