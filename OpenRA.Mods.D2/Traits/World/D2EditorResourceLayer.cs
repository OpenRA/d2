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

using System.Linq;
using OpenRA.Mods.Common.Traits;

namespace OpenRA.Mods.D2.Traits
{
	using CellContents = D2ResourceLayer.CellContents;
	using ClearSides = D2ResourceLayer.ClearSides;

	[Desc("Used to render spice with round borders.")]
	public class D2EditorResourceLayerInfo : EditorResourceLayerInfo
	{
		public override object Create(ActorInitializer init) { return new D2EditorResourceLayer(init.Self); }
	}

	public class D2EditorResourceLayer : EditorResourceLayer
	{
		public D2EditorResourceLayer(Actor self)
			: base(self) { }

		public override CellContents UpdateDirtyTile(CPos c)
		{
			var t = Tiles[c];

			// Empty tile
			if (t.Type == null)
			{
				t.Sprite = null;
				return t;
			}

			NetWorth -= t.Density * t.Type.Info.ValuePerUnit;

			t.Density = ResourceDensityAt(c);

			NetWorth += t.Density * t.Type.Info.ValuePerUnit;

			int index;
			var clear = FindClearSides(t.Type, c);
			if (clear == ClearSides.None && CellContainsMaxDensity(c, t.Type))
			{
				var maxDensityClear = FindMaxDensityClearSides(t.Type, c);
				if (D2ResourceLayer.SpriteMap.TryGetValue(maxDensityClear, out index))
					t.Sprite = t.Type.Variants.First().Value[16 + index];
				else
					t.Sprite = null;
			}
			else if (D2ResourceLayer.SpriteMap.TryGetValue(clear, out index))
				t.Sprite = t.Type.Variants.First().Value[index];
			else
				t.Sprite = null;

			return t;
		}

		bool CellContains(CPos c, ResourceType t)
		{
			return Tiles.Contains(c) && Tiles[c].Type == t;
		}

		bool CellContainsMaxDensity(CPos c, ResourceType t)
		{
			if (!Tiles.Contains(c))
				return false;

			if (FindClearSides(t, c) != ClearSides.None)
				return false;

			var tile = Tiles[c];

			// Empty tile
			if (tile.Type == null)
				return false;

			var density = ResourceDensityAt(c);

			return density > tile.Type.Info.MaxDensity / 2;
		}

		ClearSides FindClearSides(ResourceType t, CPos p)
		{
			var ret = ClearSides.None;
			if (!CellContains(p + new CVec(0, -1), t))
				ret |= ClearSides.Top;

			if (!CellContains(p + new CVec(-1, 0), t))
				ret |= ClearSides.Left;

			if (!CellContains(p + new CVec(1, 0), t))
				ret |= ClearSides.Right;

			if (!CellContains(p + new CVec(0, 1), t))
				ret |= ClearSides.Bottom;

			return ret;
		}

		ClearSides FindMaxDensityClearSides(ResourceType t, CPos p)
		{
			var ret = ClearSides.None;
			if (!CellContainsMaxDensity(p + new CVec(0, -1), t))
				ret |= ClearSides.Top;

			if (!CellContainsMaxDensity(p + new CVec(-1, 0), t))
				ret |= ClearSides.Left;

			if (!CellContainsMaxDensity(p + new CVec(1, 0), t))
				ret |= ClearSides.Right;

			if (!CellContainsMaxDensity(p + new CVec(0, 1), t))
				ret |= ClearSides.Bottom;

			return ret;
		}
	}
}
