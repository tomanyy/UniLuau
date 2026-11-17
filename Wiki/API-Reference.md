# UniLuau API Reference

This page describes the Lua-facing APIs included with UniLuau. It is based on the package runtime and is intended as a reference for the supported surface, not a promise of full Roblox compatibility. Unity components and the MoonSharp standard library are dynamic, so their complete members are documented by Unity and MoonSharp rather than repeated here.

- [Getting started](Getting-Started)
- [Writing scripts and Luau syntax](Writing-Scripts)
- [Unity objects and components](Unity-Objects-and-Components)
- [Input and UI](Input-and-UI)
- [Audio, animation, and scenes](Audio-Animation-and-Scenes)
- [Modules, inspector values, and errors](Modules-Values-and-Errors)
- [Performance, debugging, and tests](Performance-and-Debugging)

## Runtime globals

Every Lua Behaviour runs in its own environment. The environment inherits shared standard Lua globals, and receives the following UniLuau additions:

| Global | Purpose |
| --- | --- |
| `game` | Finds loaded scenes and returns the services listed below. |
| `script` | The current behaviour's script reference. Supports `Name`, `Parent`, attributes, and attribute-change signals. |
| `warn(message)` | Writes a warning to the Unity Console. `print` uses MoonSharp's debug-print callback and writes to the Console. |
| `task` | `wait`, `spawn`, `defer`, `delay`, and `cancel` for scheduler-owned coroutines. The global `wait(seconds)` is also available. |
| `require(path)` | Loads and caches a module from `Assets/Resources/Lua/<path>.lua`. Omit the extension. Circular requires raise an error. A module returning no value evaluates to `true`. |
| `typeof(value)` | Returns UniLuau names for wrapped Unity/Lua values, such as `Vector3`, `Color3`, `CFrame`, `GameObject`, `Scene`, `RBXScriptSignal`, and `RBXScriptConnection`; other values use Lua type names. |
| `Vector2`, `Vector3`, `Color3`, `CFrame`, `UDim2` | Constructors and constants described under [Value types](#value-types). |
| `Ray`, `RaycastParams`, `Random` | Ray, filtering, and random-number helpers. |
| `NumberRange`, `NumberSequence`, `NumberSequenceKeypoint`, `ColorSequence`, `ColorSequenceKeypoint` | Range and keypoint sequence helpers. |
| `GameObject.new(className, parent?)` | Creates supported Unity objects, UI objects, layouts, events, or prefabs. See [Creating objects](#creating-objects). |
| `Enum` | String-valued input and action enum tables. This is a small compatibility table, not Unity's or Roblox's full enum catalog. |
| `TweenInfo.new(...)`, `TweenService` | Tween configuration and service; `TweenService` is obtained from `game:GetService`. |

UniLuau extends the standard Lua `math` table with `clamp`, `sign`, `round`, `lerp`, and `noise`; extends `table` with `find`, `clone` (shallow), `clear`, `create`, and `pack`; and adds `string.split`. `os.time` and `os.date` are provided using the local system clock.

## Luau syntax preprocessing

Before execution, UniLuau preprocesses a practical subset of Luau syntax: compound assignment (`+=`, `-=`, `*=`, `/=`, `%=`, `^=`, `..=`), `continue`, type annotations and aliases, casts, and backtick string interpolation. This is a source transform over Lua, not a full Luau compiler. Unsupported Luau syntax may fail to parse; see [Writing Scripts](Writing-Scripts).

## `game`, scenes, and services

`game:GetService(name)` returns the following implementations:

| Service | Available behavior and limits |
| --- | --- |
| `RunService` | `Stepped`, `RenderStepped`, `Heartbeat`, `FixedUpdate`, `LateUpdate`; `Update` aliases `Heartbeat`. Signals pass delta time. |
| `UserInputService` | Keyboard, mouse, gamepad, and touch state/events. Requires Unity's Input System. See [Input and UI](Input-and-UI). |
| `Debris` | `AddItem(object, seconds?)` destroys a wrapped GameObject after a delay; default is 10 seconds. |
| `CollectionService` | `AddTag`, `RemoveTag`, `HasTag`, `GetTagged`, and `GetTags`. Tags are kept in memory by the service. |
| `HttpService` | `JSONEncode(table)`, `JSONDecode(json)`, and `GenerateGUID()`. It does not make HTTP requests. |
| `TweenService` | `GetValue(alpha, style, direction)` and `Create(instance, info, goals)`. See [Tweens](#tweens). |
| `ContextActionService` | Binds callbacks to key/input names, orders bindings by priority, and can create touch buttons. See [Input and UI](Input-and-UI). |
| `PhysicsService` | Registers collision groups using Unity layers 8–31, configures group collision, assigns a group to an object hierarchy, and lists registered groups. Reserve those layers for this service. |
| `Lighting` | Reads/writes ambient light, fog, directional-light brightness, and a simple clock time. These map to Unity render settings and a directional light. |
| `SoundService` | Controls `AudioListener.volume` and plays a `Resources` audio clip at the main camera position (or world origin). |
| `DataStoreService` | `GetDataStore(name)` returns a local JSON-file store. This is not Roblox cloud storage and its `*Async` method names do not mean network/asynchronous operations. |
| `ReplicatedStorage`, `ServerStorage` | Both expose the same local `Resources/Prefabs` lookup table. They do not provide server/client separation or replication. |
| `CoreGui`, `StarterGui` | Compatibility roots for UI `ScreenGui` objects. They are not Unity's actual internal CoreGui or a replication system. |

`game:FindScene(name)` gets a loaded scene wrapper. `game:GetScenes()` returns wrappers for loaded scenes. `Workspace` is intentionally unsupported; use `game:GetScenes()`, `game:FindScene(name)`, `Unity.Scene`, or a behaviour's `script.Parent` instead. Unknown service names raise a Lua error.

### Scene wrapper

A scene exposes `Name`, `CurrentCamera`, `Gravity`, `ChildAdded`, and `ChildRemoved`. It supports `FindFirstChild(name[, recursive])`, `FindFirstChildOfClass(className)`, `WaitForChild(name)`, `GetChildren()`, `GetDescendants()`, `Raycast(origin, direction[, params])`, `RaycastFromMouse(maxDistance[, params])`, `GetPartBoundsInRadius(position, radius)`, and `GetPartBoundsInBox(cframe, size)`. `WaitForChild` currently performs an immediate lookup; it does not wait for a future object.

## Unity-oriented `Unity` and `unity` APIs

Each behaviour gets both globals. `Unity` provides the scene/object and frame helpers without capability toggles; `unity` provides the narrower audio/animation/scene facade and obeys its Inspector toggles.

| Member | Behavior |
| --- | --- |
| `Unity.Current` | Wrapped GameObject with the Lua Behaviour. |
| `Unity.Scene` | The behaviour's loaded scene, falling back to the active scene. |
| `Unity.Update`, `Unity.FixedUpdate`, `Unity.LateUpdate` | Frame signals passing delta time. |
| `Unity.Find(name)` | Finds a GameObject in `Unity.Scene`. |
| `Unity.FindInScene(name, sceneName)` | Finds an object in a named loaded scene. |
| `Unity.Create(typeName[, name])` | Creates a Unity GameObject or primitive in the behaviour's scene and tracks it for cleanup. This helper does not create UI objects. |
| `Unity.Instantiate(gameObject)` | Clones a wrapped GameObject and tracks the clone for cleanup. |
| `Unity.Log(message)`, `Unity.Warn(message)` | Log to the Console with the behaviour GameObject as context. |

The lowercase `unity` facade provides `PlayAudio(resourcePath, loop?, volume?)`, `StopAudio()`, `SetAnimationTrigger(name)`, `SetAnimationFloat(name, value)`, `LoadScene(name)`, and `LoadSceneAsync(name)`. Audio, animation, and scene loading must be enabled on the Lua Behaviour Inspector. These toggles govern only this facade; they do not sandbox the broader object/component bindings.

## Unity objects and components

Wrapped GameObjects expose `Name`, `ClassName`, `Parent`, `Transform`, `Visible`/`Active`, `Tag`, `Layer`, `LayerName`, `Material`, and `Color`. They also provide:

- Component access: `GetComponent(className)` and `AddComponent(className)`. Component fields/properties are accessed by Unity type or Inspector label, including nested sections where available; available members depend on the Unity component and installed packages.
- Hierarchy: `FindFirstChild`, `FindFirstChildOfClass`, `FindFirstChildWhichIsA`, `FindFirstAncestor`, `FindFirstAncestorOfClass`, `FindFirstAncestorWhichIsA`, `GetFullName`, `GetChildren`, `GetDescendants`, `IsDescendantOf`, `IsA`, `WaitForChild`, `ClearAllChildren`, `Clone`, and `Destroy`.
- Events: `Touched`, `TouchEnded`, `ChildAdded`, `ChildRemoved`, `Destroying`, `Changed`, and `GetPropertyChangedSignal(property)`.
- Attributes: `SetAttribute(name, value)`, `GetAttribute(name)`, `GetAttributes()`, and `GetAttributeChangedSignal(name)`.
- `LookAt(target)` to orient its transform toward a `Vector3`.

`Transform` exposes `Position`, `LocalPosition`, `Scale`/`LocalScale`, `LossyScale`, `Rotation`, `LocalRotation`, `Forward`, `Right`, `Up`, `Parent`, `ChildCount`, `Find(name)`, and `GetChild(index)`. The rotation values use `Vector3` Euler angles in Unity degrees.

The bridge resolves many Unity `Component` types dynamically from loaded assemblies, with aliases for common UI, TextMeshPro, physics, audio, rendering, and animation types. The exact available component/property set depends on the project. Component properties that cannot be read or written by the bridge raise errors; this is not a generated binding for all Unity APIs.

## Creating objects

`GameObject.new(className, parent?)` accepts:

- Empty objects (`"GameObject"`, `"Empty"`) and primitives (`"Cube"`, `"Sphere"`, `"Capsule"`, `"Cylinder"`, `"Plane"`, `"Quad"`).
- A resolvable Unity component type; the factory adds it to a new GameObject. If it is a UI component, a `RectTransform` host is created.
- UI classes: `ScreenGui`, `Frame`, `ScrollingFrame`, `TextLabel`, `TextButton`, `ImageLabel`, `ImageButton`, and `TextBox`.
- Layout classes: `UIListLayout`, `UIGridLayout`, and `LayoutElement`.
- `BindableEvent`, `BindableFunction`, and `RemoteEvent`.
- A prefab under `Assets/Resources/Prefabs/<name>.prefab`.

`ReplicatedStorage` and `ServerStorage` find/list those prefabs with `FindFirstChild`, `WaitForChild`, `GetChildren`, and indexed lookup. `LuaPrefab:Clone()` returns a wrapped GameObject. Runtime-created prefabs and objects are cleaned up with the owning behaviour.

## Input, signals, and actions

Signals use `Connect(callback)`, `Once(callback)`, and `Wait()`. Connections expose `Connected` and `Disconnect()`. Signal callbacks run as scheduler-owned Lua tasks. When their owner stops, their connections are removed automatically.

`UserInputService` exposes `InputBegan`, `InputEnded`, `TouchStarted`, `TouchEnded`, and `TouchMoved`; `IsKeyDown(key)`, `IsMouseButtonPressed(button)`, `GetMouse()`, `GetMouseDelta()`, `GetMouseWheel()`, `GetMouseLocation()`, `MouseBehavior`, `GamepadConnected`, `GetGamepadThumbstick(name)`, `GetGamepadTrigger(name)`, and `TouchEnabled`. The mouse wrapper exposes `X`, `Y`, `Target`, and `Hit`. Input payloads provide `KeyCode` and `UserInputType`. Device availability and input bindings are subject to Unity Input System setup.

`ContextActionService` exposes `BindAction(name, callback, createTouchButton, ...inputs)`, `BindActionAtPriority(name, callback, createTouchButton, priority, ...inputs)`, `UnbindAction(name)`, `UnbindAllActions()`, `SetTitle(name, title)`, and `GetAllBoundActionInfo()`. Callbacks receive action name, input value, and `"Begin"`/`"End"`; bindings are owned by the script that created them and removed on teardown. It is a lightweight local adapter, not a full Roblox action service.

`task.spawn(callback, ...)`, `task.defer(callback, ...)`, and `task.delay(seconds, callback, ...)` return cancellable handles. `task.cancel(handle)` or `handle:Cancel()` cancels owned scheduled work; `handle.IsCancelled` reports the state. `task.wait(seconds)` and global `wait(seconds)` yield until a scheduler tick makes the delay due. Work does not preempt a callback that is already running.

## UI objects and layouts

Parent `ScreenGui` to `game:GetService("CoreGui")` or `StarterGui`, then parent child UI objects to it. Supported controls are `Frame`, `ScrollingFrame`, text/image labels and buttons, and text boxes. Common properties include `Name`, `Position`, `Size`, `AnchorPoint`, `Visible`, `ZIndex`, `BackgroundColor3`, `BackgroundTransparency`, `Text`, `TextColor3`, `TextSize`, `TextXAlignment`, `Image`, and `ImageColor3`; scrolling frames also expose `CanvasSize`, `CanvasPosition`, `ScrollingEnabled`, `HorizontalScrolling`, and `VerticalScrolling`.

UI events include `MouseButton1Click`, `MouseEnter`, `MouseLeave`, `FocusGained`, `FocusLost`, `TextChanged`, `Changed`, and `GetPropertyChangedSignal(property)`. UI objects support `Parent`, `FindFirstChild`, `GetChildren`, `GetDescendants`, `ClearAllChildren`, and `Destroy`.

`UIListLayout`, `UIGridLayout`, and `LayoutElement` expose layout properties including fill direction, child alignment, spacing, padding, child sizing/expansion, grid cell size/padding/corner/axis/constraints, and min/preferred/flexible dimensions. The custom UI layer implements a subset of uGUI behavior; it is not a complete Roblox GUI implementation. Images are loaded from resources by the package UI helper.

## Value types

| Type | Constructors and common members |
| --- | --- |
| `Vector2` | `new(x, y)`, `zero`, `one`; `X`, `Y`, `Magnitude`, `Unit`, `Dot`, `Lerp`, arithmetic operators. |
| `Vector3` | `new(x, y, z)`, `zero`, `one`, `xAxis`, `yAxis`, `zAxis`; `X`, `Y`, `Z`, `Magnitude`, `Unit`, `Dot`, `Cross`, `Lerp`, `Angle` (radians), arithmetic operators. |
| `Color3` | `new(r, g, b)`, `fromRGB(r, g, b)`, `fromHex(hex)`, `fromHSV(h, s, v)`; channels `R`, `G`, `B`, `ToHex`, `ToHSV`, and `Lerp`. RGB constructor channels are normalized 0–1; `fromRGB` uses 0–255. |
| `CFrame` | `new(...)`, `lookAt(from, to)`, `Angles`, `fromEulerAnglesXYZ`, `fromOrientation`, `fromAxisAngle`; position/basis vectors, `Inverse`, `Lerp`, `ToWorldSpace`, `ToObjectSpace`, point conversion, multiplication and translation operators. Angles are radians at constructors that accept Euler/axis angles; wrapped Unity transform rotation uses degrees. |
| `UDim2` | `new(xScale, xOffset, yScale, yOffset)`, `fromScale(x, y)`, `fromOffset(x, y)`; fields `XScale`, `XOffset`, `YScale`, `YOffset`, interpolation and arithmetic. |
| `Ray` | `new(origin, direction)`; fields `Origin`, normalized `Direction`. |
| `RaycastParams` | `new()`; `FilterType` (`"Include"`/`"Exclude"`) and `FilterDescendantsInstances`. |
| `Random` | `new(seed?)`; `NextNumber()`, `NextNumber(min, max)`, `NextInteger(min, max)`. |
| `NumberRange` | `new(min[, max])`; `Min`, `Max`. |
| `NumberSequence` | `new(value)`, `new(min, max)`, or `new(keypointTable)`; `Keypoints`. Keypoints use `NumberSequenceKeypoint.new(time, value, envelope)`. |
| `ColorSequence` | `new(color)`, `new(startColor, endColor)`, or `new(keypointTable)`; `Keypoints`. Keypoints use `ColorSequenceKeypoint.new(time, color)`. |

## Tweens

Create settings with `TweenInfo.new(time, easingStyle, easingDirection, repeatCount, reverses, delayTime)`. Supported easing styles are `Linear`, `Sine`, `Quad`, `Cubic`, `Back`, `Elastic`, and `Bounce`; directions are `In`, `Out`, and `InOut`.

```lua
local tweenService = game:GetService("TweenService")
local tween = tweenService:Create(part, TweenInfo.new(0.5, Enum.EasingStyle.Quad, Enum.EasingDirection.Out), {
    Position = Vector3.new(0, 3, 0)
})
tween.Completed:Connect(function(state)
    print("Tween ended", state)
end)
tween:Play()
```

Tween objects expose `PlaybackState`, `Completed`, and `Play()`, `Pause()`, `Cancel()`, `Reverse()`. The implementation interpolates numbers and values with a `Lerp` method, and applies goal values through wrapped object properties. It is a small runtime implementation; Unity's Animator/Animation systems and full Roblox tween semantics are not included.

## Local data, events, lighting, and materials

`DataStoreService:GetDataStore(name)` provides `GetAsync(key)`, `SetAsync(key, value)`, `UpdateAsync(key, callback)`, `IncrementAsync(key, delta)`, `RemoveAsync(key)`, and `ListKeys()`. Data is JSON-encoded in `Application.persistentDataPath/LuaData`; use simple Lua values/tables that the package JSON converter supports. Files are local to the app installation/device and writes happen synchronously.

`BindableEvent` has `Event`, `Fire(...)`, `Name`, `Parent`, and `Destroy()`. `BindableFunction` uses `OnInvoke`, `Invoke(...)`, `Name`, `Parent`, and `Destroy()`. `RemoteEvent` exposes `OnServerEvent`, `OnClientEvent`, `FireServer(...)`, `FireClient(...)`, `FireAllClients(...)`, `Name`, `Parent`, and `Destroy()`; all event directions are local signal dispatch in this single-process package, not networking.

`Lighting` maps `Ambient`, `FogEnabled`, `FogColor`, `FogStart`, `FogEnd`, `Brightness`, and `ClockTime` to Unity rendering settings and the selected directional light. `SoundService` maps `Volume` to Unity's global listener and loads sounds from `Resources` for `PlaySound(path[, volume])`.

Wrapped GameObjects expose `Material` and `Color` helpers. `Material` supports `Name`, `Shader`, `Color`/`MainColor`, `HasProperty`, `GetColor`, `SetColor`, `GetFloat`, and `SetFloat`. Exact material properties depend on the active shader; only recognized color slots are used by the convenience `Color` property.

## Lua Behaviour Inspector and editor tools

The Inspector supports script assignment, editable values (`Number`, `Boolean`, `String`, `Vector3`, `Color`, `Object`), audio/animation/scene-loading toggles, runtime status/error display, and editor hot reload. In Play mode its debugger offers line breakpoints, continue/step controls, locals, call stack, pause/resume/restart, scheduler metrics, and recent resume-time history. See [Performance, Debugging, and Tests](Performance-and-Debugging).

Editor utilities also include dragging Lua assets onto GameObjects, creating a GameObject from a selected script, a `.lua` importer, source preview, syntax and literal-module checks, and a Lua value property drawer. Lua Language Server stubs are provided separately in `Documentation~/LuaLanguageServer`.

## Lifecycle and cleanup

Scheduler jobs, delayed work, signal handlers, signal waiters, context actions, and tracked runtime-created GameObjects are tied to the owning Lua Behaviour. Disabling/destroying/restarting a behaviour or hot reloading its script cancels its owned work and runs tracked cleanup. `task.spawn` work may run immediately until it yields; a callback already executing cannot be interrupted by pause or teardown.

## Compatibility and security limits

- UniLuau runs MoonSharp Lua with a source preprocessor; it is not Luau VM or Roblox Studio.
- Roblox-shaped services are small Unity-backed adapters. Features described as local-only above are not replicated, online, or cloud-backed.
- The Inspector audio/animation/scene toggles restrict calls through the lowercase `unity` facade. The legacy `game`/`GameObject` component bridge can access Unity APIs more broadly, so these toggles are not a security boundary for untrusted scripts.
- Runtime APIs and dynamically discovered Unity component properties depend on Unity version and packages installed in the host project.
