# d2 mod for OpenRA

This repository contains a `d2 mod` for the [OpenRA](https://github.com/OpenRA/OpenRA) engine.
It is based on [OpenRAModSDK](https://github.com/OpenRA/OpenRAModSDK) and should be updated if `OpenRAModSDK` changed

These scripts and support files from `OpenRAModSDK` wrap and automatically manage a copy of the OpenRA game engine and common files,
and provide entrypoints to run development versions and to generate platform-specific installers.

Before run `d2 mod`, Download the Original Game and copy following files: 
 - `DUNE.PAK`
 - `VOC.PAK`
 - `ATRE.PAK`
 - `HARK.PAK`
 - `ORDOS.PAK`
 - `INTRO.PAK`
 - `FINALE.PAK`

Paste those files into `d2` directory. If this directory doesn't exist, create it:
 - `Documents/OpenRA/Content/d2` (Windows)
 - `~/Library/Application Support/OpenRA/Content/d2` (macOS)
 - `~/.openra/Content/d2` or `~/OpenRA/Content/d2` (Linux)

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

Currently `d2 mod` uses some data from `d2k mod`. You will need to launch OpenRA d2k mod once and download all nesessary data.
You can use `launch-d2k.cmd` (Windows) or `launch-d2k.sh` (Linux / macOS) to do this.
Launch, download data and exit. 

Now you can run `d2 mod` using `launch-game.cmd` (Windows) or `launch-game.sh` (Linux / macOS).


## License
`d2 mod` is free software. It is made available to you under the terms of the GNU General Public License
as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
For more information, see [COPYING](https://github.com/OpenRA/d2/blob/bleed/COPYING).


-------------------------------------------------------------------------------------------------------------------------

More information can be found here: [wiki](https://github.com/OpenRA/d2/wiki)
