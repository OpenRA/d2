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
 * Based on Dune Dynasty landscape.c (https://sourceforge.net/p/dunedynasty/dunedynasty/ci/master/tree/src/mods/landscape.c)
 * Dune Dynasty Written by:
 *   (c) David Wang <dswang@users.sourceforge.net>
 */
#endregion

using System;

namespace OpenRA.Mods.D2.MathExtention
{
	public static class D2MathExtention
	{
		public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
		{
			if (val.CompareTo(min) < 0)
			{
				return min;
			}
			else if (val.CompareTo(max) > 0)
			{
				return max;
			}
			else
			{
				return val;
			}
		}
	}
}
