# d2 mod for OpenRA

This repository contains a `d2 mod` for the [OpenRA](https://github.com/OpenRA/OpenRA) engine.
It is based on [OpenRAModSDK](https://github.com/OpenRA/OpenRAModSDK) and should be updated if `OpenRAModSDK` changed

These scripts and support files from `OpenRAModSDK` wrap and automatically manage a copy of the OpenRA game engine and common files,
and provide entrypoints to run development versions and to generate platform-specific installers.

Before running `d2 mod`, download the original game and copy the following files:

 - `ATRE.PAK`
 - `DUNE.PAK`
 - `FINALE.PAK`
 - `HARK.PAK`
 - `INTRO.PAK`
 - `ORDOS.PAK`
 - `SCENARIO.PAK`
 - `VOC.PAK`

For packaged installs(like this) or launchers that do not set a local support directory, the OpenRA platform support directory is used instead:
You can also place the files there if you run the mod through a registered OpenRA launcher instead of the SDK checkout launcher.

 - `%USERPROFILE%\AppData\Roaming\OpenRA\Content\d2` (Windows)
 - `%USERPROFILE%\AppData\Roaming\OpenRA\Content\d2k\v3` (Windows d2k content)
 - `~/Library/Application Support/OpenRA/Content/d2` (macOS)
 - `~/Library/Application Support/OpenRA/Content/d2k/v3` (macOS d2k content)
 - `~/.openra/Content/d2` (Linux)
 - `~/.openra/Content/d2k/v3` (Linux d2k content)

You can make it not save to %appdata% by creating a `Support` folder in d2-master\engine, and opening `launch-game.cmd`

When running from the SDK checkout, `launch-game.cmd` uses the local engine support directory.
Paste those files into:

 - `engine\Support\Content\d2` (Windows development checkout)

The current `d2 mod` still depends on a small subset of the current OpenRA `d2k` content.
This content is expected in the OpenRA `v3` layout:

 - `engine\Support\Content\d2k\v3` (Windows development checkout)

On first run, `d2 mod` will open the content installer if the `d2k` files are missing.
Use `Quick Install` to download the required `d2k` content and automatically return to `d2 mod`.
If you use `Advanced Install`, the installer may stay on the package list after downloading; press `Back`, then `Continue`, or close the game and run `launch-game.cmd` again.

The key scripts from [OpenRAModSDK](https://github.com/OpenRA/OpenRAModSDK) are:

| Windows               | Linux / macOS            | Purpose
| --------------------- | ------------------------ | ------------- |
| make.cmd              | Makefile                 | Compiles `d2 mod` and fetches dependencies (including the OpenRA engine).
| launch-game.cmd       | launch-game.sh           | Launches `d2 mod` from the SDK directory.
| launch-server.cmd     | launch-server.sh         | Launches a dedicated server for `d2 mod` from the `d2 mod` directory.
| utility.cmd           | utility.sh               | Launches the OpenRA Utility for `d2 mod`.
| &lt;not available&gt; | packaging/package-all.sh | Generates release installers for `d2 mod`.

To launch `d2 mod` from the development environment you must first compile it by running `make.cmd` (Windows),
or opening a terminal in the SDK directory and running `make` (Linux / macOS).

Now you can run `d2 mod` using `launch-game.cmd` (Windows) or `launch-game.sh` (Linux / macOS).


## License
`d2 mod` is free software. It is made available to you under the terms of the GNU General Public License
as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
For more information, see [COPYING](https://github.com/OpenRA/d2/blob/bleed/COPYING).


## Authors
d2 mod for OpenRA [AUTHORS](https://github.com/OpenRA/d2/blob/master/mods/d2/AUTHORS)

Special thanks to [OpenRA AUTHORS](https://github.com/OpenRA/OpenRA/blob/bleed/AUTHORS)


-------------------------------------------------------------------------------------------------------------------------

More information can be found here: [wiki](https://github.com/OpenRA/d2/wiki)
