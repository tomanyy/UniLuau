local function test(name, callback)
    local ok, err = pcall(callback)
    if ok then
        print("[PASS] " .. name)
    else
        warn("[FAIL] " .. name .. ": " .. tostring(err))
    end
end
local testScreen
test("uGUI scrolling frame and list layout", function()
    local screen = GameObject.new("ScreenGui")
    screen.Name = "LuaFeatureSmokeTest"
    screen.Parent = game:GetService("CoreGui")
    testScreen = screen
    local scroll = GameObject.new("ScrollingFrame")
    scroll.Name = "ListScroll"
    scroll.Position = UDim2.new(0, 20, 0, 20)
    scroll.Size = UDim2.new(0, 280, 0, 230)
    scroll.CanvasSize = UDim2.new(0, 0, 0, 560)
    scroll.ScrollingEnabled = true
    scroll.VerticalScrolling = true
    scroll.Parent = screen
    local list = GameObject.new("UIListLayout")
    list.FillDirection = "Vertical"
    list.Spacing = 8
    list.PaddingLeft = 8
    list.PaddingRight = 8
    list.PaddingTop = 8
    list.ChildControlWidth = true
    list.ChildControlHeight = true
    list.Parent = scroll
    for i = 1, 10 do
        local button = GameObject.new("TextButton")
        button.Name = "ListItem" .. i
        button.Text = "Scroll item " .. i
        button.Size = UDim2.new(1, -16, 0, 42)
        button.Parent = scroll
        local element = GameObject.new("LayoutElement")
        element.PreferredHeight = 42
        element.Parent = button
    end
    assert(scroll.ScrollingEnabled and scroll.VerticalScrolling)
    assert(scroll:FindFirstChild("UIListLayout") ~= nil)
end)
test("uGUI grid layout", function()
    local screen = testScreen
    assert(screen ~= nil, "scrolling UI test did not create its ScreenGui")
    local frame = GameObject.new("Frame")
    frame.Name = "GridPanel"
    frame.Position = UDim2.new(0, 320, 0, 20)
    frame.Size = UDim2.new(0, 300, 0, 230)
    frame.Parent = screen
    local grid = GameObject.new("UIGridLayout")
    grid.CellSize = Vector2.new(88, 64)
    grid.CellPadding = Vector2.new(8, 8)
    grid.Constraint = "FixedColumnCount"
    grid.ConstraintCount = 3
    grid.PaddingLeft = 8
    grid.PaddingTop = 8
    grid.Parent = frame
    for i = 1, 6 do
        local tile = GameObject.new("Frame")
        tile.Name = "GridItem" .. i
        tile.BackgroundColor3 = Color3.fromRGB(60 + i * 20, 120, 210)
        tile.Parent = frame
    end
    assert(frame:FindFirstChild("UIGridLayout") ~= nil)
    assert(grid.ConstraintCount == 3)
end)
test("Unity scene, component and inspector property paths", function()
    local scene = game.SampleScene
    assert(scene ~= nil, "SampleScene is not loaded")
    local cameraObject = scene["Main Camera"]
    assert(cameraObject ~= nil, "SampleScene needs a Main Camera")
    local camera = cameraObject.Camera
    assert(camera ~= nil, "Main Camera has no Camera component")
    local fov = camera.Projection["Field of View"]
    assert(type(fov) == "number", "Field of View was not resolved through the component path")
    camera.Projection["Field of View"] = fov
    assert(cameraObject.FieldOfView == nil, "use the Camera component before reading Field of View")
end)
test("GameObject.new with Unity components, material, visibility, tag and layer", function()
    local scene = game.SampleScene
    assert(scene ~= nil, "SampleScene is not loaded")
    local cube = GameObject.new("Cube", scene)
    assert(cube ~= nil, "Cube was not created")
    assert(cube:GetComponent("MeshRenderer") ~= nil, "Cube has no MeshRenderer")
    local rigidbody = cube:AddComponent("Rigidbody")
    assert(rigidbody ~= nil and rigidbody.ClassName == "Rigidbody", "AddComponent(Rigidbody) failed")
    assert(cube:GetComponent("Rigidbody") ~= nil, "GetComponent(Rigidbody) failed")
    assert(cube.Rigidbody ~= nil, "component path Rigidbody failed")
    cube.Material.Color = Color3.fromRGB(255, 0, 0)
    local color = cube.Material.Color
    assert(color.R > 0.9 and color.G < 0.1 and color.B < 0.1, "material color was not applied")
    cube.Visible = false
    assert(cube.Visible == false, "Visible=false did not hide the object")
    assert(cube.Parent == scene, "hiding a root object lost its scene parent")
    cube.Visible = true
    assert(cube.Visible == true, "Visible=true did not show the object")
    cube.Tag = "Player"
    assert(cube.Tag == "Player", "tag was not applied")
    cube.Tag = "Untagged"
    cube.LayerName = "Default"
    assert(cube.Layer == 0, "Default layer was not applied")
    cube:Destroy()
end)
test("GameObject.new('Text - TextMeshPro', parent)", function()
    local scene = game.SampleScene
    assert(scene ~= nil, "SampleScene is not loaded")
    local canvas = GameObject.new("Canvas", scene)
    assert(canvas ~= nil, "Canvas was not created")
    assert(canvas:GetComponent("Canvas") ~= nil, "Canvas component was not added")
    local text = GameObject.new("Text - TextMeshPro", canvas)
    assert(text ~= nil, "TextMeshPro object was not created")
    assert(text.TextMeshProUGUI ~= nil, "TextMeshProUGUI component path failed")
    local tmp = text:GetComponent("TextMeshProUGUI")
    assert(tmp ~= nil, "TextMeshProUGUI component was not found")
    tmp.Text = "Hello from GameObject.new"
    assert(tmp.Text == "Hello from GameObject.new", "TextMeshPro text was not set")
    canvas:Destroy()
end)
test("ContextActionService keyboard, gamepad and touch binding", function()
    local actions = game:GetService("ContextActionService")
    actions:BindAction("LuaSmokeAction", function(actionName, input, state)
        print("[ACTION] " .. actionName .. " " .. state .. " via " .. input.KeyCode .. "/" .. input.UserInputType)
    end, true, Enum.KeyCode.K, Enum.KeyCode.ButtonA)
    actions:SetTitle("LuaSmokeAction", "Test action")
    local info = actions:GetAllBoundActionInfo()
    assert(info.LuaSmokeAction ~= nil, "action was not registered")
end)
test("PhysicsService collision groups", function()
    local physics = game:GetService("PhysicsService")
    local groups = physics:GetRegisteredCollisionGroups()
    local foundA, foundB = false, false
    for _, group in ipairs(groups) do
        if group.name == "LuaSmokeA" then foundA = true end
        if group.name == "LuaSmokeB" then foundB = true end
    end
    if not foundA then physics:RegisterCollisionGroup("LuaSmokeA") end
    if not foundB then physics:RegisterCollisionGroup("LuaSmokeB") end
    physics:CollisionGroupSetCollidable("LuaSmokeA", "LuaSmokeB", false)
    local probe = game.SampleScene["Capsule"]
    assert(probe ~= nil, "SampleScene needs a Capsule to test collision groups")
    physics:SetPartCollisionGroup(probe, "LuaSmokeA")
end)
print("[INFO] For IL2CPP, build and run a player with this script to verify MoonSharp reflection in the player build.")
