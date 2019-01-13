--[[
   Copyright 2007-2018 The OpenRA Developers (see AUTHORS)
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

ToHarvest =
{
	easy = 1000,
	normal = 1500,
	hard = 2000
}

Tick = function()
	if ordos.HasNoRequiredUnits() and not player.IsObjectiveCompleted(KillOrdos) then
		player.MarkCompletedObjective(KillOrdos)
	end

	if player.Resources > SpiceToHarvest - 1 and not player.IsObjectiveCompleted(GatherSpice) then
		player.MarkCompletedObjective(GatherSpice)
	end

	UserInterface.SetMissionText("Produced Credits: " .. player.Resources .. "/" .. SpiceToHarvest, player.Color)
end

WorldLoaded = function()
	player = Player.GetPlayer("Atreides")
	ordos = Player.GetPlayer("Ordos")

	SpiceToHarvest = ToHarvest[Difficulty]

	InitObjectives(player)
	KillAtreides = ordos.AddPrimaryObjective("Kill all Atreides units.")
	GatherSpice = player.AddPrimaryObjective("Harvest Spice and produce " .. tostring(SpiceToHarvest) .. " Credits.")
	KillOrdos = player.AddSecondaryObjective("Eliminate all Ordos units in the area.")

	Camera.Position = AConyard.CenterPosition

	local checkResourceCapacity = function()
		Trigger.AfterDelay(0, function()
			if player.ResourceCapacity < SpiceToHarvest then
				Media.DisplayMessage("We don't have enough silo space to store the required amount of Spice!", "Mentat")
				Trigger.AfterDelay(DateTime.Seconds(3), function()
					ordos.MarkCompletedObjective(KillAtreides)
				end)

				return true
			end
		end)
	end

	Trigger.OnRemovedFromWorld(AConyard, function()

		-- Mission already failed, no need to check the other conditions as well
		if checkResourceCapacity() then
			return
		end

		local refs = Utils.Where(Map.ActorsInWorld, function(actor) return actor.Type == "refinery" and actor.Owner == player end)
		if #refs == 0 then
			ordos.MarkCompletedObjective(KillAtreides)
		else
			Trigger.OnAllRemovedFromWorld(refs, function()
				ordos.MarkCompletedObjective(KillAtreides)
			end)

			Utils.Do(refs, function(actor) Trigger.OnRemovedFromWorld(actor, checkResourceCapacity) end)
		end
	end)
end
