# IL2CPP and MoonSharp

The `link.xml` file preserves the predefined assemblies that contain MoonSharp
and the Lua-facing API. This keeps MoonSharp's reflected user-data types
available when Unity builds an IL2CPP player with managed code stripping.

To build with IL2CPP:

1. In **Project Settings > Player > Other Settings**, select **IL2CPP** as the
   scripting backend for the target platform.
2. Keep `link.xml` under `Assets`. If you move MoonSharp or the Lua API into
   assembly definition files, update the assembly names in `link.xml` to match.
3. Build and run a player that loads Lua scripts, creates instances, and calls
   methods/properties on MoonSharp user data.

Both predefined assemblies are preserved in full because `UserData.RegisterAssembly`
discovers exposed types by reflection. This favors reliable reflection over the
smallest possible player; narrow the preservation rules if build size becomes
important and verify each exposed Lua type after changing them.
