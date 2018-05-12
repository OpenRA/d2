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
using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.D2.Traits
{
	[Desc("Used to render spice with round borders.")]
	public class D2ResourceLayerInfo : ResourceLayerInfo
	{
		public override object Create(ActorInitializer init) { return new D2ResourceLayer(init.Self); }
	}

	public class D2ResourceLayer : ResourceLayer
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

		public D2ResourceLayer(Actor self)
			: base(self) { }

		bool CellContains(CPos c, ResourceType t)
		{
			return RenderContent.Contains(c) && RenderContent[c].Type == t;
		}

		bool CellContainsMaxDensity(CPos c, ResourceType t)
		{
			if (!RenderContent.Contains(c))
				return false;

			if (FindClearSides(t, c) != ClearSides.None)
				return false;

			var tile = RenderContent[c];
			return tile.Density > tile.Type.Info.MaxDensity / 2;
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

		void UpdateRenderedTileInner(CPos p)
		{
			if (!RenderContent.Contains(p))
				return;

			var t = RenderContent[p];
			if (t.Density > 0)
			{
				var clear = FindClearSides(t.Type, p);
				int index;

				if (clear == ClearSides.None && CellContainsMaxDensity(p, t.Type))
				{
					var maxDensityClear = FindMaxDensityClearSides(t.Type, p);
					if (SpriteMap.TryGetValue(maxDensityClear, out index))
						t.Sprite = t.Type.Variants.First().Value[16 + index];
					else
						t.Sprite = null;
				}
				else if (SpriteMap.TryGetValue(clear, out index))
					t.Sprite = t.Type.Variants.First().Value[index];
				else
					t.Sprite = null;
			}
			else
				t.Sprite = null;

			RenderContent[p] = t;
		}

		protected override void UpdateRenderedSprite(CPos p)
		{
			// Need to update neighbouring tiles too
			UpdateRenderedTileInner(p);
			UpdateRenderedTileInner(p + new CVec(-1, -1));
			UpdateRenderedTileInner(p + new CVec(0, -1));
			UpdateRenderedTileInner(p + new CVec(1, -1));
			UpdateRenderedTileInner(p + new CVec(-1, 0));
			UpdateRenderedTileInner(p + new CVec(1, 0));
			UpdateRenderedTileInner(p + new CVec(-1, 1));
			UpdateRenderedTileInner(p + new CVec(0, 1));
			UpdateRenderedTileInner(p + new CVec(1, 1));
			UpdateRenderedTileInner(p + new CVec(-2, -2));
			UpdateRenderedTileInner(p + new CVec(-1, -2));
			UpdateRenderedTileInner(p + new CVec(0, -2));
			UpdateRenderedTileInner(p + new CVec(1, -2));
			UpdateRenderedTileInner(p + new CVec(2, -2));
			UpdateRenderedTileInner(p + new CVec(2, -1));
			UpdateRenderedTileInner(p + new CVec(2, 0));
			UpdateRenderedTileInner(p + new CVec(2, 1));
			UpdateRenderedTileInner(p + new CVec(2, 2));
			UpdateRenderedTileInner(p + new CVec(1, 2));
			UpdateRenderedTileInner(p + new CVec(0, 2));
			UpdateRenderedTileInner(p + new CVec(-1, 2));
			UpdateRenderedTileInner(p + new CVec(-2, 2));
			UpdateRenderedTileInner(p + new CVec(-2, 1));
			UpdateRenderedTileInner(p + new CVec(-2, 0));
			UpdateRenderedTileInner(p + new CVec(-2, -1));
		}
	}
}
