#!/bin/bash

cd ../../..
make all
cp Eluant.dll mods/d2/OpenRA.Mods.D2/dependencies
cp OpenRA.Game.exe mods/d2/OpenRA.Mods.D2/dependencies
#cp mods/cnc/*.dll mods/d2/OpenRA.Mods.D2/dependencies
cp mods/common/*.dll mods/d2/OpenRA.Mods.D2/dependencies
#cp mods/ra/*.dll mods/d2/OpenRA.Mods.D2/dependencies
#cp mods/d2k/*.dll mods/d2/OpenRA.Mods.D2/dependencies
cd mods/d2/OpenRA.Mods.D2
make all

