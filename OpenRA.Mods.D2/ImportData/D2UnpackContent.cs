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
using OpenRA;

namespace OpenRA.Mods.D2.ImportData
{
	public class D2UnpackContent
	{
		public static int UnpackFiles(ModData modData, Dictionary<string, string> info)
		{
			string[] files = {};
			var unpackedFilesCount = 0;

			if (info.ContainsKey("UnpackFiles"))
				files = info["UnpackFiles"].Split(',');

			if (files.Length == 0)
				return 0;

			var path = Platform.ResolvePath(Platform.SupportDir);

			var contentPath = Path.Combine(path, "Content");
			if (!Directory.Exists(contentPath))
				Directory.CreateDirectory(contentPath);

			var d2Path = Path.Combine(contentPath, "d2");
			if (!Directory.Exists(d2Path))
				Directory.CreateDirectory(d2Path);

			var unpackedPath = Path.Combine(d2Path, "Unpacked");
			if (!Directory.Exists(unpackedPath))
				Directory.CreateDirectory(unpackedPath);

			foreach (var s in files)
			{
				var originalFileName = s.Trim();
				var fileName = originalFileName;

				var explicitSplit = s.IndexOf(':');
				if (explicitSplit > 0)
				{
					originalFileName = s.Substring(0, explicitSplit).Trim();
					fileName = s.Substring(explicitSplit + 1).Trim();
				}

				try
				{
					if (modData.DefaultFileSystem.Exists(originalFileName))
					{
						using (var stream = modData.DefaultFileSystem.Open(originalFileName))
						{
							var newFileName = Path.Combine(unpackedPath, fileName);
							if (!File.Exists(newFileName))
							{
								using (FileStream fs = new FileStream(newFileName, FileMode.CreateNew, FileAccess.Write))
 								{
									stream.CopyTo(fs);
									unpackedFilesCount += 1;
									Console.WriteLine("Successfully unpacked file: {0}", fileName);
								}
							}
						}
					}
				}
				catch (Exception)
				{
				}
			}

			return unpackedFilesCount;
		}
	}
}
