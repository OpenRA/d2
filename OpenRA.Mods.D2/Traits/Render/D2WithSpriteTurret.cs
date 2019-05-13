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
using OpenRA.Graphics;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Mods.Common.Traits;

namespace OpenRA.Mods.D2.Traits.Render
{
	[Desc("Renders turrets for units with the Turreted trait.")]
	public class D2WithSpriteTurretInfo : WithSpriteTurretInfo
	{
		[Desc("Preview by default disabled in game and enabled in editor.")]
		[SequenceReference] public readonly bool previewDisabled = true;

		public override object Create(ActorInitializer init) { return new WithSpriteTurret(init.Self, this); }

		public override IEnumerable<IActorPreview> RenderPreviewSprites(ActorPreviewInitializer init, RenderSpritesInfo rs, string image, int facings, PaletteReference p)
		{
			if (previewDisabled && init.World.Type == WorldType.Regular)
				return new IActorPreview[0];

			return base.RenderPreviewSprites(init, rs, image, facings, p);
		}
	}
}
