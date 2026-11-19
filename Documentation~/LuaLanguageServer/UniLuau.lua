local function newSignal()
    return {
        Connect = function(self, callback) return { Connected = true, Disconnect = function() end } end,
        Once = function(self, callback) return { Connected = true, Disconnect = function() end } end,
        Wait = function(self) end
    }
end

Vector2 = {
    new = function(x, y) return { X = x, Y = y, Magnitude = 0, Unit = {} } end
}
Vector2.zero = Vector2.new(0, 0)
Vector2.one = Vector2.new(1, 1)

Vector3 = {
    new = function(x, y, z) return { X = x, Y = y, Z = z, Magnitude = 0, Unit = {}, Forward = {}, Right = {}, Up = {} } end
}
Vector3.zero = Vector3.new(0, 0, 0)
Vector3.one = Vector3.new(1, 1, 1)
Vector3.up = Vector3.new(0, 1, 0)
Vector3.forward = Vector3.new(0, 0, 1)

Color3 = {
    new = function(r, g, b) return { R = r, G = g, B = b } end,
    fromRGB = function(r, g, b) return { R = r / 255, G = g / 255, B = b / 255 } end
}

CFrame = {
    new = function(x, y, z) return { Position = Vector3.new(x, y, z), LookVector = Vector3.new(0, 0, 1) } end,
    lookAt = function(from, to) return {} end
}
CFrame.identity = CFrame.new(0, 0, 0)

UDim = { new = function(scale, offset) return { Scale = scale, Offset = offset } end }
UDim2 = { new = function(xs, xo, ys, yo) return { X = UDim.new(xs, xo), Y = UDim.new(ys, yo) } end }
UDim2.fromOffset = function(x, y) return UDim2.new(0, x, 0, y) end
Rect = { new = function(x, y, width, height) return { X = x, Y = y, Width = width, Height = height } end }
NumberRange = { new = function(minimum, maximum) return { Min = minimum, Max = maximum } end }
NumberSequenceKeypoint = { new = function(time, value, envelope) return { Time = time, Value = value, Envelope = envelope or 0 } end }
NumberSequence = { new = function(value) return { Keypoints = {} } end }
ColorSequenceKeypoint = { new = function(time, value) return { Time = time, Value = value } end }
ColorSequence = { new = function(value) return { Keypoints = {} } end }
TweenInfo = { new = function(duration, style, direction, repeats, reverses, delay) return {} end }

RaycastParams = {
    new = function() return { FilterType = "Exclude", FilterDescendantsInstances = {} } end
}

Enum = {
    KeyCode = { Space = "Space", W = "W", A = "A", S = "S", D = "D" },
    UserInputType = { MouseButton1 = "MouseButton1", MouseButton2 = "MouseButton2", Touch = "Touch" }
}

local userInputService = {
    InputBegan = newSignal(),
    InputEnded = newSignal(),
    IsKeyDown = function(self, key) return false end,
    IsMouseButtonPressed = function(self, button) return false end
}

local runService = {
    Heartbeat = newSignal(),
    Stepped = newSignal(),
    RenderStepped = newSignal(),
    FixedUpdate = newSignal(),
    LateUpdate = newSignal()
}

game = {
    GetService = function(self, serviceName)
        if serviceName == "UserInputService" then return userInputService end
        if serviceName == "RunService" then return runService end
        return {}
    end
}

workspace = {}
script = {
    Parent = {},
    Name = "LuaBehaviour",
    ClassName = "LuaScriptRef",
    GetAttribute = function(self, name) end,
    GetAttributeChangedSignal = function(self, name) return newSignal() end
}

Unity = {
    Current = {},
    Scene = {},
    Update = newSignal(),
    FixedUpdate = newSignal(),
    LateUpdate = newSignal(),
    Find = function(name) end,
    FindInScene = function(name, sceneName) end,
    Create = function(typeName, name) end,
    Instantiate = function(object) end,
    Log = function(message) end,
    Warn = function(message) end
}

unity = {
    PlayAudio = function(resourcePath, loop, volume) end,
    StopAudio = function() end,
    SetAnimationTrigger = function(name) end,
    SetAnimationFloat = function(name, value) end,
    LoadScene = function(name) end,
    LoadSceneAsync = function(name) end
}

task = {
    wait = function(seconds) end,
    spawn = function(callback, ...) return { Cancel = function() end, IsCancelled = false } end,
    defer = function(callback, ...) return { Cancel = function() end, IsCancelled = false } end,
    delay = function(seconds, callback, ...) return { Cancel = function() end, IsCancelled = false } end,
    cancel = function(handle) end
}

Instance = { new = function(className, parent) return {} end }
GameObject = { new = function(className, parent) return {} end }
