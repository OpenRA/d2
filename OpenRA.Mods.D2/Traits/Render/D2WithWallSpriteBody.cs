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
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.D2.Traits.Render
{
	[RequireExplicitImplementation]
	interface ID2WallConnectorInfo : ITraitInfoInterface
	{
		string GetWallConnectionType();
	}

	interface ID2WallConnector
	{
		bool AdjacentWallCanConnect(Actor self, CPos wallLocation, string wallType, out CVec facing);
		void SetDirty();
	}

	[Desc("Render trait for actors that change sprites if neighbors with the same trait are present.")]
	class D2WithWallSpriteBodyInfo : D2WithSpriteBodyInfo, ID2WallConnectorInfo, Requires<BuildingInfo>
	{
		public readonly string Type = "wall";

		public override object Create(ActorInitializer init) { return new D2WithWallSpriteBody(init, this); }

		string ID2WallConnectorInfo.GetWallConnectionType()
		{
			return Type;
		}
	}

	class D2WithWallSpriteBody : D2WithSpriteBody, INotifyRemovedFromWorld, ID2WallConnector, ITick
	{
		readonly D2WithWallSpriteBodyInfo wallInfo;
		int adjacent = 0;
		bool dirty = true;

		bool ID2WallConnector.AdjacentWallCanConnect(Actor self, CPos wallLocation, string wallType, out CVec facing)
		{
			facing = wallLocation - self.Location;
			return wallInfo.Type == wallType && Math.Abs(facing.X) + Math.Abs(facing.Y) == 1;
		}

		void ID2WallConnector.SetDirty() { dirty = true; }

		public D2WithWallSpriteBody(ActorInitializer init, D2WithWallSpriteBodyInfo info)
			: base(init, info, () => 0)
		{
			wallInfo = info;
		}

		protected override void DamageStateChanged(Actor self)
		{
			DefaultAnimation.PlayFetchIndex(NormalizeSequence(self, Info.Sequence), () => adjacent);
		}

		void ITick.Tick(Actor self)
		{
			if (!dirty)
				return;

			// Update connection to neighbours
			var adjacentActors = CVec.Directions.SelectMany(dir =>
				self.World.ActorMap.GetActorsAt(self.Location + dir));

			adjacent = 0;
			foreach (var a in adjacentActors)
			{
				CVec facing;
				var wc = a.TraitsImplementing<ID2WallConnector>().FirstEnabledTraitOrDefault();
				if (wc == null || !wc.AdjacentWallCanConnect(a, self.Location, wallInfo.Type, out facing))
					continue;

				if (facing.Y > 0)
					adjacent |= 1;
				else if (facing.X < 0)
					adjacent |= 2;
				else if (facing.Y < 0)
					adjacent |= 4;
				else if (facing.X > 0)
					adjacent |= 8;
			}

			dirty = false;
		}

		protected override void OnBuildComplete(Actor self)
		{
			DefaultAnimation.PlayFetchIndex(NormalizeSequence(self, Info.Sequence), () => adjacent);
			UpdateNeighbours(self);

			// Set the initial animation frame before the render tick (for frozen actor previews)
			self.World.AddFrameEndTask(_ => DefaultAnimation.Tick());
		}

		static void UpdateNeighbours(Actor self)
		{
			var adjacentActorTraits = CVec.Directions.SelectMany(dir =>
					self.World.ActorMap.GetActorsAt(self.Location + dir))
				.SelectMany(a => a.TraitsImplementing<ID2WallConnector>());

			foreach (var aat in adjacentActorTraits)
				aat.SetDirty();
		}

		void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
		{
			UpdateNeighbours(self);
		}

		protected override void TraitEnabled(Actor self) { dirty = true; }
	}

	public class RuntimeNeighbourInit : IActorInit<Dictionary<CPos, string[]>>, ISuppressInitExport
	{
		[FieldFromYamlKey] readonly Dictionary<CPos, string[]> value = null;
		public RuntimeNeighbourInit() { }
		public RuntimeNeighbourInit(Dictionary<CPos, string[]> init) { value = init; }
		public Dictionary<CPos, string[]> Value(World world) { return value; }
	}
}
