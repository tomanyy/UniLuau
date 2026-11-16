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

Use the Lua Behaviour **Values** list to expose editable values to the script. Give each value a name, then read or watch it as an attribute:

```lua
local speed = script:GetAttribute("Speed")
script:GetAttributeChangedSignal("Speed"):Connect(function()
    speed = script:GetAttribute("Speed")
end)
```

## Errors and validation

The inspector checks script syntax and reports missing literal `require` paths. At runtime, errors appear in the Unity Console and on the Lua Behaviour inspector with the script name and source line where available. The **Status** field shows whether the behaviour is ready, running, disabled, or has an error.
