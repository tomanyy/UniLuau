local cameraObject = Unity.Find("Main Camera")
assert(cameraObject, "Add a camera named Main Camera to the active scene")

local marker = Unity.Create("Sphere", "LuaMarker")
local startPosition = marker.Transform.Position
local elapsed = 0

Unity.Update:Connect(function(deltaTime)
    elapsed += deltaTime
    marker.Transform.Position = startPosition + Vector3.new(0, math.sin(elapsed), 0)
end)

Unity.Log("Running Lua on " .. Unity.Current.Name .. " in " .. Unity.Scene.Name)
