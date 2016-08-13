# d2 mod for OpenRA

Notes:
  * d2 will not run against the `release-20160508` version. You must build OpenRA from source (exact commit unknown).
  * The commands given below are meant to be run in a command prompt / terminal from inside of the `OpenRA.Mods.D2` directory.

1. [Build OpenRA from source](https://github.com/OpenRA/OpenRA/wiki/Compiling)

2. Install [`cake`](http://cakebuild.net/):
  * OS X: `brew install cake`
  * Windows: `choco install cake-portable`
  * GNU/Linux: https://github.com/cake-build/cake#1-install-the-cake-bootstrapper

3. Build the d2 mod:
  * GNU/Linux, OS X: Run `make`
  * Windows: Run `make.ps1` in PowerShell

4. Alternatively, instead of installing `cake` you can:
  * Put `d2` directory into OpenRA sources in `mods` directory
  * Copy or link the following files from your main OpenRA installation into `OpenRA.Mods.D2/dependencies` directory:
    ```
    OpenRA.Game.exe,
    OpenRA.Mods.Common.dll,
    Eluant.dll
    ```
  * Run `make all` in `OpenRA.Mods.D2` directory for linux and mac. Or open `OpenRA.Mods.D2.sln` solution in Visual Studio and build project for windows

5. Launch OpenRA and install the D2k assets if you do not have them already:
  * Click the D2k option 
  * Click `Manage Content`
  * Click `Download` (in the `Base Game Files` row)
  * Click `Back`

5. Place these files in your [Content directory for d2](https://github.com/OpenRA/OpenRA/wiki/Game-Content#manual-installation) (where `$MOD` is `d2`):
  * DUNE.PAK
  * VOC.PAK
  * ATRE.PAK
  * HARK.PAK
  * ORDOS.PAK
  * INTRO.PAK
  * FINALE.PAK

6. Choose the Dune 2 mod, click `Play`, and enjoy

More information here: [wiki](https://github.com/OpenRA/d2/wiki)

AUTHORS:
[OpenRA Developers](https://github.com/OpenRA/OpenRA/blob/bleed/AUTHORS)
