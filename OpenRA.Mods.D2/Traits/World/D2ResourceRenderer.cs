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

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Traits;

namespace OpenRA.Mods.D2.Traits
{
	[Desc("Used to render spice with round borders in d2 mod.", "Attach this to the world actor")]
	public class D2ResourceRendererInfo : ResourceRendererInfo
	{
		public override object Create(ActorInitializer init) { return new D2ResourceRenderer(init.Self, this); }
	}

	public class D2ResourceRenderer : ResourceRenderer
	{
		[Flags]
		public enum ClearSides : byte
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

		static readonly CVec[] Directions =
		{
			new CVec(-1, -1),
			new CVec(-1,  0),
			new CVec(-1,  1),
			new CVec(0, -1),
			new CVec(0,  1),
			new CVec(1, -1),
			new CVec(1,  0),
			new CVec(1,  1),
			new CVec(-2, -2),
			new CVec(-1, -2),
			new CVec(0, -2),
			new CVec(1, -2),
			new CVec(2, -2),
			new CVec(2, -1),
			new CVec(2, 0),
			new CVec(2, 1),
			new CVec(2, 2),
			new CVec(1, 2),
			new CVec(0, 2),
			new CVec(-1, 2),
			new CVec(-2, 2),
			new CVec(-2, 1),
			new CVec(-2, 0),
			new CVec(-2, -1)
		};

		public D2ResourceRenderer(Actor self, D2ResourceRendererInfo info)
			: base(self, info) { }

		bool CellContains(CPos cell, string resourceType)
		{
			return RenderContents.Contains(cell) && RenderContents[cell].Type == resourceType;
		}

		bool CellContainsMaxDensity(CPos cell, string resourceType)
		{
			if (!RenderContents.Contains(cell))
				return false;

			if (FindClearSides(resourceType, cell) != ClearSides.None)
				return false;

			var tile = RenderContents[cell];
			var maxDensity = ResourceLayer.GetMaxDensity(tile.Type);
			return tile.Density > maxDensity / 2;
		}

		ClearSides FindClearSides(string resourceType, CPos cell)
		{
			var ret = ClearSides.None;
			if (!CellContains(cell + new CVec(0, -1), resourceType))
				ret |= ClearSides.Top;

			if (!CellContains(cell + new CVec(-1, 0), resourceType))
				ret |= ClearSides.Left;

			if (!CellContains(cell + new CVec(1, 0), resourceType))
				ret |= ClearSides.Right;

			if (!CellContains(cell + new CVec(0, 1), resourceType))
				ret |= ClearSides.Bottom;

			return ret;
		}

		ClearSides FindMaxDensityClearSides(string resourceType, CPos cell)
		{
			var ret = ClearSides.None;
			if (!CellContainsMaxDensity(cell + new CVec(0, -1), resourceType))
				ret |= ClearSides.Top;

			if (!CellContainsMaxDensity(cell + new CVec(-1, 0), resourceType))
				ret |= ClearSides.Left;

			if (!CellContainsMaxDensity(cell + new CVec(1, 0), resourceType))
				ret |= ClearSides.Right;

			if (!CellContainsMaxDensity(cell + new CVec(0, 1), resourceType))
				ret |= ClearSides.Bottom;

			return ret;
		}

		protected override void UpdateRenderedSprite(CPos cell, RendererCellContents content)
		{
			UpdateRenderedSpriteInner(cell, content);

			for (var i = 0; i < Directions.Length; i++)
				UpdateRenderedSpriteInner(cell + Directions[i]);
		}

		void UpdateRenderedSpriteInner(CPos cell)
		{
			if (RenderContents.Contains(cell))
				UpdateRenderedSpriteInner(cell, RenderContents[cell]);
		}

		void UpdateRenderedSpriteInner(CPos cell, RendererCellContents content)
		{
			var density = content.Density;
			var renderType = content.Type;

			if (density > 0 && renderType != null)
			{
				// The call chain for this method (that starts with AddDirtyCell()) guarantees
				// that the new content type would still be suitable for this renderer,
				// but that is a bit too fragile to rely on in case the code starts changing.
				if (!Info.ResourceTypes.ContainsKey(renderType))
					return;

				var clear = FindClearSides(renderType, cell);
				int index;

				if (clear == ClearSides.None && CellContainsMaxDensity(cell, renderType))
				{
					var maxDensityClear = FindMaxDensityClearSides(renderType, cell);
					if (SpriteMap.TryGetValue(maxDensityClear, out index))
					{
						// Max density sprites is right after normal sprites
						index += 16;
						UpdateSpriteLayers(cell, Variants[renderType].First().Value, index, content.Palette);
					}
					else
						throw new InvalidOperationException("SpriteMap does not contain an index for Max Densitty ClearSides type '{0}'".F(clear));
				}
				else if (SpriteMap.TryGetValue(clear, out index))
				{
					UpdateSpriteLayers(cell, Variants[renderType].First().Value, index, content.Palette);
				}
				else
					throw new InvalidOperationException("SpriteMap does not contain an index for ClearSides type '{0}'".F(clear));
			}
			else
				UpdateSpriteLayers(cell, null, 0, null);
		}
	}
}
