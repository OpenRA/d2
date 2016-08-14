InfToTrain = {
  "light_inf",
  "trooper",
}

TanksToTrain = {
  "combat_tank_a"
}

UnitCosts = {
}

CanAfford = function(unit_type)
  return Atreides.Resources >= UnitCosts[unit_type]
end

SetupInvincibiliy = function()
  Utils.Do(Map.NamedActors, function(actor)
    if actor.HasProperty("AcceptsUpgrade") and actor.AcceptsUpgrade("invincible") then
      actor.GrantUpgrade("invincible")
    end
  end)
end

HarkonnenSpawnUnit = function(unit_type, locations)
  local loc = rand_t(locations)
  local unit = Actor.Create(unit_type, true, {
    Owner = Harkonnen,
    Location = loc
  })

  unit.AttackMove(AtreidesMPSpawn.Location)
end

SendHarkonnenWave = function()
  local units = concat_t(InfToTrain, { "combat_tank_h" })
  Utils.Do(units, function(unit)
    HarkonnenSpawnUnit(unit, HarkonnenSpawns)
  end)

  Trigger.AfterDelay(DateTime.Seconds(20), SendHarkonnenWave)
end

AtreidesSpawnUnit = function(unit_type, locations)
  local loc = rand_t(locations)
  local unit = Actor.Create(unit_type, true, {
    Owner = Atreides,
    Location = loc
  })

  unit.AttackMove(rand_t(BaseEntrances))
end

AtreidesTrainInf = function()
  local inf = rand_t(InfToTrain)

  if not CanAfford(inf) then
    Trigger.AfterDelay(DateTime.Seconds(35), AtreidesTrainInf)
    return
  end

  AtreidesPayFor(inf)
  AtreidesSpawnUnit(inf, InfSpawns)
  Trigger.AfterDelay(DateTime.Seconds(20), AtreidesTrainInf)
end

AtreidesTrainTank = function()
  local tank = rand_t(TanksToTrain)

  if not CanAfford(tank) then
    Trigger.AfterDelay(DateTime.Seconds(45), AtreidesTrainTank)
    return
  end

  AtreidesPayFor(tank)
  AtreidesSpawnUnit(tank, TankSpawns)
  Trigger.AfterDelay(DateTime.Seconds(30), AtreidesTrainTank)
end

AtreidesPayFor = function(unit_type)
  Atreides.Resources = Atreides.Resources - UnitCosts[unit_type]
end

WorldLoaded = function()
  Atreides = Player.GetPlayer("Atreides")
  Harkonnen = Player.GetPlayer("Harkonnen")

  Trigger.AfterDelay(1, SetupInvincibiliy)

  TankSpawns = {
    TankSpawn1.Location,
    TankSpawn2.Location
  }

  InfSpawns = {
    InfSpawn1.Location,
    InfSpawn2.Location
  }

  BaseEntrances = {
    BaseEntranceNorth.Location,
    BaseEntranceEast.Location,
    BaseEntranceSouth.Location,
    BaseEntranceWest.Location,
  }

  HarkonnenSpawns = {
    HarkSpawnNorth.Location,
    HarkSpawnSouthEast.Location,
    HarkSpawnSouthWest.Location
  }

  UnitCosts = {
    ["light_inf"] = Actor.Cost("light_inf"),
    ["trooper"] = Actor.Cost("trooper"),
    ["combat_tank_a"] = Actor.Cost("combat_tank_a")
  }

  Trigger.AfterDelay(1, function()
    Atreides.Resources = 5000
  end)

  Trigger.AfterDelay(DateTime.Seconds(30), AtreidesTrainInf)
  Trigger.AfterDelay(DateTime.Seconds(45), AtreidesTrainTank)
  Trigger.AfterDelay(DateTime.Seconds(20), SendHarkonnenWave)
end
