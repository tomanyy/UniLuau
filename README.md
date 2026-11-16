# UniLuau
UniLuau is a Unity Package Manager package for running Lua and a practical subset of Luau in Unity. It includes a Lua Behaviour component, Unity object and component bindings, UI and input helpers, editor import/attach tools, and hot reload while playing.

Lua behaviours expose a Unity-oriented `Unity` global for scene and GameObject access, object creation and cloning, Unity frame callbacks, and context-aware logging. Existing script bindings remain available.

## Install from GitHub

In Unity, open **Window > Package Management > Package Manager**, select **+ > Add package from git URL**, and enter:

```text
https://github.com/tomanyy/UniLuau.git
```

The package requires Unity 6 and declares the Input System and Unity UI package dependencies. To install a tagged version, append `#v0.1.0` after creating that Git tag.

## Quick start

1. Create a `.lua` or `.luau` file in your project's `Assets` folder.
2. Add **Lua Behaviour** from **Add Component > Lua** to a GameObject.
3. Assign the Lua asset to the component and enter Play mode.

You can also drag a Lua asset onto a GameObject in the Hierarchy or select it and use **Assets > Add Lua Script to New GameObject**. The component supports Inspector values, runtime status/error reporting, and hot reload in the Editor.

Import the **Lua examples and smoke tests** sample from Package Manager for starter scripts. The broad smoke test expects a scene containing objects named `Main Camera`, `Cube`, and `Capsule`; see `Documentation~/LUA_GUIDE.md` for details and the Unity API facade.

## Developer guide

Browse the [wiki page drafts](Wiki/Home.md) for installation, scripting, Unity bindings, input, UI, modules, and troubleshooting.

## Package layout

- `Runtime/` contains UniLuau and the bundled MoonSharp interpreter.
- `Editor/` contains the `.lua` importer and Unity Editor conveniences.
- `Samples~/` contains importable Lua examples and smoke tests.
- `Documentation~/` contains setup and API notes.

MoonSharp is distributed under its 3-clause BSD license; see `Runtime/ThirdParty/MoonSharp/LICENSE`. The UniLuau code is distributed under the repository's GPL-3.0 license.
