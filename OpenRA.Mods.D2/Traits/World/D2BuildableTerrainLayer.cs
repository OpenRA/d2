#region Copyright & License Information
/*
 * Copyright 2007-2020 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.D2.Traits
{
	[Desc("Attach this to the world actor. Required for D2Building to work.")]
	public class D2BuildableTerrainLayerInfo : TraitInfo
	{
		[Desc("Palette to render the layer sprites in.")]
		public readonly string Palette = TileSet.TerrainPaletteInternalName;

		[Desc("The hitpoints, which can be reduced by the DamagesConcreteWarhead.")]
		public readonly int MaxStrength = 9000;

		public override object Create(ActorInitializer init) { return new D2BuildableTerrainLayer(init.Self, this); }
	}

	public class D2BuildableTerrainLayer : IRenderOverlay, IWorldLoaded, ITickRender, INotifyActorDisposing
	{
		readonly D2BuildableTerrainLayerInfo info;
		readonly Dictionary<CPos, TerrainTile?> dirty = new Dictionary<CPos, TerrainTile?>();
		readonly World world;
		readonly CellLayer<int> strength;

		TerrainSpriteLayer render;
		ITiledTerrainRenderer terrainRenderer;
		PaletteReference paletteReference;
		bool disposed;

		public D2BuildableTerrainLayer(Actor self, D2BuildableTerrainLayerInfo info)
		{
			this.info = info;
			world = self.World;
			strength = new CellLayer<int>(world.Map);
			terrainRenderer = self.Trait<ITiledTerrainRenderer>();
		}

		public void WorldLoaded(World w, WorldRenderer wr)
		{
			render = new TerrainSpriteLayer(w, wr, terrainRenderer.MissingTile, BlendMode.Alpha, wr.World.Type != WorldType.Editor);
			paletteReference = wr.Palette(info.Palette);
		}

		public void AddTile(CPos cell, TerrainTile tile)
		{
			if (!strength.Contains(cell))
				return;

			world.Map.CustomTerrain[cell] = world.Map.Rules.TerrainInfo.GetTerrainIndex(tile);
			strength[cell] = info.MaxStrength;
			dirty[cell] = tile;
		}

		public void HitTile(CPos cell, int damage)
		{
			if (!strength.Contains(cell) || strength[cell] == 0)
				return;

			// Buildings (but not other actors) block damage to cells under their footprint
			if (world.ActorMap.GetActorsAt(cell).Any(a => a.TraitOrDefault<Building>() != null))
				return;

			strength[cell] = strength[cell] - damage;
			if (strength[cell] < 1)
				RemoveTile(cell);
		}

		public void RemoveTile(CPos cell)
		{
			if (!strength.Contains(cell))
				return;

			world.Map.CustomTerrain[cell] = byte.MaxValue;
			strength[cell] = 0;
			dirty[cell] = null;
		}

		void ITickRender.TickRender(WorldRenderer wr, Actor self)
		{
			var remove = new List<CPos>();
			foreach (var kv in dirty)
			{
				if (!self.World.FogObscures(kv.Key))
				{
					var tile = kv.Value;
					if (tile.HasValue)
					{
						// Terrain tiles define their origin at the topleft
						var s = terrainRenderer.TileSprite(tile.Value);
						var ss = new Sprite(s.Sheet, s.Bounds, s.ZRamp, float2.Zero, s.Channel, s.BlendMode);
						render.Update(kv.Key, ss, paletteReference);
					}
					else
						render.Clear(kv.Key);

					remove.Add(kv.Key);
				}
			}

			foreach (var r in remove)
				dirty.Remove(r);
		}

		void IRenderOverlay.Render(WorldRenderer wr)
		{
			render.Draw(wr.Viewport);
		}

		void INotifyActorDisposing.Disposing(Actor self)
		{
			if (disposed)
				return;

			render.Dispose();
			disposed = true;
		}
	}
}
