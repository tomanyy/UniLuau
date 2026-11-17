# Writing Scripts

UniLuau runs Lua and preprocesses a practical subset of Luau syntax before execution. Supported additions include compound assignment such as `count += 1`, `continue`, type annotations, type aliases, casts, and backtick string interpolation. It is not a complete Luau implementation; unsupported syntax is documented in `Runtime/UniLuau/Luau.cs` in the source repository.

Each Lua Behaviour runs its script in a separate environment. Scripts can access shared built-ins and services, but their own top-level variables are not a global namespace for other scripts. Use `require` for reusable modules.

In the Unity Editor, **Hot Reload** is enabled by default. Saving a changed script while in Play mode restarts that behaviour and cleans up its owned tasks, signal waiters, connections, and tracked objects. Inspector values can be edited while playing and read through `script:GetAttribute("Name")`.

The Unity-style global is available alongside the existing APIs:

```lua
local player = Unity.Current
local scene = Unity.Scene
local target = Unity.Find("Target")

Unity.Update:Connect(function(deltaTime)
    if target then
        target.Transform.Position = target.Transform.Position + Vector3.new(0, deltaTime, 0)
    end
end)
```

`Unity.FixedUpdate` and `Unity.LateUpdate` follow Unity's corresponding frame callbacks. `Unity.FindInScene(name, sceneName)` searches a named loaded scene. `Unity.Create(typeName, name)` creates a GameObject or supported primitive in the behaviour's scene, and `Unity.Instantiate(gameObject)` clones a wrapped GameObject.

Disable the behaviour or leave Play mode to stop it. Connections and scheduled work owned by the behaviour are cleaned up when it stops. For pause, step, profiling, and cancellation details, see [Performance, Debugging, and Tests](Performance-and-Debugging).
