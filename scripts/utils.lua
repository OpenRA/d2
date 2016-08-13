len_t = function(t)
    local c = 0
    for _ in pairs(t) do c = c + 1 end
    return c
end

copy_t = function(t)
    if type(t) ~= 'table' then
      return t
    end

    local ret = {}
    for k, v in pairs(t) do
        ret[copy_t(k)] = copy_t(v)
    end
  
    return ret
end

concat_t = function(t1, t2, ...)
    local args = {...}
    local ret = copy_t(t1)

    for _, v in pairs(t2) do
      ret[#ret + 1] = v
    end

    for _, t in pairs(args) do
        for _, v in pairs(t) do
            ret[#ret + 1] = v
        end
    end

    return ret
end

shuffle_t = function(t)
  local count = #t
  local ret = copy_t(t)
  for i = 0, count * 20 do
    local ndx0 = Utils.RandomInteger(0, count) + 1
    local ndx1 = Utils.RandomInteger(0, count) + 1
    ret[ndx0], ret[ndx1] = ret[ndx1], ret[ndx0]
  end

  return ret
end

nonil_t = function(t)
  local ret = {}
  for _, v in pairs(t) do
    ret[#ret + 1] = v
  end

  return ret
end

print_t = function(t)
    for k, v in pairs(t) do
        print(k, v)
    end
end

rand_t = function(t)
    if t == nil then
        error("rand_t: t must not be nil", 2)
    elseif len_t(t) == 0 then
        error("rand_t: t must not be empty", 2)
    end

    return Utils.Random(t)
end
