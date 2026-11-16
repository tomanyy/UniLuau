# Unity Objects and Components

UniLuau wraps real Unity GameObjects, scenes, transforms, and components. Start with the `Unity` global to find or create GameObjects in the scene attached to the behaviour:

```lua
local scene = Unity.Scene
local cameraObject = Unity.Find("Main Camera")
local marker = Unity.Create("Empty", "SpawnMarker")
```

Components are accessed by their Unity type name; inspector properties use their Unity label.

```lua
local scene = game.SampleScene
local cameraObject = scene["Main Camera"]
local camera = cameraObject.Camera

local fov = camera.Projection["Field of View"]
camera.Projection["Field of View"] = 70

local cube = GameObject.new("Cube", scene)
local body = cube:AddComponent("Rigidbody")
body["Use Gravity"] = true
```

`GameObject.new` accepts a Unity component type, a supported primitive (`Cube`, `Sphere`, `Capsule`, `Cylinder`, `Plane`, or `Quad`), a UI type, or `Empty`. For example, create a named empty object with:

```lua
local marker = GameObject.new("Empty", scene)
marker.Name = "SpawnMarker"
```

Useful wrapper methods include `GetComponent`, `FindFirstChild`, `GetChildren`, `GetDescendants`, `Clone`, and `Destroy`. `scene:Raycast(origin, direction, params)` performs a 3D physics raycast. `RaycastParams.new()` supports `FilterType` (`"Include"` or `"Exclude"`) and `FilterDescendantsInstances`.

The bridge exposes Unity's real components; component properties that are not available or writable in the current Unity version will report errors. Use component wrappers rather than assuming every inspector property is a GameObject property.
