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

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.FileFormats;
using OpenRA.Mods.D2.SpriteLoaders;

namespace OpenRA.Mods.D2.FileFormats
{
	public class WsaReader
	{
		int width;
		int height;
		int currentFrame;
		ISpriteFrame[] frames;

		public int Length { get { return frames == null ? 0 : frames.Length; } }
		public int CurrentFrame { get { return currentFrame; } }
		public ISpriteFrame Frame { get { return currentSpriteFrame(); } }
		public int Width { get { return width; } }
		public int Height { get { return height; } }

		public WsaReader()
		{
			frames = null;
			Reset();
		}

		public WsaReader(Stream stream)
		{
			Read(stream);
			Reset();
		}

		public void Read(Stream stream)
		{
			ISpriteFrame[] videoFrames;
			ISpriteFrame prev = null;
			WsaLoader wsaLoader = new WsaLoader();

			if (frames != null)
				prev = frames[frames.Length-1];

			wsaLoader.TryParseSpriteWithPrevFrame(stream, prev, out videoFrames);

			if (frames == null) {
				frames = videoFrames;

				if (Length > 0) {
					var frame = frames[0];
					width = frame.Size.Width;
					height = frame.Size.Height;
				}
			}
			else
				frames = frames.Concat(videoFrames).ToArray();
		}

		public void Reset()
		{
			currentFrame = 0;
		}

		public void AdvanceFrame()
		{
			if (frames != null && currentFrame < frames.Length - 1)
				currentFrame++;
			else
				currentFrame = 0;
		}

		ISpriteFrame currentSpriteFrame()
		{
			if (frames != null && currentFrame < frames.Length)
				return frames[currentFrame];

			return null;
		}
	}
}
