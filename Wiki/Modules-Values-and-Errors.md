# Modules, Inspector Values, and Errors

## Modules

Place reusable Lua modules under `Assets/Resources/Lua`. Require them by path relative to that folder, without an extension:

```text
Assets/Resources/Lua/Movement/Helpers.lua
```

```lua
local helpers = require("Movement/Helpers")
helpers.move(script.Parent)
```

Modules are cached while running. Hot reload clears the module cache and reloads the behaviour.

## Inspector values

Use the Lua Behaviour **Attributes** list to expose editable defaults to the script. Add an entry with **+**, give it a name, select its type, and set its default value. Then read or watch it as an attribute:

```lua
local speed = script:GetAttribute("Speed")
script:GetAttributeChangedSignal("Speed"):Connect(function()
    speed = script:GetAttribute("Speed")
end)
```

## Errors and validation

Runtime errors are reported in the Unity Console with the script name and source location when available.
