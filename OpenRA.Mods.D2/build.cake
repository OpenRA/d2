// OS X: brew install cake
// Windows: choco install cake.portable
//
// Example:
// cake -openra-root=~/projects/openra

var target = Argument("target", "default").ToLowerInvariant();
var configuration = Argument("configuration", "Debug");

var depsDir = Directory("dependencies");

// TODO: Combine 'deps' and 'depsInOpenRA' into an array of pairs/structs
var deps = new[] {
    "Eluant.dll",
    "OpenRA.Game.exe",
    "OpenRA.Mods.Common.dll",
    "OpenRA.Mods.D2k.dll"
};

// The path to the assembly relative to the OpenRA root
var depsInOpenRA = new[] {
    "Eluant.dll",
    "OpenRA.Game.exe",
    "mods/common/OpenRA.Mods.Common.dll",
    "mods/d2k/OpenRA.Mods.D2k.dll"
};

// Location on-disk of the OpenRA source code.
var openraRoot = Argument<string>("openra-root", null);

Task("deps").Does(() => {
    var missingDeps = new List<string>();
    foreach (var dep in deps)
    {
        var fullPath = System.IO.Path.Combine(depsDir.Path.FullPath, dep);
        if (!System.IO.File.Exists(fullPath))
            missingDeps.Add(dep);
    }

    if (!missingDeps.Any())
    {
        Information("All dependencies accounted for. Aborting 'deps' task.");
        return;
    }

    // Steps to resolving dependency location:
    //   1) -openra-root=<path> command-line argument
    //   2) environment variable OPENRA_ROOT
    //   3) Ask the user for the path

    if (string.IsNullOrWhiteSpace(openraRoot))
        openraRoot = Environment.GetEnvironmentVariable("OPENRA_ROOT");

    if (string.IsNullOrWhiteSpace(openraRoot))
    {
        Console.Write("Please enter the path to the OpenRA root: ");
        openraRoot = Console.ReadLine();
    }

    if (string.IsNullOrWhiteSpace(openraRoot))
    {
        Error("OpenRA root path could not be resolved!");
        Environment.Exit(1);
    }

    if (openraRoot.StartsWith("~"))
        openraRoot = openraRoot.Replace("~", IsRunningOnWindows() ?
            Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") :
            Environment.GetEnvironmentVariable("HOME"));

    System.IO.Directory.CreateDirectory(depsDir.Path.FullPath);

    var missingDepsCopy = missingDeps.ToArray();
    for (var i = 0; i < missingDepsCopy.Length; i++)
    {
        var dep = missingDepsCopy[i];
        var depPathInOpenRA = depsInOpenRA[i];

        var depPath = System.IO.Path.Combine(depsDir.Path.FullPath, dep);
        var oraPath = System.IO.Path.Combine(openraRoot, depPathInOpenRA);

        if (!System.IO.File.Exists(oraPath))
            Error(string.Format("Could not automatically resolve missing dependency '{0}'.", dep));
        else
        {
            System.IO.File.Copy(oraPath, depPath, true);
            if (System.IO.File.Exists(depPath))
                missingDeps.Remove(dep);
        }
    }

    if (missingDeps.Any())
    {
        Error(string.Format("Missing {0} dependencies.", missingDeps.Count));
        Environment.Exit(1);
    }
});

Task("default")
    .IsDependentOn("deps")
    .Does(() => {
        if (IsRunningOnWindows())
            MSBuild("OpenRA.Mods.D2.sln", settings => settings.SetConfiguration(configuration));
        else
            XBuild("OpenRA.Mods.D2.sln", settings => settings.SetConfiguration(configuration));

        System.IO.File.Copy("./bin/Debug/OpenRA.Mods.D2.dll", "../OpenRA.Mods.D2.dll", true);
});

Task("clean") .Does(() => {
    DeleteFiles("./bin/*/*.dll");
    DeleteFiles("./bin/*/*.exe");
    DeleteFiles("./obj/*/*.dll");
    DeleteFiles("./obj/*/*.exe");
    DeleteFiles("./dependencies/*.exe");
    DeleteFiles("./dependencies/*.dll");

    if (FileExists("OpenRA.Mods.D2.dll"))
        DeleteFile("OpenRA.Mods.D2.dll");

    if (FileExists("../OpenRA.Mods.D2.dll"))
        DeleteFile("../OpenRA.Mods.D2.dll");
});

RunTarget(target);
