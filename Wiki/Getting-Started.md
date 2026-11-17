# Getting Started

## Install the package

UniLuau requires Unity 6 and declares its Input System and Unity UI dependencies in `package.json`.

In Unity, open **Window > Package Management > Package Manager**, select **+ > Add package from git URL**, and enter:

```text
https://github.com/tomanyy/UniLuau.git
```

UniLuau declares its Input System and Unity UI dependencies in `package.json`. Let Package Manager resolve them before using the package.

## Add your first Lua script

1. Create a `.lua` or `.luau` file under your project's `Assets` folder.
2. Create or select a GameObject in the Hierarchy.
3. Add **Lua Behaviour** from **Add Component > Lua**.
4. Assign the script asset to the component's **Script** field.
5. Enter Play mode and check the Unity Console for output or errors.

You can also drag a Lua asset onto a GameObject in the Hierarchy. To create a GameObject and attach the script in one step, select the script asset and choose **Assets > Add Lua Script to New GameObject**.

## First script

```lua
print("Hello from UniLuau")

local object = script.Parent
print("This script is attached to " .. object.Name)
```

`script.Parent` refers to the GameObject that has the Lua Behaviour. `print` and `warn` write to the Unity Console.
