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
using System.IO;
using OpenRA.Graphics;
using OpenRA.Mods.D2.FileFormats;
using OpenRA.Primitives;

namespace OpenRA.Mods.D2.SpriteLoaders
{
	public class WsaLoader : ISpriteLoader
	{
		int tileWidth;
		int tileHeight;
		public uint Delta;
		uint[] offsets;

		long numTiles;

		class WsaTile : ISpriteFrame
		{
			public SpriteFrameType Type { get { return SpriteFrameType.Indexed; } }
			public Size Size { get; private set; }
			public Size FrameSize { get { return Size; } }
			public float2 Offset { get { return float2.Zero; } }
			public byte[] Data { get; set; }
			public bool DisableExportPadding { get { return false; } }

			public WsaTile(Stream s, Size size, ISpriteFrame prev)
			{
				Size = size;
				var dataLen = s.Length - s.Position;
				Console.WriteLine("dataLen = {0}", dataLen);
				var tempData = StreamExts.ReadBytes(s, (int)dataLen);
				byte[] srcData = new byte[size.Width * size.Height];

				// format80 decompression
				LCWCompression.DecodeInto(tempData, srcData);

				// and format40 decmporession
				Data = new byte[size.Width * size.Height];
				if (prev == null)
					Array.Clear(Data, 0, Data.Length);
				else
					Array.Copy(prev.Data, Data, Data.Length);
				XORDeltaCompression.DecodeInto(srcData, Data, 0);
			}
		}

		bool IsWsa(Stream s)
		{
			if (s.Length < 10)
				return false;

			var start = s.Position;

			numTiles = s.ReadUInt16();
			tileWidth = s.ReadUInt16();
			tileHeight = s.ReadUInt16();
			Delta = s.ReadUInt32();

			offsets = new uint[numTiles + 1];
			for (var i = 0; i <= numTiles; i++)
				offsets[i] = s.ReadUInt32();

			s.Position = start;

			/*
			if (offsets[numTiles] < s.Length)
				return false;
			*/

			if (offsets[0] == 0)
			{
				numTiles -= 1;
				for (var i = 1; i <= numTiles; i++)
					offsets[i - 1] = offsets[i];
			}

			return true;
		}

		WsaTile[] ParseFrames(Stream s, ISpriteFrame prev)
		{
			var start = s.Position;

			var tiles = new WsaTile[numTiles];
			for (var i = 0; i < numTiles; i++)
			{
				s.Position = offsets[i];
				tiles[i] = new WsaTile(s, new Size(tileWidth, tileHeight), (i == 0) ? prev : tiles[i - 1]);
			}

			s.Position = start;
			return tiles;
		}

		public bool TryParseSpriteWithPrevFrame(Stream s, ISpriteFrame prev, out ISpriteFrame[] frames, out TypeDictionary metadata)
		{
			metadata = null;
			if (!IsWsa(s))
			{
				frames = null;
				return false;
			}

			frames = ParseFrames(s, prev);
			return true;
		}

		public bool TryParseSprite(Stream s, out ISpriteFrame[] frames, out TypeDictionary metadata)
		{
			return TryParseSpriteWithPrevFrame(s, null, out frames, out metadata);
		}
	}
}
