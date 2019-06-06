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
using System.IO;
using OpenRA.FileSystem;

namespace OpenRA.Mods.D2.ImportData
{
	public class D2ImportOriginalMaps
	{
		public static int ImportOriginalMaps(ModData modData, Dictionary<string, string> info)
		{
			string[] files = { };
			var unpackedFilesCount = 0;

			if (info.ContainsKey("OriginalMaps"))
				files = info["OriginalMaps"].Split(',');

			if (files.Length == 0)
				return 0;

			var path = Platform.ResolvePath(Platform.SupportDir);

			var contentPath = Path.Combine(path, "maps");
			if (!Directory.Exists(contentPath))
				Directory.CreateDirectory(contentPath);

			var d2Path = Path.Combine(contentPath, "d2");
			if (!Directory.Exists(d2Path))
				Directory.CreateDirectory(d2Path);

			var originalPath = Path.Combine(d2Path, "original");
			if (!Directory.Exists(originalPath))
				Directory.CreateDirectory(originalPath);

			foreach (var s in files)
			{
				var filename = s.Trim();
				var mapFilename = Path.Combine(originalPath, Path.GetFileNameWithoutExtension(filename) + ".oramap");
				try
				{
					if (!File.Exists(mapFilename))
					{
						if (modData.DefaultFileSystem.Exists(filename))
						{
							var rules = Ruleset.LoadDefaults(modData);
							var map = D2MapImporter.Import(filename, modData.Manifest.Id, "arrakis2", rules);

							if (map != null)
							{
								map.Save(ZipFileLoader.Create(mapFilename));
								Console.WriteLine("Original map {0} saved to {1}", filename, mapFilename);
							}
						}
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
				}
			}

			return unpackedFilesCount;
		}
	}
}
