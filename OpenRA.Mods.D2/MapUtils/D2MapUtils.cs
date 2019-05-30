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
using System.Collections.Generic;

namespace OpenRA.Mods.D2.MapUtils
{
	public static class D2MapUtils
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

		public const ushort SandTile = 0;
		public const ushort ConcreteTile = 126;
		public const ushort RockTile = 143;
		public const ushort DuneTile = 159;
		public const ushort RoughTile = 175;

		public static readonly Dictionary<ClearSides, ushort> SpriteMap = new Dictionary<ClearSides, ushort>()
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

		static readonly ushort[] RockSides = { 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143 };
		static readonly ushort[] DuneSides = { 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159 };
		static readonly ushort[] RoughSides = { 160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175 };

		public static ushort SmoothTileTypeForPos(ushort[] m, ushort width, ushort height, ushort x, ushort y)
		{
			var index = y * width + x;
			var tile = m[index];

			if (tile == RockTile)
			{
				ClearSides clear = ClearSides.None;

				if (x > 0)
				{
					var leftIndex = index - 1;
					var leftTile = m[leftIndex];
					if (!(leftTile >= 126 && leftTile <= 143) &&
					    !(leftTile >= 160 && leftTile <= 175))
					{
						clear |= ClearSides.Left;
					}
				}

				if (y > 0)
				{
					var topIndex = index - width;
					var topTile = m[topIndex];
					if (!(topTile >= 126 && topTile <= 143) &&
					    !(topTile >= 160 && topTile <= 175))
					{
						clear |= ClearSides.Top;
					}
				}

				if (x < width - 1)
				{
					var rightIndex = index + 1;
					var rightTile = m[rightIndex];
					if (!(rightTile >= 126 && rightTile <= 143) &&
					    !(rightTile >= 160 && rightTile <= 175))
					{
						clear |= ClearSides.Right;
					}
				}

				if (y < height - 1)
				{
					var bottomIndex = index + width;
					var bottomTile = m[bottomIndex];
					if (!(bottomTile >= 126 && bottomTile <= 143) &&
					    !(bottomTile >= 160 && bottomTile <= 175))
					{
						clear |= ClearSides.Bottom;
					}
				}

				if (clear != ClearSides.None)
				{
					return RockSides[SpriteMap[clear]];
				}
			}

			if (tile == DuneTile)
			{
				ClearSides clear = ClearSides.None;

				if (x > 0)
				{
					var leftIndex = index - 1;
					var leftTile = m[leftIndex];
					if (!(leftTile >= 144 && leftTile <= 159))
					{
						clear |= ClearSides.Left;
					}
				}

				if (y > 0)
				{
					var topIndex = index - width;
					var topTile = m[topIndex];
					if (!(topTile >= 144 && topTile <= 159))
					{
						clear |= ClearSides.Top;
					}
				}

				if (x < width - 1)
				{
					var rightIndex = index + 1;
					var rightTile = m[rightIndex];
					if (!(rightTile >= 144 && rightTile <= 159))
					{
						clear |= ClearSides.Right;
					}
				}

				if (y < height - 1)
				{
					var bottomIndex = index + width;
					var bottomTile = m[bottomIndex];
					if (!(bottomTile >= 144 && bottomTile <= 159))
					{
						clear |= ClearSides.Bottom;
					}
				}

				if (clear != ClearSides.None)
				{
					return DuneSides[SpriteMap[clear]];
				}
			}

			if (tile == RoughTile)
			{
				ClearSides clear = ClearSides.None;

				if (x > 0)
				{
					var leftIndex = index - 1;
					var leftTile = m[leftIndex];
					if (!(leftTile >= 160 && leftTile <= 175))
					{
						clear |= ClearSides.Left;
					}
				}

				if (y > 0)
				{
					var topIndex = index - width;
					var topTile = m[topIndex];
					if (!(topTile >= 160 && topTile <= 175))
					{
						clear |= ClearSides.Top;
					}
				}

				if (x < width - 1)
				{
					var rightIndex = index + 1;
					var rightTile = m[rightIndex];
					if (!(rightTile >= 160 && rightTile <= 175))
					{
						clear |= ClearSides.Right;
					}
				}

				if (y < height - 1)
				{
					var bottomIndex = index + width;
					var bottomTile = m[bottomIndex];
					if (!(bottomTile >= 160 && bottomTile <= 175))
					{
						clear |= ClearSides.Bottom;
					}
				}

				if (clear != ClearSides.None)
				{
					return RoughSides[SpriteMap[clear]];
				}
			}

			return tile;
		}

		public static ushort SmoothIndexForPos(CellLayer<TerrainTile> tiles, MPos pos)
		{
			var tile = tiles[pos].Type;

			if (tile == RockTile)
			{
				ClearSides clear = ClearSides.None;

				if (pos.U > 0)
				{
					var leftPos = new MPos(pos.U - 1, pos.V);
					var leftTile = tiles[leftPos];
					if (!(leftTile.Type >= 126 && leftTile.Type <= 143) &&
					    !(leftTile.Type >= 160 && leftTile.Type <= 175))
					{
						clear |= ClearSides.Left;
					}
				}

				if (pos.V > 0)
				{
					var topPos = new MPos(pos.U, pos.V - 1);
					var topTile = tiles[topPos];
					if (!(topTile.Type >= 126 && topTile.Type <= 143) &&
					    !(topTile.Type >= 160 && topTile.Type <= 175))
					{
						clear |= ClearSides.Top;
					}
				}

				if (pos.U < tiles.Size.Width - 1)
				{
					var rightPos = new MPos(pos.U + 1, pos.V);
					var rightTile = tiles[rightPos];
					if (!(rightTile.Type >= 126 && rightTile.Type <= 143) &&
					    !(rightTile.Type >= 160 && rightTile.Type <= 175))
					{
						clear |= ClearSides.Right;
					}
				}

				if (pos.V < tiles.Size.Height - 1)
				{
					var bottomPos = new MPos(pos.U, pos.V + 1);
					var bottomTile = tiles[bottomPos];
					if (!(bottomTile.Type >= 126 && bottomTile.Type <= 143) &&
					    !(bottomTile.Type >= 160 && bottomTile.Type <= 175))
					{
						clear |= ClearSides.Bottom;
					}
				}

				if (clear != ClearSides.None)
				{
					return SpriteMap[clear];
				}
			}

			if (tile == DuneTile)
			{
				ClearSides clear = ClearSides.None;

				if (pos.U > 0)
				{
					var leftPos = new MPos(pos.U - 1, pos.V);
					var leftTile = tiles[leftPos];
					if (!(leftTile.Type >= 144 && leftTile.Type <= 159))
					{
						clear |= ClearSides.Left;
					}
				}

				if (pos.V > 0)
				{
					var topPos = new MPos(pos.U, pos.V - 1);
					var topTile = tiles[topPos];
					if (!(topTile.Type >= 144 && topTile.Type <= 159))
					{
						clear |= ClearSides.Top;
					}
				}

				if (pos.U < tiles.Size.Width - 1)
				{
					var rightPos = new MPos(pos.U + 1, pos.V);
					var rightTile = tiles[rightPos];
					if (!(rightTile.Type >= 144 && rightTile.Type <= 159))
					{
						clear |= ClearSides.Right;
					}
				}

				if (pos.V < tiles.Size.Height - 1)
				{
					var bottomPos = new MPos(pos.U, pos.V + 1);
					var bottomTile = tiles[bottomPos];
					if (!(bottomTile.Type >= 144 && bottomTile.Type <= 159))
					{
						clear |= ClearSides.Bottom;
					}
				}

				if (clear != ClearSides.None)
				{
					return SpriteMap[clear];
				}
			}

			if (tile == RoughTile)
			{
				ClearSides clear = ClearSides.None;

				if (pos.U > 0)
				{
					var leftPos = new MPos(pos.U - 1, pos.V);
					var leftTile = tiles[leftPos];
					if (!(leftTile.Type >= 160 && leftTile.Type <= 175))
					{
						clear |= ClearSides.Left;
					}
				}

				if (pos.V > 0)
				{
					var topPos = new MPos(pos.U, pos.V - 1);
					var topTile = tiles[topPos];
					if (!(topTile.Type >= 160 && topTile.Type <= 175))
					{
						clear |= ClearSides.Top;
					}
				}

				if (pos.U < tiles.Size.Width - 1)
				{
					var rightPos = new MPos(pos.U + 1, pos.V);
					var rightTile = tiles[rightPos];
					if (!(rightTile.Type >= 160 && rightTile.Type <= 175))
					{
						clear |= ClearSides.Right;
					}
				}

				if (pos.V < tiles.Size.Height - 1)
				{
					var bottomPos = new MPos(pos.U, pos.V + 1);
					var bottomTile = tiles[bottomPos];
					if (!(bottomTile.Type >= 160 && bottomTile.Type <= 175))
					{
						clear |= ClearSides.Bottom;
					}
				}

				if (clear != ClearSides.None)
				{
					return SpriteMap[clear];
				}
			}

			return SpriteMap[ClearSides.None];
		}
	}
}
