#region Copyright & License Information
/*
 * Copyright 2007-2020 The d2 mod Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.D2.Graphics
{
	[TraitLocation(SystemActors.World)]
	[Desc("Renders Dune 2 style screen-space pixel distortion effects.")]
	public class D2DistortionRendererInfo : TraitInfo
	{
		public readonly string FragmentShader = "d2|glsl/postprocess_textured_distortion.frag";
		public readonly PostProcessPassType PassType = PostProcessPassType.AfterWorld;
		public readonly HashSet<D2DistortionStyle> Styles = new() { D2DistortionStyle.Sand, D2DistortionStyle.Sonic };

		public override object Create(ActorInitializer init) { return new D2DistortionRenderer(this); }
	}

	public sealed class D2DistortionRenderer : IRenderPostProcessPass, INotifyActorDisposing
	{
		// Renderables enqueue these records during normal world rendering. The post-process pass
		// then replays OpenDUNE-style blurred sprites against the completed world texture.
		readonly struct Distortion
		{
			public readonly D2DistortionStyle Style;
			public readonly float3 Pos;
			public readonly Sprite Sprite;
			public readonly int BlurOffset;

			public Distortion(D2DistortionStyle style, float3 pos, Sprite sprite, int blurOffset)
			{
				Style = style;
				Pos = pos;
				Sprite = sprite;
				BlurOffset = blurOffset;
			}
		}

		sealed class D2DistortionShaderBindings : IShaderBindings
		{
			public D2DistortionShaderBindings(string fragmentShader)
			{
				// Reuse the engine's textured post-process vertex shader. It interprets vertex
				// positions as local screen-space pixel offsets from the Pos uniform and passes
				// through sprite-sheet UVs for the fragment shader's mask lookup.
				VertexShaderName = "postprocess_textured";
				VertexShaderCode = ShaderBindings.GetShaderCode("postprocess_textured.vert");
				FragmentShaderName = fragmentShader;

				using (var stream = Game.ModData.DefaultFileSystem.Open(fragmentShader))
				using (var reader = new StreamReader(stream))
					FragmentShaderCode = reader.ReadToEnd();
			}

			public string VertexShaderName { get; }
			public string VertexShaderCode { get; }
			public string FragmentShaderName { get; }
			public string FragmentShaderCode { get; }
			public int Stride => Attributes.Sum(a => a.Components * 4);

			public ShaderVertexAttribute[] Attributes { get; } =
			{
				new("aVertexPosition", ShaderVertexAttributeType.Float, 2, 0),
				new("aVertexTexCoord", ShaderVertexAttributeType.Float, 2, 8)
			};
		}

		readonly Renderer renderer;
		readonly IShader shader;
		readonly D2DistortionRendererInfo info;
		readonly IVertexBuffer<RenderPostProcessPassTexturedVertex> buffer;
		readonly List<Distortion> distortions = new();
		readonly RenderPostProcessPassTexturedVertex[] vertices = new RenderPostProcessPassTexturedVertex[6];
		static readonly int[] BlurOffsets = { 1, 3, 2, 5, 4, 3, 2, 1 };
		static int blurIndex;
		static int blurTickCounter;
		const int BlurTickInterval = 6;

		public D2DistortionRenderer(D2DistortionRendererInfo info)
		{
			this.info = info;
			renderer = Game.Renderer;
			shader = renderer.CreateShader(new D2DistortionShaderBindings(info.FragmentShader));
			buffer = renderer.CreateVertexBuffer<RenderPostProcessPassTexturedVertex>(6);
		}

		public bool Accepts(D2DistortionStyle style)
		{
			return info.Styles.Contains(style);
		}

		public void DrawSprite(float3 pos, Sprite sprite, D2DistortionStyle style)
		{
			// BlurOffset больше не вычисляется здесь — только в Draw(),
			// чтобы blurIndex продвигался один раз за рендер-пасс, а не за спрайт.
			distortions.Add(new Distortion(style, pos, sprite, 0));
		}

		PostProcessPassType IRenderPostProcessPass.Type => info.PassType;
		bool IRenderPostProcessPass.Enabled => distortions.Count > 0;

		void UpdateVertices(in Distortion d)
		{
			var halfWidth = d.Sprite.Size.X / 2;
			var halfHeight = d.Sprite.Size.Y / 2;
			vertices[0] = new RenderPostProcessPassTexturedVertex(-halfWidth, -halfHeight, d.Sprite.Left, d.Sprite.Top);
			vertices[1] = new RenderPostProcessPassTexturedVertex(halfWidth, -halfHeight, d.Sprite.Right, d.Sprite.Top);
			vertices[2] = new RenderPostProcessPassTexturedVertex(halfWidth, halfHeight, d.Sprite.Right, d.Sprite.Bottom);
			vertices[3] = new RenderPostProcessPassTexturedVertex(halfWidth, halfHeight, d.Sprite.Right, d.Sprite.Bottom);
			vertices[4] = new RenderPostProcessPassTexturedVertex(-halfWidth, halfHeight, d.Sprite.Left, d.Sprite.Bottom);
			vertices[5] = new RenderPostProcessPassTexturedVertex(-halfWidth, -halfHeight, d.Sprite.Left, d.Sprite.Top);
		}

		void IRenderPostProcessPass.Draw(WorldRenderer wr)
		{
			if (++blurTickCounter >= BlurTickInterval)
			{
				blurTickCounter = 0;
				blurIndex = (blurIndex + 1) % BlurOffsets.Length;
			}

			var scroll = wr.Viewport.TopLeft;
			var size = renderer.WorldFrameBufferSize;
			var width = 2f / (renderer.WorldDownscaleFactor * size.Width);
			var height = 2f / (renderer.WorldDownscaleFactor * size.Height);

			shader.SetVec("Scroll", scroll.X, scroll.Y);
			shader.SetVec("p1", width, height);
			shader.SetVec("p2", -1, -1);
			shader.SetTexture("WorldTexture", Game.Renderer.WorldBufferSnapshot());
			shader.PrepareRender();

			for (var i = 0; i < distortions.Count; i++)
			{
				var d = distortions[i];
				// Каждый сегмент получает следующий шаг таблицы — как три последовательных
				// вызова GUI_DrawSprite в оригинале в рамках одного игрового кадра.
				var blurOffset = BlurOffsets[(blurIndex + i) % BlurOffsets.Length];

				UpdateVertices(d);
				buffer.SetData(vertices, 6);

				shader.SetVec("Pos", d.Pos.X, d.Pos.Y);
				shader.SetVec("Style", (float)d.Style);
				shader.SetVec("BlurOffset", blurOffset);
				shader.SetVec("MaskChannel", (float)d.Sprite.Channel);
				shader.SetTexture("MaskTexture", d.Sprite.Sheet.GetTexture());
				renderer.DrawBatch(buffer, shader, 0, 6, PrimitiveType.TriangleList);
			}

			distortions.Clear();
		}

		void INotifyActorDisposing.Disposing(Actor self)
		{
			buffer.Dispose();
		}
	}
}
