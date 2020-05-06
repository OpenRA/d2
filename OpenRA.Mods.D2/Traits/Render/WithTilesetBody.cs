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
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.D2.Traits
{
	public class WithTilesetBodyInfo : TraitInfo, Requires<BuildingInfo>, IRenderActorPreviewSpritesInfo, Requires<RenderSpritesInfo>
	{
		[SequenceReference]
		public readonly string Sequence = "idle-tileset";

		[PaletteReference]
		public readonly string Palette = null;

		[Desc("skip drawing sprite frame because it always under idle-top animation")]
		public readonly int[] SkipFrames;

		public override object Create(ActorInitializer init) { return new WithTilesetBody(init.Self, this); }

		public IEnumerable<IActorPreview> RenderPreviewSprites(ActorPreviewInitializer init, RenderSpritesInfo rs, string image, int facings, PaletteReference p)
		{
			if (Palette != null)
				p = init.WorldRenderer.Palette(Palette);

			var bi = init.Actor.TraitInfo<BuildingInfo>();

			var cols = bi.Dimensions.X;
			var rows = bi.Dimensions.Y;

			for (var index = 0; index < (cols * rows); index++)
			{
				if (SkipFrames == null || !SkipFrames.Contains(index))
				{
					var y = index / cols;
					var x = index % cols;

					var anim = new Animation(init.World, image);
					Func<WVec> offset = () => new WVec(x * 1024 - 512, y * 1024 - 512, 0);
					Func<int> zOffset = () => 0;

					var frameIndex = index;
					anim.PlayFetchIndex(Sequence, () => frameIndex);
					anim.IsDecoration = true;

					yield return new SpriteActorPreview(anim, offset, zOffset, p, rs.Scale);
				}
			}
		}
	}

	public class WithTilesetBody
	{
		public WithTilesetBody(Actor self, WithTilesetBodyInfo info)
		{
			var rs = self.Trait<RenderSprites>();
			var bi = self.Info.TraitInfo<BuildingInfo>();

			var cols = bi.Dimensions.X;
			var rows = bi.Dimensions.Y;

			for (var index = 0; index < (cols * rows); index++)
			{
				if (info.SkipFrames == null || !info.SkipFrames.Contains(index))
				{
					var y = index / cols;
					var x = index % cols;

					var anim = new Animation(self.World, rs.GetImage(self));
					var cellOffset = new WVec(x * 1024 - 512, y * 1024 - 512, 0);

					var frameIndex = index;
					anim.PlayFetchIndex(info.Sequence, () => frameIndex);
					anim.IsDecoration = true;

					var awo = new AnimationWithOffset(anim, () => cellOffset, null, 0);
					rs.Add(awo, info.Palette);
				}
			}
		}
	}
}
