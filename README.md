# d2 mod for OpenRA

How to install:
  * Download [OpenRA-Bleed](https://github.com/OpenRA/OpenRA/archive/bleed.zip)
  * Download [D2-Master](https://github.com/OpenRA/d2/archive/master.zip)
  * Extract both files, rename the `d2-master` folder to `d2` and move it into the just extracted OpenRA-bleed folder  
  -> OpenRA-bleed/mods
  * Assuming that you already installed OpenRA before from the Website (not from source), you need to switch to its Directory and copy 3 files : 
    - `OpenRA.Game.exe`
    - `Eluant.dll`
    - `OpenRA.Mods.Common.dll` (OpenRA/mods/common)
  * Paste the files into `/OpenRA-release-bleed/mods/d2/OpenRA.Mods.D2/dependencies`
  * Now download the [Original Game](http://www.abandonia.com/en/games/36/Dune+II+-+The+Building+of+a+Dynasty.html) and copy following files: 
    - `DUNE.PAK`
    - `VOC.PAK`
    - `ATRE.PAK`
    - `HARK.PAK`
    - `ORDOS.PAK`
    - `INTRO.PAK`
    - `FINALE.PAK`
  * Paste those files into `Documents/OpenRA/Content/d2` (if the d2 folder doesnt exist, create it)
  * Keep sure that you atleast once started the Dune2000 mod (thus downloaded the content for it)
  * Run `make all` in `OpenRA-bleed` directory via commandline.
  * Head to `/OpenRA-bleed/mods/d2/OpenRA.Mods.D2` and run `make` (win) or `./build.sh` (linux)
  * Run ./launch-game.sh Game.Mod=d2 and select D2

-------------------------------------------------------------------------------------------------------------------------

Installing with cake

1. [Build OpenRA from source](https://github.com/OpenRA/OpenRA/wiki/Compiling)

2. Install [`cake`](http://cakebuild.net/):
   * OS X: `brew install cake`
   * Windows: `choco install cake-portable`
   * GNU/Linux: https://github.com/cake-build/cake#1-install-the-cake-bootstrapper

3. Build the d2 mod:
   * GNU/Linux, OS X: Run `make`
   * Windows: Run `make.ps1` in PowerShell


More information here: [wiki](https://github.com/OpenRA/d2/wiki)

AUTHORS:
[OpenRA Developers](https://github.com/OpenRA/OpenRA/blob/bleed/AUTHORS)
