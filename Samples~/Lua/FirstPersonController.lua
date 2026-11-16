local RunService = game:GetService("RunService")
local UserInputService = game:GetService("UserInputService")
local scene = game.SampleScene
local playerRoot = script.Parent
local cameraObject = scene.CurrentCamera
assert(cameraObject, "FirstPersonController needs a Unity camera tagged MainCamera")
local camera = cameraObject.Camera
local body = playerRoot.Rigidbody
local collider = playerRoot.Collider
local WALK_SPEED = 7
local JUMP_SPEED = 6.5
local LOOK_SENSITIVITY = 0.001
local CAMERA_OFFSET = Vector3.new(0, 0.65, 0)
local PITCH_LIMIT = math.rad(85)
local yaw = math.rad(playerRoot.Transform.Rotation.Y)
local pitch = 0
local jumpRequested = false
local cursorLocked = false
body["Is Kinematic"] = false
body["Freeze Rotation"] = true
if collider then collider["Enabled"] = true end
camera.Projection["Field of View"] = 80
local rayParams = RaycastParams.new()
rayParams.FilterType = "Exclude"
rayParams.FilterDescendantsInstances = { playerRoot }
local function setCursorLocked(locked)
    cursorLocked = locked
    UserInputService.MouseBehavior = locked and Enum.MouseBehavior.LockCenter or Enum.MouseBehavior.Default
end
UserInputService.InputBegan:Connect(function(input, gameProcessed)
    if input.KeyCode == "Escape" then
        setCursorLocked(false)
        return
    end
    if not gameProcessed and input.UserInputType == "MouseButton1" then
        setCursorLocked(true)
        return
    end
    if not gameProcessed and input.KeyCode == "Space" then
        jumpRequested = true
    end
end)
RunService.Heartbeat:Connect(function(deltaTime)
    if cursorLocked then
        local mouseDelta = UserInputService:GetMouseDelta()
        yaw = yaw + mouseDelta.X * LOOK_SENSITIVITY
        pitch = math.clamp(pitch - mouseDelta.Y * LOOK_SENSITIVITY, -PITCH_LIMIT, PITCH_LIMIT)
    end
    local forward = CFrame.Angles(0, yaw, 0).LookVector
    local right = CFrame.Angles(0, yaw, 0).RightVector
    local move = Vector3.zero
    if UserInputService:IsKeyDown("W") then move += forward end
    if UserInputService:IsKeyDown("S") then move -= forward end
    if UserInputService:IsKeyDown("D") then move += right end
    if UserInputService:IsKeyDown("A") then move -= right end
    if move.Magnitude > 1 then move = move.Unit end
    local velocity = body["Linear Velocity"]
    local verticalSpeed = velocity.Y
    local grounded = scene:Raycast(playerRoot.Transform.Position, Vector3.new(0, -1.15, 0), rayParams) ~= nil
    if jumpRequested and grounded then verticalSpeed = JUMP_SPEED end
    jumpRequested = false
    body["Linear Velocity"] = Vector3.new(move.X * WALK_SPEED, verticalSpeed, move.Z * WALK_SPEED)
    playerRoot.Transform.Rotation = Vector3.new(0, math.deg(yaw), 0)
    cameraObject.Transform.Position = playerRoot.Transform.Position + CAMERA_OFFSET
    cameraObject.Transform.Rotation = Vector3.new(math.deg(pitch), math.deg(yaw), 0)
end)
