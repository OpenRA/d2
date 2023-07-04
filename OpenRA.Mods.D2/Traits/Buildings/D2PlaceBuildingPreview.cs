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
using OpenRA.Graphics;
using OpenRA.Mods.Common.Orders;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.D2.Graphics;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.D2.Traits
{
	[Desc("Creates a d2 building placement preview.")]
	public class D2PlaceBuildingPreviewInfo : FootprintPlaceBuildingPreviewInfo, Requires<D2BuildingInfo>
	{
		[Desc("Speed of footprint animation.")]
		public float ColorAnimationStep = 0.04f;

		[Desc("If true, all tiles should be buildable for drawing without cross, if false - any tile can be buildable for drawing without cross.")]
		public bool StrictBuildableChecks = true;

		protected override IPlaceBuildingPreview CreatePreview(WorldRenderer wr, ActorInfo ai, TypeDictionary init)
		{
			return new D2PlaceBuildingPreviewPreview(wr, ai, this);
		}

		public override object Create(ActorInitializer init)
		{
			return new D2PlaceBuildingPreview();
		}
	}

	public class D2PlaceBuildingPreview { }

	public class D2PlaceBuildingPreviewPreview : FootprintPlaceBuildingPreviewPreview
	{
		protected readonly D2PlaceBuildingPreviewInfo info;
		protected readonly ActorInfo ai;
		protected readonly D2BuildingInfo bi;

		protected readonly int cols;
		protected readonly int rows;
		protected readonly Rectangle bounds;

		protected float t = 0.0f;

		public D2PlaceBuildingPreviewPreview(WorldRenderer wr, ActorInfo ai, D2PlaceBuildingPreviewInfo info)
			: base(wr, ai, info)
		{
			this.info = info;
			this.ai = ai;

			bi = ai.TraitInfo<D2BuildingInfo>();

			cols = bi.Dimensions.X;
			rows = bi.Dimensions.Y;

			var halfWidth = cols * 512;
			var halfHeight = rows * 512;

			var width = halfWidth * 2;
			var height = halfHeight * 2;

			bounds = new Rectangle(-halfWidth, -halfHeight, width, height);
		}

		protected override void TickInner()
		{
			if (info.ColorAnimationStep == 0.0f)
				return;

			t += info.ColorAnimationStep;

			if (t >= 1.0f || t <= 0)
				info.ColorAnimationStep = -info.ColorAnimationStep;

			if (t > 1.0f)
				t = 1.0f;

			if (t < 0.0f)
				t = 0.0f;
		}

		protected bool IsCloseEnoughAndBuildable(World w, ActorInfo ai, CPos topLeft)
		{
			var isCloseEnough = bi.IsCloseEnoughToBase(w, w.LocalPlayer, ai, topLeft);
			var isAllBuildable = true;
			var isAnyBuildable = false;

			for (var y = 0; y < rows; y++)
			{
				for (var x = 0; x < cols; x++)
				{
					var cellPos = new CPos(topLeft.X + x, topLeft.Y + y);
					if (w.IsCellBuildable(cellPos, ai, bi))
					{
						isAnyBuildable = true;
						if (!info.StrictBuildableChecks)
							break;
					}
					else
					{
						isAllBuildable = false;
						if (info.StrictBuildableChecks)
							break;
					}
				}

				if (isAnyBuildable && !info.StrictBuildableChecks)
					break;

				if (!isAllBuildable && info.StrictBuildableChecks)
					break;
			}

			if (info.StrictBuildableChecks)
				return isCloseEnough && isAllBuildable;
			else
				return isCloseEnough && isAnyBuildable;
		}

		protected override IEnumerable<IRenderable> RenderInner(WorldRenderer wr, CPos topLeft, Dictionary<CPos, PlaceBuildingCellType> footprint)
		{
			var centerPosition = wr.World.Map.CenterOfCell(topLeft) + CenterOffset;

			var colorComponent = (int)Math.Round(127 + 127 * t);
			var color = Color.FromArgb(255, colorComponent, colorComponent, colorComponent);

			var cross = !IsCloseEnoughAndBuildable(wr.World, ActorInfo, topLeft);

			yield return new D2BuildingPlacementRenderable(centerPosition, bounds, color, cross);
		}
	}
}
