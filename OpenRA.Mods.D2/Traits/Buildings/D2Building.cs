#region Copyright & License Information
/*
 * Copyright 2007-2021 The d2 mod Developers (see AUTHORS)
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
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.D2.Traits
{
	public class D2BuildingInfo : BuildingInfo
	{
		[Desc("All cells should be explored")]
		public readonly bool AllCellsShouldBeExplored = true;

		[Desc("Any cell should be explored")]
		public readonly bool AnyCellShouldBeExplored = false;

		[Desc("All cells should be not hidden by fog")]
		public readonly bool AllCellsShouldBeVisible = false;

		[Desc("Any cell should be not hidden by fog")]
		public readonly bool AnyCellShouldBeVisible = true;

		[Desc("Amount of damage received per DamageInterval ticks.")]
		public readonly int Damage = 10;

		[Desc("Delay between receiving damage.")]
		public readonly int DamageInterval = 100;

		[Desc("Apply the damage using these damagetypes.")]
		public readonly BitSet<DamageType> DamageTypes = default(BitSet<DamageType>);

		[Desc("Terrain types where the actor will take damage.")]
		public readonly string[] DamageTerrainTypes = { "Rock" };

		[Desc("Percentage health below which the actor will not receive further damage.")]
		public readonly int DamageThreshold = 50;

		[Desc("Inflict damage down to the DamageThreshold when the actor gets created on damaging terrain.")]
		public readonly bool StartOnThreshold = true;

		[Desc("Place building on concrete")]
		public readonly bool LaysOnConcrete = false;

		[Desc("The terrain template to place when adding a concrete foundation. " +
			"If the template is PickAny, then the actor footprint will be filled with this tile.")]
		public readonly ushort ConcreteTemplate = 0;

		[Desc("List of required prerequisites to place a terrain template.")]
		public readonly string[] ConcretePrerequisites = { };

		public override object Create(ActorInitializer init) { return new D2Building(init, this); }
		public override bool IsCloseEnoughToBase(World world, Player p, ActorInfo ai, CPos topLeft)
		{
			if (base.IsCloseEnoughToBase(world, p, ai, topLeft))
				return true;

			var requiresBuildableArea = ai.TraitInfoOrDefault<RequiresBuildableAreaInfo>();
			var mapBuildRadius = world.WorldActor.Trait<MapBuildRadius>();

			if (requiresBuildableArea == null || p.PlayerActor.Trait<DeveloperMode>().BuildAnywhere)
				return true;

			if (mapBuildRadius.BuildRadiusEnabled && RequiresBaseProvider && FindBaseProvider(world, p, topLeft) == null)
				return false;

			var adjacent = requiresBuildableArea.Adjacent;
			var buildingMaxBounds = Dimensions;

			var scanStart = world.Map.Clamp(topLeft - new CVec(adjacent, adjacent));
			var scanEnd = world.Map.Clamp(topLeft + buildingMaxBounds + new CVec(adjacent, adjacent));

			var nearnessCandidates = new List<CPos>();
			var allyBuildEnabled = mapBuildRadius.AllyBuildRadiusEnabled;

			var co = world.WorldActor.Trait<D2ConcreteOwners>();
			var shroud = p.Shroud;

			var hasVisibleCells = false;
			var hasExploredCells = false;

			for (var y = topLeft.Y; y < topLeft.Y + Dimensions.Y; y++)
			{
				for (var x = topLeft.X; x < topLeft.X + Dimensions.X; x++)
				{
					var pos = new CPos(x, y);

					if (!shroud.IsExplored(pos))
					{
						if (AllCellsShouldBeExplored)
							return false;
					}
					else
						hasExploredCells = true;

					if (!shroud.IsVisible(pos))
					{
						if (AllCellsShouldBeVisible)
							return false;
					}
					else
						hasVisibleCells = true;
				}
			}

			if (AnyCellShouldBeExplored && !hasExploredCells)
				return false;
			if (AnyCellShouldBeVisible && !hasVisibleCells)
				return false;

			for (var y = scanStart.Y; y < scanEnd.Y; y++)
			{
				for (var x = scanStart.X; x < scanEnd.X; x++)
				{
					var pos = new CPos(x, y);

					if (shroud.IsExplored(pos) && shroud.IsVisible(pos))
					{
						var ownerAtPos = co.GetOwnerAt(pos);

						if (ownerAtPos != null && (ownerAtPos == p || (allyBuildEnabled && ownerAtPos.RelationshipWith(p) == PlayerRelationship.Ally)))
							nearnessCandidates.Add(pos);
					}
				}
			}

			var buildingTiles = Tiles(topLeft).ToList();
			return nearnessCandidates
				.Any(a => buildingTiles
					.Any(b => Math.Abs(a.X - b.X) <= adjacent
						&& Math.Abs(a.Y - b.Y) <= adjacent));
		}
	}

	/*
	 * D2Building is based on D2kBuilding from d2k mod
	 */
	public class D2Building : Building, ITick, INotifyCreated
	{
		readonly D2BuildingInfo info;

		D2BuildableTerrainLayer layer;
		IHealth health;
		int safeTiles;
		int totalTiles;
		int damageThreshold;
		int damageTicks;
		TechTree techTree;
		BuildingInfluence bi;

		public D2Building(ActorInitializer init, D2BuildingInfo info)
			: base(init, info)
		{
			this.info = info;
		}

		void INotifyCreated.Created(Actor self)
		{
			health = self.TraitOrDefault<IHealth>();
			layer = self.World.WorldActor.TraitOrDefault<D2BuildableTerrainLayer>();
			bi = self.World.WorldActor.Trait<BuildingInfluence>();
			techTree = self.Owner.PlayerActor.TraitOrDefault<TechTree>();
		}

		protected override void AddedToWorld(Actor self)
		{
			base.AddedToWorld(self);

			if (info.LaysOnConcrete)
			{
				if (layer != null && (!info.ConcretePrerequisites.Any() || techTree == null || techTree.HasPrerequisites(info.ConcretePrerequisites)))
				{
					var map = self.World.Map;
					var template = map.Rules.TileSet.Templates[info.ConcreteTemplate];
					if (template.PickAny)
					{
						// Fill the footprint with random variants
						foreach (var c in info.Tiles(self.Location))
						{
							// Only place on allowed terrain types
							if (!map.Contains(c) || map.CustomTerrain[c] != byte.MaxValue || !info.TerrainTypes.Contains(map.GetTerrainInfo(c).Type))
								continue;

							// Don't place under other buildings (or their bib)
							if (bi.GetBuildingAt(c) != self)
								continue;

							var index = Game.CosmeticRandom.Next(template.TilesCount);
							layer.AddTile(c, new TerrainTile(template.Id, (byte)index));
						}
					}
					else
					{
						for (var i = 0; i < template.TilesCount; i++)
						{
							var c = self.Location + new CVec(i % template.Size.X, i / template.Size.X);

							// Only place on allowed terrain types
							if (!map.Contains(c) || map.CustomTerrain[c] != byte.MaxValue || !info.TerrainTypes.Contains(map.GetTerrainInfo(c).Type))
								continue;

							// Don't place under other buildings (or their bib)
							if (bi.GetBuildingAt(c) != self)
								continue;

							layer.AddTile(c, new TerrainTile(template.Id, (byte)i));
						}
					}
				}
			}

			if (health == null)
				return;

			foreach (var kv in self.OccupiesSpace.OccupiedCells())
			{
				totalTiles++;
				if (!info.DamageTerrainTypes.Contains(self.World.Map.GetTerrainInfo(kv.Cell).Type))
					safeTiles++;
			}

			if (totalTiles == 0 || totalTiles == safeTiles)
				return;

			// Cast to long to avoid overflow when multiplying by the health
			damageThreshold = (int)((info.DamageThreshold * (long)health.MaxHP + (100 - info.DamageThreshold) * safeTiles * (long)health.MaxHP / totalTiles) / 100);

			if (!info.StartOnThreshold)
				return;

			// Start with maximum damage applied
			var delta = health.HP - damageThreshold;
			if (delta > 0)
				self.InflictDamage(self.World.WorldActor, new Damage(delta, info.DamageTypes));
		}

		void ITick.Tick(Actor self)
		{
			if (info.DamageInterval == 0 || info.Damage == 0 || totalTiles == safeTiles || health.HP <= damageThreshold || --damageTicks > 0)
				return;

			self.InflictDamage(self.World.WorldActor, new Damage(info.Damage, info.DamageTypes));
			damageTicks = info.DamageInterval;
		}
	}
}
