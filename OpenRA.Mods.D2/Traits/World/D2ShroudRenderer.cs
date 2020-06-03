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

using System;
using System.IO;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Traits;

/* based on ShroudRenderer */

namespace OpenRA.Mods.D2.Traits
{
	public class D2ShroudRendererInfo : TraitInfo
	{
		public readonly string Sequence = "shroud";
		[SequenceReference("Sequence")]
		public readonly string ShroudName = "shroud";

		[SequenceReference("Sequence")]
		public readonly string FogName = "fog";

		[PaletteReference]
		public readonly string ShroudPalette = "shroud";

		[PaletteReference]
		public readonly string FogPalette = "fog";

		[Desc("Hide or not map borders under shroud")]
		public readonly bool ShroudOnMapBorders = true;

		public readonly BlendMode ShroudBlend = BlendMode.Alpha;
		public override object Create(ActorInitializer init) { return new D2ShroudRenderer(init.World, this); }
	}

	public sealed class D2ShroudRenderer : IRenderShroud, IWorldLoaded, INotifyActorDisposing
	{
		[Flags]
		enum Edges : byte
		{
			None = 0,
			Top = 0x1,
			Right = 0x2,
			Bottom = 0x4,
			Left = 0x8,
			All = Top | Right | Bottom | Left,
			TopLeft = Top | Left,
			TopRight = Top | Right,
			BottomLeft = Bottom | Left,
			BottomRight = Bottom | Right
		}

		struct TileInfo
		{
			public readonly float3 ScreenPosition;

			public TileInfo(float3 screenPosition)
			{
				ScreenPosition = screenPosition;
			}
		}

		readonly D2ShroudRendererInfo info;
		readonly World world;
		readonly Map map;
		readonly Edges notVisibleSides;

		readonly CellLayer<TileInfo> tileInfos;
		readonly CellLayer<bool> cellsDirty;
		readonly Sprite[] fogSprites, shroudSprites;

		Shroud shroud;
		Func<PPos, bool> visibleUnderShroud, visibleUnderFog;
		TerrainSpriteLayer shroudLayer, fogLayer;
		bool disposed;

		public D2ShroudRenderer(World world, D2ShroudRendererInfo info)
		{
			this.info = info;
			this.world = world;
			map = world.Map;

			tileInfos = new CellLayer<TileInfo>(map);

			cellsDirty = new CellLayer<bool>(map);

			// Load sprites
			var sequenceProvider = map.Rules.Sequences;

			var shroudSequence = sequenceProvider.GetSequence(info.Sequence, info.ShroudName);
			shroudSprites = new Sprite[shroudSequence.Length];
			for (var i = 0; i < shroudSequence.Length; i++)
				shroudSprites[i] = shroudSequence.GetSprite(i);

			var fogSequence = sequenceProvider.GetSequence(info.Sequence, info.FogName);
			fogSprites = new Sprite[fogSequence.Length];
			for (var i = 0; i < fogSequence.Length; i++)
				fogSprites[i] = fogSequence.GetSprite(i);

			notVisibleSides = Edges.All;

			world.RenderPlayerChanged += WorldOnRenderPlayerChanged;
		}

		void IWorldLoaded.WorldLoaded(World w, WorldRenderer wr)
		{
			// Initialize tile cache
			// This includes the region outside the visible area to cover any sprites peeking outside the map
			foreach (var uv in w.Map.AllCells.MapCoords)
			{
				var pos = w.Map.CenterOfCell(uv.ToCPos(map));
				var screen = wr.Screen3DPosition(pos - new WVec(0, 0, pos.Z));
				tileInfos[uv] = new TileInfo(screen);
			}

			// All tiles are visible in the editor
			if (w.Type == WorldType.Editor)
				visibleUnderShroud = _ => true;
			else
				visibleUnderShroud = puv => map.Contains(puv);

			visibleUnderFog = puv => map.Contains(puv);

			var shroudSheet = shroudSprites[0].Sheet;
			if (shroudSprites.Any(s => s.Sheet != shroudSheet))
				throw new InvalidDataException("Shroud sprites span multiple sheets. Try loading their sequences earlier.");

			var shroudBlend = shroudSprites[0].BlendMode;
			if (shroudSprites.Any(s => s.BlendMode != shroudBlend))
				throw new InvalidDataException("Shroud sprites must all use the same blend mode.");

			var fogSheet = fogSprites[0].Sheet;
			if (fogSprites.Any(s => s.Sheet != fogSheet))
				throw new InvalidDataException("Fog sprites span multiple sheets. Try loading their sequences earlier.");

			var fogBlend = fogSprites[0].BlendMode;
			if (fogSprites.Any(s => s.BlendMode != fogBlend))
				throw new InvalidDataException("Fog sprites must all use the same blend mode.");

			shroudLayer = new TerrainSpriteLayer(w, wr, shroudSheet, shroudBlend, wr.Palette(info.ShroudPalette), false);
			fogLayer = new TerrainSpriteLayer(w, wr, fogSheet, fogBlend, wr.Palette(info.FogPalette), false);

			WorldOnRenderPlayerChanged(world.RenderPlayer);
		}

		Edges GetEdges(PPos puv, Func<PPos, bool> isVisible)
		{
			if (!isVisible(puv))
				return notVisibleSides;

			var cell = ((MPos)puv).ToCPos(map);

			// If a side is shrouded then we also count the corners.
			var edge = Edges.None;
			if (!isVisible((PPos)(cell + new CVec(0, -1)).ToMPos(map))) edge |= Edges.Top;
			if (!isVisible((PPos)(cell + new CVec(1, 0)).ToMPos(map))) edge |= Edges.Right;
			if (!isVisible((PPos)(cell + new CVec(0, 1)).ToMPos(map))) edge |= Edges.Bottom;
			if (!isVisible((PPos)(cell + new CVec(-1, 0)).ToMPos(map))) edge |= Edges.Left;

			if (!info.ShroudOnMapBorders)
			{
				var mpos = cell.ToMPos(map);
				if (edge == Edges.Top && mpos.V <= 1) edge = Edges.None;
				if (edge == Edges.Left && mpos.U <= 1) edge = Edges.None;
				if (edge == Edges.Bottom && mpos.V >= map.Tiles.Size.Height - 2) edge = Edges.None;
				if (edge == Edges.Right && mpos.U >= map.Tiles.Size.Width - 2) edge = Edges.None;

				if (edge == Edges.TopLeft && mpos.V <= 1 && mpos.U <= 1) edge = Edges.None;
				if (edge == Edges.TopRight && mpos.V <= 1 && mpos.U >= map.Tiles.Size.Width - 2) edge = Edges.None;
				if (edge == Edges.BottomLeft && mpos.V >= map.Tiles.Size.Height - 2 && mpos.U <= 1) edge = Edges.None;
				if (edge == Edges.BottomRight && mpos.V >= map.Tiles.Size.Height - 2 && mpos.U >= map.Tiles.Size.Width - 2) edge = Edges.None;
			}

			return edge;
		}

		void WorldOnRenderPlayerChanged(Player player)
		{
			var newShroud = player != null ? player.Shroud : null;

			if (shroud != newShroud)
			{
				if (shroud != null)
					shroud.OnShroudChanged -= UpdateShroudCell;

				if (newShroud != null)
				{
					visibleUnderShroud = puv => newShroud.IsExplored(puv);
					visibleUnderFog = puv => newShroud.IsVisible(puv);
					newShroud.OnShroudChanged += UpdateShroudCell;
				}
				else
				{
					visibleUnderShroud = puv => map.Contains(puv);
					visibleUnderFog = puv => map.Contains(puv);
				}

				shroud = newShroud;
			}

			// Dirty the full projected space so the cells outside
			// the map bounds can be initialized as fully shrouded.
			cellsDirty.Clear(true);
			var tl = new PPos(0, 0);
			var br = new PPos(map.MapSize.X - 1, map.MapSize.Y - 1);
			UpdateShroud(new ProjectedCellRegion(map, tl, br));
		}

		void UpdateShroud(ProjectedCellRegion region)
		{
			foreach (var puv in region)
			{
				var uv = (MPos)puv;
				if (!cellsDirty[uv] || !tileInfos.Contains(uv))
					continue;

				cellsDirty[uv] = false;

				var tileInfo = tileInfos[uv];
				var shroudSprite = GetSprite(shroudSprites, GetEdges(puv, visibleUnderShroud));
				var shroudPos = tileInfo.ScreenPosition;
				if (shroudSprite != null)
					shroudPos += shroudSprite.Offset - 0.5f * shroudSprite.Size;

				var fogSprite = GetSprite(fogSprites, GetEdges(puv, visibleUnderFog));
				var fogPos = tileInfo.ScreenPosition;
				if (fogSprite != null)
					fogPos += fogSprite.Offset - 0.5f * fogSprite.Size;

				shroudLayer.Update(uv, shroudSprite, shroudPos);
				fogLayer.Update(uv, fogSprite, fogPos);
			}
		}

		void IRenderShroud.RenderShroud(WorldRenderer wr)
		{
			UpdateShroud(map.ProjectedCellBounds);
			fogLayer.Draw(wr.Viewport);
			shroudLayer.Draw(wr.Viewport);
		}

		void UpdateShroudCell(PPos puv)
		{
			var uv = (MPos)puv;
			cellsDirty[uv] = true;
			var cell = uv.ToCPos(map);
			foreach (var direction in CVec.Directions)
				if (map.Contains((PPos)(cell + direction).ToMPos(map)))
					cellsDirty[cell + direction] = true;
		}

		Sprite GetSprite(Sprite[] sprites, Edges edges)
		{
			if (edges == Edges.None)
				return null;

			return sprites[(byte)edges];
		}

		void INotifyActorDisposing.Disposing(Actor self)
		{
			if (disposed)
				return;

			shroudLayer.Dispose();
			fogLayer.Dispose();
			disposed = true;
		}
	}
}
