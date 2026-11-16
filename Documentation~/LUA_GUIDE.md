# Lua scripts in Unity

Attach a `.lua` or `.luau` asset by dropping it on a GameObject, or select a script and use **Assets > Add Lua Script to New GameObject**. The Lua Behaviour Inspector also accepts multiple scripts and shows syntax and missing-module checks. During Play mode, **Hot Reload** restarts the script after its asset changes.

## Unity API facade

Each Lua Behaviour receives a `unity` global scoped to its GameObject:

| Lua call | Inspector capability | Unity behavior |
| --- | --- | --- |
| `unity.PlayAudio(resourcePath, loop, volume)` | Allow Audio | Loads an `AudioClip` from `Resources`, creates/reuses an `AudioSource` on the script's GameObject, and plays it. |
| `unity.StopAudio()` | Allow Audio | Stops that GameObject's `AudioSource`. |
| `unity.SetAnimationTrigger(name)` | Allow Animation | Sets a trigger on the GameObject's `Animator`. |
| `unity.SetAnimationFloat(name, value)` | Allow Animation | Sets a float parameter on the GameObject's `Animator`. |
| `unity.LoadScene(name)` / `unity.LoadSceneAsync(name)` | Allow Scene Loading | Loads a scene by name; add it to Build Settings / Build Profiles. |

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

## Capability scope

The Inspector switches govern calls through the `unity` facade. This project also has older general-purpose Lua bindings that can access Unity objects and components; these switches do not sandbox those bindings and should not be treated as a security boundary for untrusted scripts. Only run scripts you trust. The facade is the narrow, Unity-named API intended for new scripts.
