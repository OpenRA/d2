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
		public Hotkey CancelKey = new(Keycode.ESCAPE, Modifiers.None);
		public float AspectRatio = 1.2f;
		public bool Skippable = true;

		public bool Paused { get; private set; }
		public WsaReader Video { get; private set; } = null;

		string cachedVideo;
		float2 videoOrigin;
		bool stopped;
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
				palette = new ImmutablePalette(stream, Array.Empty<int>(), Array.Empty<int>());

			hardwarePalette = new HardwarePalette();
			hardwarePalette.AddPalette("chrome", palette, false);
			hardwarePalette.Initialize();
			Game.Renderer.SetPalette(hardwarePalette);
			var pal = hardwarePalette.GetPalette("chrome");
			pr = new PaletteReference("chromeref", hardwarePalette.GetPaletteIndex("chrome"), pal, hardwarePalette);
		}

		public void Open(WsaReader video)
		{
			Video = video;

			stopped = true;
			Paused = true;
			onComplete = () => { };

			var scale = Math.Min((float)RenderBounds.Width / video.Width, (float)RenderBounds.Height / video.Height * AspectRatio);
			videoOrigin = new float2(
				RenderBounds.X + (RenderBounds.Width - scale * video.Width) / 2,
				RenderBounds.Y + (RenderBounds.Height - scale * video.Height * AspectRatio) / 2);
		}

		public override void Draw()
		{
			if (Video == null)
				return;

			var sheetBuilder = new SheetBuilder(SheetType.Indexed, 512);
			var videoSprite = sheetBuilder.Add(Video.Frame);

			Game.Renderer.EnableScissor(RenderBounds);
			Game.Renderer.RgbaColorRenderer.FillRect(
				new float2(RenderBounds.Left, RenderBounds.Top),
				new float2(RenderBounds.Right, RenderBounds.Bottom), Primitives.Color.Black);
			Game.Renderer.DisableScissor();

			Game.Renderer.SpriteRenderer.DrawSprite(videoSprite, pr, videoOrigin);

			if (!stopped && !Paused)
			{
				if (Video.CurrentFrame >= Video.Length - 1)
				{
					Stop();
					return;
				}

				Video.AdvanceFrame();
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
			if (Video == null)
				return;

			onComplete = after;

			stopped = Paused = false;
		}

		public void Pause()
		{
			if (stopped || Paused || Video == null)
				return;

			Paused = true;
		}

		public void Stop()
		{
			if (stopped || Video == null)
				return;

			stopped = true;
			Paused = true;
			Video.Reset();
			Game.RunAfterTick(onComplete);
		}

		public void CloseVideo()
		{
			Stop();
			Video = null;
		}
	}
}
