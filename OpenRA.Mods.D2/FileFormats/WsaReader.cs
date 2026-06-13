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
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.D2.SpriteLoaders;
using OpenRA.Primitives;

namespace OpenRA.Mods.D2.FileFormats
{
	public class WsaReader
	{
		ISpriteFrame[] frames;

		public int Length { get { return frames == null ? 0 : frames.Length; } }
		public int CurrentFrame { get; private set; }
		public ISpriteFrame Frame { get { return CurrentSpriteFrame(); } }
		public int Width { get; private set; }
		public int Height { get; private set; }

		public WsaReader()
		{
			frames = null;
			Reset();
		}

		public WsaReader(Stream stream)
		{
			Read(stream, out _);
			Reset();
		}

		public void Read(Stream stream, out TypeDictionary metadata)
		{
			ISpriteFrame prev = null;
			var wsaLoader = new WsaLoader();

			if (frames != null)
				prev = frames[^1];

			wsaLoader.TryParseSpriteWithPrevFrame(stream, prev, out var videoFrames, out metadata);

			if (frames == null)
			{
				frames = videoFrames;

				if (Length > 0)
				{
					var frame = frames[0];
					Width = frame.Size.Width;
					Height = frame.Size.Height;
				}
			}
			else
				frames = frames.Concat(videoFrames).ToArray();
		}

		public void Reset()
		{
			CurrentFrame = 0;
		}

		public void AdvanceFrame()
		{
			if (frames != null && CurrentFrame < frames.Length - 1)
				CurrentFrame++;
			else
				CurrentFrame = 0;
		}

		ISpriteFrame CurrentSpriteFrame()
		{
			if (frames != null && CurrentFrame < frames.Length)
				return frames[CurrentFrame];

			return null;
		}
	}
}
