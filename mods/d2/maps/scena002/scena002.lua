--[[
   Copyright 2007-2018 The OpenRA Developers (see AUTHORS)
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

OrdosBase = { OConyard, OPower, OBarracks, OOutpost, ORefinery }

Tick = function()
	if ordos.HasNoRequiredUnits() and not player.IsObjectiveCompleted(KillOrdos) then
		player.MarkCompletedObjective(KillOrdos)
	end

	if player.HasNoRequiredUnits() and not ordos.IsObjectiveCompleted(KillAtreides) then
		ordos.MarkCompletedObjective(KillAtreides)
	end
end

WorldLoaded = function()
	player = Player.GetPlayer("Atreides")
	ordos = Player.GetPlayer("Ordos")

	InitObjectives(player)
	KillAtreides = ordos.AddPrimaryObjective("Destroy the Atreides.")
	KillOrdos = player.AddPrimaryObjective("Destroy the Ordos.")

	Camera.Position = AConyard.CenterPosition
	
	Trigger.AfterDelay(0, ActivateAI)
end
