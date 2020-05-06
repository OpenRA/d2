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
using System.IO;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.D2.MapUtils;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Used to render terrain with round borders. Based on SmudgeLayer.cs")]
	class D2TerrainLayerInfo : TraitInfo
	{
		[Desc("Sprite sequence name")]
		public readonly string Sequence = "sides";

		[PaletteReference]
		public readonly string Palette = TileSet.TerrainPaletteInternalName;

		public override object Create(ActorInitializer init) { return new D2TerrainLayer(init.Self, this); }
	}

	class D2TerrainLayer : IRenderOverlay, IWorldLoaded, INotifyActorDisposing
	{
		public readonly D2TerrainLayerInfo Info;
		readonly Dictionary<string, Sprite[]> sideSprites = new Dictionary<string, Sprite[]>();
		readonly World world;

		TerrainSpriteLayer render;

		public D2TerrainLayer(Actor self, D2TerrainLayerInfo info)
		{
			Info = info;
			world = self.World;

			var sequenceProvider = world.Map.Rules.Sequences;
			var types = sequenceProvider.Sequences(Info.Sequence);
			foreach (var t in types)
			{
				var seq = sequenceProvider.GetSequence(Info.Sequence, t);
				var sprites = Exts.MakeArray(seq.Length, x => seq.GetSprite(x));
				sideSprites.Add(t, sprites);
			}
		}

		public void WorldLoaded(World w, WorldRenderer wr)
		{
			/* based on SmudgeLayer.cs */
			var first = sideSprites.First().Value.First();
			var sheet = first.Sheet;
			if (sideSprites.Values.Any(sprites => sprites.Any(s => s.Sheet != sheet)))
				throw new InvalidDataException("Resource sprites span multiple sheets. Try loading their sequences earlier.");

			var blendMode = first.BlendMode;
			if (sideSprites.Values.Any(sprites => sprites.Any(s => s.BlendMode != blendMode)))
				throw new InvalidDataException("Smudges specify different blend modes. "
					+ "Try using different smudge types for smudges that use different blend modes.");

			render = new TerrainSpriteLayer(w, wr, sheet, blendMode, wr.Palette(Info.Palette), wr.World.Type != WorldType.Editor);

			var tilesLayer = w.Map.Tiles;
			for (var v = 0; v < tilesLayer.Size.Height; v++)
			{
				for (var u = 0; u < tilesLayer.Size.Width; u++)
				{
					var pos = new MPos(u, v);
					var tile = tilesLayer[pos];

					if (tile.Type == D2MapUtils.RockTile)
					{
						var index = D2MapUtils.SmoothIndexForPos(tilesLayer, pos);
						if (index != 15)
						{
							CPos cpos = pos.ToCPos(w.Map);
							Sprite sprite = sideSprites["rock"][index];
							render.Update(cpos, sprite);
						}
					}

					if (tile.Type == D2MapUtils.DuneTile)
					{
						var index = D2MapUtils.SmoothIndexForPos(tilesLayer, pos);
						if (index != 15)
						{
							CPos cpos = pos.ToCPos(w.Map);
							Sprite sprite = sideSprites["dune"][index];
							render.Update(cpos, sprite);
						}
					}

					if (tile.Type == D2MapUtils.RoughTile)
					{
						var index = D2MapUtils.SmoothIndexForPos(tilesLayer, pos);
						if (index != 15)
						{
							CPos cpos = pos.ToCPos(w.Map);
							Sprite sprite = sideSprites["rough"][index];
							render.Update(cpos, sprite);
						}
					}
				}
			}
		}

		void IRenderOverlay.Render(WorldRenderer wr)
		{
			render.Draw(wr.Viewport);
		}

		bool disposed;

		void INotifyActorDisposing.Disposing(Actor self)
		{
			if (disposed)
				return;

			render.Dispose();
			disposed = true;
		}
	}
}
