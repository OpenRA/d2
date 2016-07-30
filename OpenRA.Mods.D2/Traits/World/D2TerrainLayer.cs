#region Copyright & License Information
/*
 * Copyright 2007-2016 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Used to render terrain with round borders. Based on SmudgeLayer.cs")]
	class D2TerrainLayerInfo : ITraitInfo
	{
		[Desc("Sprite sequence name")]
		public readonly string Sequence = "sides";

		[PaletteReference] public readonly string Palette = TileSet.TerrainPaletteInternalName;

		public object Create(ActorInitializer init) { return new D2TerrainLayer(init.Self, this); }
	}

	class D2TerrainLayer : IRenderOverlay, IWorldLoaded, INotifyActorDisposing
	{
		[Flags] public enum ClearSides : byte
		{
			None = 0x0,
			Left = 0x1,
			Top = 0x2,
			Right = 0x4,
			Bottom = 0x8,

			All = 0x0F
		}

		public static readonly Dictionary<ClearSides, int> SpriteMap = new Dictionary<ClearSides, int>()
		{
			{ ClearSides.All, 0 },
			{ ClearSides.Left | ClearSides.Right | ClearSides.Bottom, 1 },
			{ ClearSides.Left | ClearSides.Top | ClearSides.Bottom, 2 },
			{ ClearSides.Left | ClearSides.Bottom, 3 },
			{ ClearSides.Left | ClearSides.Top | ClearSides.Right, 4 },
			{ ClearSides.Left | ClearSides.Right, 5 },
			{ ClearSides.Left | ClearSides.Top, 6 },
			{ ClearSides.Left, 7 },
			{ ClearSides.Top | ClearSides.Right | ClearSides.Bottom, 8 },
			{ ClearSides.Right | ClearSides.Bottom, 9 },
			{ ClearSides.Top | ClearSides.Bottom, 10 },
			{ ClearSides.Bottom, 11 },
			{ ClearSides.Top | ClearSides.Right, 12 },
			{ ClearSides.Right, 13 },
			{ ClearSides.Top, 14 },
			{ ClearSides.None, 15 },
		};

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
					var mpos = new MPos(u, v);
					var tile = tilesLayer[mpos];

					if (tile.Type == 143)
					{
						ClearSides clear = ClearSides.None;

						if (u > 0)
						{
							var leftPos = new MPos(u - 1, v);
							var leftTile = tilesLayer[leftPos];
							if (!(leftTile.Type >= 126 && leftTile.Type <= 143) &&
							    !(leftTile.Type >= 160 && leftTile.Type <= 175))
							{
								clear |= ClearSides.Left;
							}
						}

						if (v > 0)
						{
							var topPos = new MPos(u, v - 1);
							var topTile = tilesLayer[topPos];
							if (!(topTile.Type >= 126 && topTile.Type <= 143) &&
							    !(topTile.Type >= 160 && topTile.Type <= 175))
							{
								clear |= ClearSides.Top;
							}
						}

						if (u < tilesLayer.Size.Width - 1)
						{
							var rightPos = new MPos(u + 1, v);
							var rightTile = tilesLayer[rightPos];
							if (!(rightTile.Type >= 126 && rightTile.Type <= 143) &&
							    !(rightTile.Type >= 160 && rightTile.Type <= 175))
							{
								clear |= ClearSides.Right;
							}
						}

						if (v < tilesLayer.Size.Height - 1)
						{
							var bottomPos = new MPos(u, v + 1);
							var bottomTile = tilesLayer[bottomPos];
							if (!(bottomTile.Type >= 126 && bottomTile.Type <= 143) &&
							    !(bottomTile.Type >= 160 && bottomTile.Type <= 175))
							{
								clear |= ClearSides.Bottom;
							}
						}

						if (clear != ClearSides.None)
						{
							CPos cpos = mpos.ToCPos(w.Map);
							Sprite sprite = sideSprites["rock"][SpriteMap[clear]];
							render.Update(cpos, sprite);
						}
					}

					if (tile.Type == 175)
					{
						ClearSides clear = ClearSides.None;

						if (u > 0)
						{
							var leftPos = new MPos(u - 1, v);
							var leftTile = tilesLayer[leftPos];
							if (!(leftTile.Type >= 160 && leftTile.Type <= 175))
							{
								clear |= ClearSides.Left;
							}
						}

						if (v > 0)
						{
							var topPos = new MPos(u, v - 1);
							var topTile = tilesLayer[topPos];
							if (!(topTile.Type >= 160 && topTile.Type <= 175))
							{
								clear |= ClearSides.Top;
							}
						}

						if (u < tilesLayer.Size.Width - 1)
						{
							var rightPos = new MPos(u + 1, v);
							var rightTile = tilesLayer[rightPos];
							if (!(rightTile.Type >= 160 && rightTile.Type <= 175))
							{
								clear |= ClearSides.Right;
							}
						}

						if (v < tilesLayer.Size.Height - 1)
						{
							var bottomPos = new MPos(u, v + 1);
							var bottomTile = tilesLayer[bottomPos];
							if (!(bottomTile.Type >= 160 && bottomTile.Type <= 175))
							{
								clear |= ClearSides.Bottom;
							}
						}

						if (clear != ClearSides.None)
						{
							CPos cpos = mpos.ToCPos(w.Map);
							Sprite sprite = sideSprites["rough"][SpriteMap[clear]];
							render.Update(cpos, sprite);
						}
					}

					if (tile.Type == 159)
					{
						ClearSides clear = ClearSides.None;

						if (u > 0)
						{
							var leftPos = new MPos(u - 1, v);
							var leftTile = tilesLayer[leftPos];
							if (!(leftTile.Type >= 144 && leftTile.Type <= 159))
							{
								clear |= ClearSides.Left;
							}
						}

						if (v > 0)
						{
							var topPos = new MPos(u, v - 1);
							var topTile = tilesLayer[topPos];
							if (!(topTile.Type >= 144 && topTile.Type <= 159))
							{
								clear |= ClearSides.Top;
							}
						}

						if (u < tilesLayer.Size.Width - 1)
						{
							var rightPos = new MPos(u + 1, v);
							var rightTile = tilesLayer[rightPos];
							if (!(rightTile.Type >= 144 && rightTile.Type <= 159))
							{
								clear |= ClearSides.Right;
							}
						}

						if (v < tilesLayer.Size.Height - 1)
						{
							var bottomPos = new MPos(u, v + 1);
							var bottomTile = tilesLayer[bottomPos];
							if (!(bottomTile.Type >= 144 && bottomTile.Type <= 159))
							{
								clear |= ClearSides.Bottom;
							}
						}

						if (clear != ClearSides.None)
						{
							CPos cpos = mpos.ToCPos(w.Map);
							Sprite sprite = sideSprites["dune"][SpriteMap[clear]];
							render.Update(cpos, sprite);
						}
					}
				}
			}
		}

		public void Render(WorldRenderer wr)
		{
			render.Draw(wr.Viewport);
		}

		bool disposed;
		public void Disposing(Actor self)
		{
			if (disposed)
				return;

			render.Dispose();
			disposed = true;
		}
	}
}
