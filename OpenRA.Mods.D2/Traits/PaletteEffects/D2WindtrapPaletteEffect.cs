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

using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.D2.Traits
{
	[Desc("Palette effect used for windtrap \"animations\".")]
	class D2WindtrapPaletteEffectInfo : ITraitInfo
	{
		[Desc("Palette for effect.")]
		public string PaletteName = "player";

		[Desc("Palette index where Rotated color will be copied.")]
		public readonly int RotationIndex = 223;

		[Desc("Palette index of first RotationRange color.")]
		public readonly int RotationBase = 160;

		[Desc("Range of colors to rotate.")]
		public readonly int RotationRange = 4;

		[Desc("Step towards next color index per tick.")]
		public readonly float RotationStep = .2f;

		public object Create(ActorInitializer init) { return new D2WindtrapPaletteEffect(init.World, this); }
	}

	class D2WindtrapPaletteEffect : ITick, IPaletteModifier
	{
		readonly D2WindtrapPaletteEffectInfo info;
		float t = 0;
		float step;

		public D2WindtrapPaletteEffect(World world, D2WindtrapPaletteEffectInfo info)
		{
			this.info = info;
			step = info.RotationStep;
		}

		public void Tick(Actor self)
		{
			t += step;
			if (t >= info.RotationRange || t <= 0) step = -step;
		}

		public void AdjustPalette(IReadOnlyDictionary<string, MutablePalette> palettes)
		{
			var rotate = (int)t % (info.RotationRange + 1);

			foreach (var kvp in palettes)
			{
				if ( kvp.Key.StartsWith(info.PaletteName) )
				{
					var palette = kvp.Value;
					palette[info.RotationIndex] = palette[info.RotationBase + rotate];
				}
			}
		}
	}
}
