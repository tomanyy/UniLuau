# Input and UI

## Input

Get input services through the shared `game` object:

```lua
local input = game:GetService("UserInputService")
local run = game:GetService("RunService")

input.InputBegan:Connect(function(key, processed)
    if not processed and key.KeyCode == "Space" then
        print("Jump pressed")
    end
end)

run.Heartbeat:Connect(function(deltaTime)
    if input:IsKeyDown("W") then
        print("Moving forward")
    end
end)
```

The bridge also exposes mouse and gamepad state through `UserInputService`. Input events use the Input System package.

## UI

UI objects are created with `GameObject.new` and parented to a `ScreenGui`. `CoreGui` is a UniLuau UI root intended for runtime-created screens.

```lua
local screen = GameObject.new("ScreenGui")
screen.Name = "Hud"
screen.Parent = game:GetService("CoreGui")

local label = GameObject.new("TextLabel")
label.Text = "Ready"
label.Size = UDim2.new(0, 220, 0, 48)
label.Position = UDim2.new(0, 20, 0, 20)
label.Parent = screen
```

The package supports common uGUI objects, scrolling frames, list and grid layouts, and button events. Destroy the screen when it is no longer needed.
