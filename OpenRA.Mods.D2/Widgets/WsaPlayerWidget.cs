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
using OpenRA.Mods.D2.FileFormats;
using OpenRA.Widgets;

namespace OpenRA.Mods.D2.Widgets
{
	public class WsaPlayerWidget : Widget
	{
		public Hotkey CancelKey = new Hotkey(Keycode.ESCAPE, Modifiers.None);
		public float AspectRatio = 1.2f;
		public bool Skippable = true;

		public bool Paused { get { return paused; } }
		public WsaReader Video { get { return video; } }

		WsaReader video = null;
		string cachedVideo;
		float2 videoOrigin, videoSize;
		bool stopped;
		bool paused;

		ImmutablePalette palette;
		HardwarePalette hardwarePalette;
		PaletteReference pr;

		Action onComplete;

		public WsaPlayerWidget()
		{
			LoadPalette();
		}

		public void Load(string filename)
		{
			if (filename == cachedVideo)
				return;
			var video = new WsaReader(Game.ModData.DefaultFileSystem.Open(filename));
			cachedVideo = filename;
			Open(video);
		}

		void LoadPalette()
		{
			using (var stream = Game.ModData.DefaultFileSystem.Open("IBM.PAL"))
				palette = new ImmutablePalette(stream, new int[] { }, new int[] { });

			hardwarePalette = new HardwarePalette();
			hardwarePalette.AddPalette("chrome", palette, false);
			hardwarePalette.Initialize();
			Game.Renderer.SetPalette(hardwarePalette);
			var pal = hardwarePalette.GetPalette("chrome");
			pr = new PaletteReference("chromeref", hardwarePalette.GetPaletteIndex("chrome"), pal, hardwarePalette);
		}

		public void Open(WsaReader video)
		{
			this.video = video;

			stopped = true;
			paused = true;
			onComplete = () => { };

			var size = Math.Max(video.Width, video.Height);
			var textureSize = Exts.NextPowerOf2(size);

			var scale = Math.Min((float)RenderBounds.Width / video.Width, (float)RenderBounds.Height / video.Height * AspectRatio);
			videoOrigin = new float2(
				RenderBounds.X + (RenderBounds.Width - scale * video.Width) / 2,
				RenderBounds.Y + (RenderBounds.Height - scale * video.Height * AspectRatio) / 2);

			// Round size to integer pixels. Round up to be consistent with the scale calculation.
			videoSize = new float2((int)Math.Ceiling(video.Width * scale), (int)Math.Ceiling(video.Height * AspectRatio * scale));
		}

		public override void Draw()
		{
			if (video == null)
				return;

			var sheetBuilder = new SheetBuilder(SheetType.Indexed, 512);
			var videoSprite = sheetBuilder.Add(video.Frame);

			Game.Renderer.EnableScissor(RenderBounds);
			Game.Renderer.RgbaColorRenderer.FillRect(
				new float2(RenderBounds.Left, RenderBounds.Top),
				new float2(RenderBounds.Right, RenderBounds.Bottom), OpenRA.Primitives.Color.Black);
			Game.Renderer.DisableScissor();

			Game.Renderer.SpriteRenderer.DrawSprite(videoSprite, pr, videoOrigin);
/*
			Game.Renderer.RgbaSpriteRenderer.DrawSprite(
				videoSprite,
				videoOrigin,
				videoSize);
*/

			if (!stopped && !paused)
			{
				if (video.CurrentFrame >= video.Length - 1)
				{
					Stop();
					return;
				}

				video.AdvanceFrame();
			}
		}

		public override bool HandleKeyPress(KeyInput e)
		{
			if (Hotkey.FromKeyInput(e) != CancelKey || e.Event != KeyInputEvent.Down || !Skippable)
				return false;

			Stop();
			return true;
		}

		public override bool HandleMouseInput(MouseInput mi)
		{
			return RenderBounds.Contains(mi.Location) && Skippable;
		}

		public override string GetCursor(int2 pos)
		{
			return null;
		}

		public void Play()
		{
			PlayThen(() => { });
		}

		public void PlayThen(Action after)
		{
			if (video == null)
				return;

			onComplete = after;

			stopped = paused = false;
		}

		public void Pause()
		{
			if (stopped || paused || video == null)
				return;

			paused = true;
		}

		public void Stop()
		{
			if (stopped || video == null)
				return;

			stopped = true;
			paused = true;
			video.Reset();
			Game.RunAfterTick(onComplete);
		}

		public void CloseVideo()
		{
			Stop();
			video = null;
		}
	}
}
