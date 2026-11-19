# Lua scripts in Unity

Create a `.lua` or `.luau` asset in Unity's Project window and assign it to a Lua Behaviour. During Play mode, **Hot Reload** restarts the script after its asset changes.

## Unity API facade

Each Lua Behaviour receives a `unity` global scoped to its GameObject:

| Lua call | Unity behavior |
| --- | --- | --- |
| `unity.PlayAudio(resourcePath, loop, volume)` | Audio | Loads an `AudioClip` from `Resources`, creates/reuses an `AudioSource` on the script's GameObject, and plays it. |
| `unity.StopAudio()` | Audio | Stops that GameObject's `AudioSource`. |
| `unity.SetAnimationTrigger(name)` | Animation | Sets a trigger on the GameObject's `Animator`. |
| `unity.SetAnimationFloat(name, value)` | Animation | Sets a float parameter on the GameObject's `Animator`. |
| `unity.LoadScene(name)` / `unity.LoadSceneAsync(name)` | Scene Loading | Loads a scene by name; add it to Build Settings / Build Profiles. |

The audio example expects `Assets/Resources/Audio/ambience` (extension omitted in Lua). See `UnityAudioExample.lua`, `UnityAnimationExample.lua`, and `UnitySceneExample.lua` for starter scripts.

## Unity-style scripting API

Every Lua Behaviour also receives a capitalized `Unity` API alongside the existing bindings. `Unity.Current` is the attached GameObject, `Unity.Scene` is its loaded scene, and `Unity.Update`, `Unity.FixedUpdate`, and `Unity.LateUpdate` are Unity frame signals.

```lua
local camera = Unity.Find("Main Camera")
local marker = Unity.Create("Sphere", "LuaMarker")

Unity.Update:Connect(function(deltaTime)
    marker.Transform.Position = marker.Transform.Position + Vector3.new(0, deltaTime, 0)
end)
```

`Unity.Find(name)` searches the behaviour's scene. `Unity.FindInScene(name, sceneName)` searches a named loaded scene. `Unity.Instantiate(gameObject)` clones a wrapped GameObject and tracks it for script cleanup. `Unity.Log` and `Unity.Warn` write to the Unity Console with the owning GameObject as context. `Unity.Create` creates a Unity GameObject type or primitive and parents it to the behaviour's scene.

### C#-style Lua examples

Lua keeps its normal syntax while the Unity API exposes familiar GameObject, Transform, and frame callback concepts:

```lua
local marker = Unity.Create("Sphere", "LuaMarker")

Unity.Update:Connect(function(deltaTime)
    marker.Transform.Position = marker.Transform.Position + Vector3.new(0, deltaTime, 0)
end)
```

### Inspector attributes

Add attributes manually in the Lua Behaviour **Attributes** list with the **+** control. Set each name, type, and default value, then read it from Lua with `script:GetAttribute("Name")`. `script:GetAttributeChangedSignal("Name")` observes changes, and `script:SetAttribute("Name", value)` updates the runtime value. Supported values are Number, Integer, Boolean, String, Vector2, Vector3, Color, and GameObject.

```lua
local speed = script:GetAttribute("Speed")
script:GetAttributeChangedSignal("Speed"):Connect(function()
    speed = script:GetAttribute("Speed")
end)
```

## Optional networking sample

NGO support is an optional sample, separate from the core package. Install Netcode for GameObjects, then import **Netcode for GameObjects integration** from Package Manager. Add `LuaNetcodeAdapter` to a GameObject in the networked scene; it registers after NGO starts listening. Create a `RemoteEvent` in Lua and use `FireServer(...)` from clients, `FireClientTo(clientId, ...)` from the server, or `FireAllClients(...)` to broadcast. The server's `OnServerEvent` callback receives a peer object first, so `peer.ClientId` identifies the sender.

```lua
local remote = Instance.new("RemoteEvent")
remote.Name = "ChatMessage"

remote.OnServerEvent:Connect(function(peer, message)
    print("Client " .. tostring(peer.ClientId) .. ": " .. tostring(message))
    remote:FireClientTo(peer.ClientId, "Message received")
end)

remote.OnClientEvent:Connect(function(message)
    print("Server: " .. tostring(message))
end)
```

Keep server-side validation for every request that can change game state. UniLuau limits event payload size, nesting, argument count, and event rate, but those limits do not establish player identity or authorize gameplay actions. See the [network API reference](../Wiki/API-Reference.md#networking-adapter).

## Editor workflow

Create starter scripts from **Assets > Create > Scripting**. Unity's Inspector edits script values, and script errors appear in the Unity Console. The package imports `.lua` and `.luau` files as script assets.

For autocomplete and diagnostics in Visual Studio Code, see `Documentation~/LuaLanguageServer/README.md`. Lua Language Server understands Lua and the declared UniLuau API stubs; Luau syntax extensions may still produce diagnostics.
