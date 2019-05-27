#region Copyright & License Information
/*
 * Copyright 2007-2019 The d2 mod Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
/*
 * Based on Dune Dynasty random_general.c (https://sourceforge.net/p/dunedynasty/dunedynasty/ci/master/tree/src/tools/random_general.c)
 * Dune Dynasty Written by:
 *   (c) David Wang <dswang@users.sourceforge.net>
 */
#endregion

using System;

namespace OpenRA.Mods.D2.ImportData
{
	public class D2MapSeed
	{
		byte[] seed = new byte[4];

		public UInt32 Seed
		{
			get
			{
				return (UInt32)(seed[0]) 
					+ (UInt32)(seed[1] << 8)
					+ (UInt32)(seed[2] << 16)
					+ (UInt32)(seed[3] << 24);
			}
		}

		public D2MapSeed(UInt32 seed)
		{
			this.seed[0] = (byte)(seed & 0xFF);
			this.seed[1] = (byte)((seed >> 8) & 0xFF);
			this.seed[2] = (byte)((seed >> 16) & 0xFF);
			this.seed[3] = (byte)((seed >> 24) & 0xFF);
		}

		public byte Random()
		{
			var carry0 = (byte)((seed[0] >> 1) & 0x01);
			var carry2 = (byte)(seed[2] >> 7);
			seed[2] = (byte)((seed[2] << 1) | carry0);

			var carry1 = (byte)(seed[1] >> 7);
			seed[1] = (byte)((seed[1] << 1) | carry2);

			var carry = (byte)(((seed[0] >> 2) - seed[0] - (carry1 ^ 0x01)) & 0x01);
			seed[0] = (byte)((carry << 7) | (seed[0] >> 1));

			return (byte)(seed[0] ^ seed[1]);
		}
	}
}
