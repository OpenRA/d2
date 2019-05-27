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
using System.IO;
using OpenRA.FileSystem;
using OpenRA.Mods.D2.ImportData;

namespace OpenRA.Mods.D2.UtilityCommands
{
	class D2ImportD2MapCommand : IUtilityCommand
	{
		string IUtilityCommand.Name { get { return "--import-d2-map"; } }

		bool IUtilityCommand.ValidateArguments(string[] args)
		{
			return args.Length >= 3;
		}

		[Desc("FILENAME", "TILESET", "Convert a legacy Dune 2 MAP file to the OpenRA format.")]
		void IUtilityCommand.Run(Utility utility, string[] args)
		{
			// HACK: The engine code assumes that Game.modData is set.
			Game.ModData = utility.ModData;

			var rules = Ruleset.LoadDefaultsForTileSet(utility.ModData, "arrakis2");
			var map = D2MapImporter.Import(args[1], utility.ModData.Manifest.Id, args[2], rules);

			if (map == null)
				return;

			var dest = Path.GetFileNameWithoutExtension(args[1]) + ".oramap";
			map.Save(ZipFileLoader.Create(dest));
			Console.WriteLine(dest + " saved.");
		}
	}
}
