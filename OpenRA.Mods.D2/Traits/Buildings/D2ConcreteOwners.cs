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

using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.D2.Traits
{
	[Desc("A dictionary of concrete placed on the map. Attach this to the world actor.")]
	public class D2ConcreteOwnersInfo : TraitInfo
	{
		public override object Create(ActorInitializer init) { return new D2ConcreteOwners(init.World); }
	}

	public class D2ConcreteOwners
	{
		readonly Map map;
		readonly CellLayer<Player> owners;

		public D2ConcreteOwners(World world)
		{
			map = world.Map;

			owners = new CellLayer<Player>(map);

			world.ActorAdded += a =>
			{
				var c = a.Info.TraitInfoOrDefault<D2ConcreteInfo>();
				if (c == null)
					return;

				var b = a.Info.TraitInfoOrDefault<BuildingInfo>();
				if (b == null)
					return;

				var tiles = b.Tiles(a.Location).ToList();
				foreach (var u in tiles)
					if (owners.Contains(u) && owners[u] == null)
						owners[u] = a.Owner;
			};
		}

		public Player GetOwnerAt(CPos cell)
		{
			return owners.Contains(cell) ? owners[cell] : null;
		}
	}
}
