# Lua Language Server setup

UniLuau does not bundle an editor extension. The Lua Language Server (LuaLS) can provide autocomplete and diagnostics for the standard Lua surface and the declared UniLuau globals.

1. Install the Lua Language Server extension in Visual Studio Code (`Lua`, publisher `LuaLS`).
2. Copy `UniLuau.lua` into your Unity project's `.vscode/UniLuau/` directory.
3. Copy `.luarc.json.example` to the project root as `.luarc.json`.
4. Reload the Lua Language Server or reopen the workspace.

The stub describes the common globals and Unity facade. It is an editor-only declaration file and should not be placed under `Assets` or loaded at runtime. The Lua Language Server understands Lua, not every Luau feature supported by UniLuau's preprocessor, so some Luau syntax may still need configuration or produce diagnostics.
