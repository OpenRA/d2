# d2 mod for OpenRA

1. Install OpenRA or build it from sources. Note that current release-20160508 can not be used for d2 mod. At this moment you should build from bleed

2. Copy or link the following files from your main OpenRA installation into OpenRA.Mods.D2/dependencies directory: 
```
OpenRA.Game.exe
OpenRA.Mods.Common.dll
Eluant.dll
```

3. Run ```make all``` in OpenRA.Mods.D2 directory for linux and mac. Or open OpenRA.Mods.D2.sln solution in Visual Studio and build project for windows

4. Create subdirectory named 'd2' in main OpenRA installation in directory 'mods'

5. Copy all files from this repo to directory 'd2', including OpenRA.Mods.D2.dll. But exclude OpenRA.Mods.D2 directory from copying (this is sources of OpenRA.Mods.D2.dll)

6. Launch your OpenRA, Choose d2k mod (not d2, but dune2000) and load assets for it. This is because some sprites in d2 uses d2k assets

7. Launch your OpenRA. Choose mod 'd2' in modchooser and play game.

More information here: [wiki](https://github.com/OpenRA/d2/wiki)

AUTHORS:
[OpenRA Developers](https://github.com/OpenRA/OpenRA/blob/bleed/AUTHORS)
