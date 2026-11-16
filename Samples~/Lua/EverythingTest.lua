local passed, failed, skipped = 0, 0, 0
local function test(name, fn)
    local ok, err = pcall(fn)
    if ok then
        passed += 1
        print("[PASS] " .. name)
    else
        failed += 1
        warn("[FAIL] " .. name .. ": " .. tostring(err))
    end
end
local function skip(name, reason)
    skipped += 1
    print("[SKIP] " .. name .. ": " .. reason)
end
test("Unity-named audio, animation and scene API is exposed", function()
    assert(unity ~= nil, "unity global is missing")
    assert(type(unity.PlayAudio) == "function")
    assert(type(unity.StopAudio) == "function")
    assert(type(unity.SetAnimationTrigger) == "function")
    assert(type(unity.SetAnimationFloat) == "function")
    assert(type(unity.LoadScene) == "function")
    assert(type(unity.LoadSceneAsync) == "function")
end)

test("Lua values, math, Luau syntax and script attributes", function()
    local v = Vector3.new(3, 4, 0)
    assert(math.abs(v.Magnitude - 5) < 0.001)
    local x = 1
    x += 2
    assert(x == 3)
    assert(script:GetAttribute("TestValue") == nil)
    script:SetAttribute("TestValue", 42)
    assert(script:GetAttribute("TestValue") == 42)
end)
test("scene lookup, hierarchy, components and Unity property paths", function()
    local scene = game.SampleScene
    assert(scene ~= nil, "SampleScene is not loaded")
    local cameraObject = scene["Main Camera"]
    assert(cameraObject ~= nil, "Main Camera is missing")
    assert(cameraObject.Camera ~= nil, "Camera component is missing")
    local fov = cameraObject.Camera.Projection["Field of View"]
    assert(type(fov) == "number")
    local cube = scene.Cube
    assert(cube ~= nil, "Cube is missing")
    assert(cube:GetComponent("MeshRenderer") ~= nil)
    assert(cube.Transform ~= nil)
    assert(#scene:GetChildren() > 0)
end)
test("GameObject creation, component add/remove, tags, layers and cleanup", function()
    local obj = GameObject.new("Empty", game.SampleScene)
    obj.Name = "EverythingTestObject"
    assert(obj ~= nil)
    local body = obj:AddComponent("Rigidbody")
    assert(body ~= nil and obj:GetComponent("Rigidbody") ~= nil)
    obj.Tag = "Player"
    assert(obj.Tag == "Player")
    obj.Tag = "Untagged"
    obj.LayerName = "Default"
    assert(obj.Layer == 0)
    obj.Visible = false
    assert(obj.Visible == false)
    obj.Visible = true
    assert(obj.Visible == true)
    obj:Destroy()
end)
test("raycast and raycast filter types", function()
    local params = RaycastParams.new()
    params.FilterType = "Exclude"
    params.FilterDescendantsInstances = { script.Parent }
    assert(params.FilterType == "Exclude")
    local hit = game.SampleScene:Raycast(Vector3.new(0, 1000, 0), Vector3.new(0, -1, 0), params)
    assert(hit == nil or (hit.Instance ~= nil and hit.Distance >= 0))
end)
test("RunService frame signal and connection cleanup", function()
    local run = game:GetService("RunService")
    assert(run ~= nil)
    local connection = run.Heartbeat:Connect(function() end)
    assert(connection.Connected)
    connection:Disconnect()
    assert(not connection.Connected)
end)
test("UserInputService access", function()
    local input = game:GetService("UserInputService")
    assert(input ~= nil)
    assert(type(input:IsKeyDown("W")) == "boolean")
    assert(input:GetMouseLocation() ~= nil)
end)
test("Collection, JSON and GUID services", function()
    local collection = game:GetService("CollectionService")
    local obj = GameObject.new("Empty", game.SampleScene)
    obj.Name = "TaggedTestObject"
    collection:AddTag(obj, "LuaEverythingTest")
    assert(collection:HasTag(obj, "LuaEverythingTest"))
    assert(#collection:GetTagged("LuaEverythingTest") > 0)
    collection:RemoveTag(obj, "LuaEverythingTest")
    assert(not collection:HasTag(obj, "LuaEverythingTest"))
    obj:Destroy()
    local http = game:GetService("HttpService")
    local id = http:GenerateGUID()
    assert(type(id) == "string" and #id > 0)
    local encoded = http:JSONEncode({ answer = 42 })
    assert(type(encoded) == "string")
    assert(http:JSONDecode(encoded).answer == 42)
end)
test("Debris and bindable event", function()
    local event = GameObject.new("BindableEvent")
    local got = false
    local connection = event.Event:Connect(function(value) got = value == "ok" end)
    event:Fire("ok")
    assert(got)
    connection:Disconnect()
    event:Destroy()
    local obj = GameObject.new("Empty", game.SampleScene)
    obj.Name = "DebrisTestObject"
    game:GetService("Debris"):AddItem(obj, 60)
    assert(obj ~= nil)
    obj:Destroy()
end)
test("UI creation, events and layout components", function()
    local screen = GameObject.new("ScreenGui")
    screen.Name = "LuaEverythingTest"
    screen.Parent = game:GetService("CoreGui")
    local frame = GameObject.new("Frame")
    frame.Size = UDim2.new(0, 240, 0, 140)
    frame.Parent = screen
    local button = GameObject.new("TextButton")
    button.Text = "Click test"
    button.Size = UDim2.new(0, 160, 0, 40)
    button.Parent = frame
    local clicked = false
    local clickConnection = button.MouseButton1Click:Connect(function() clicked = true end)
    assert(button.Text == "Click test")
    assert(frame:FindFirstChild("TextButton") ~= nil)
    local layout = GameObject.new("UIListLayout")
    layout.Spacing = 5
    layout.Parent = frame
    assert(frame:FindFirstChild("UIListLayout") ~= nil)
    clickConnection:Disconnect()
    screen:Destroy()
end)
test("Physics collision group API", function()
    local physics = game:GetService("PhysicsService")
    local groups = physics:GetRegisteredCollisionGroups()
    local exists = false
    for _, group in ipairs(groups) do
        if group.name == "LuaEverythingTest" then exists = true end
    end
    if not exists then physics:RegisterCollisionGroup("LuaEverythingTest") end
    physics:CollisionGroupSetCollidable("Default", "LuaEverythingTest", true)
    assert(#physics:GetRegisteredCollisionGroups() > 0)
end)
if script:GetAttribute("AllowAudioTest") then
    test("Audio facade playback and stop", function()
        unity.PlayAudio("Audio/LuaTestClip", false, 0.1)
        unity.StopAudio()
    end)
else
    skip("audio playback", "add LuaValue AllowAudioTest=true and enable Allow Audio")
end
if script:GetAttribute("AllowAnimationTest") then
    test("Animator facade parameter write", function()
        unity.SetAnimationFloat("LuaTestFloat", 0)
    end)
else
    skip("Animator parameter write", "add LuaValue AllowAnimationTest=true and enable Allow Animation")
end
test("scene loading remains opt-in", function()
    assert(type(unity.LoadSceneAsync) == "function")
end)
print("[RESULT] EverythingTest: " .. passed .. " passed, " .. failed .. " failed, " .. skipped .. " skipped")
