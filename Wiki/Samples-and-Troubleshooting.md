# Samples and Troubleshooting

## Import samples

After installing UniLuau, open its entry in Package Manager and expand **Samples**. Import **Lua examples and smoke tests** to copy scripts into your project. `EverythingTest.lua` is a broad Play Mode smoke test; it expects `SampleScene` to contain `Main Camera`, `Cube`, and `Capsule`. It skips audio and Animator operations unless you opt in and provide the required assets/components. Scene loading is deliberately not performed by the test.

Other examples include a first-person controller and separate audio, animation, and scene-loading scripts. Check each script's setup notes before running it.

## Troubleshooting

**The behaviour says “Missing script.”** Assign a `.lua` or `.luau` TextAsset in the Inspector.

**A component or property cannot be found.** Confirm the GameObject has that Unity component and use the component's actual Unity type name. Inspector labels such as `Field of View` may need bracket notation: `camera.Projection["Field of View"]`.

**A module cannot be found.** Put it under `Assets/Resources/Lua`, check path capitalization, and omit the `.lua` extension in `require`.

**Audio cannot find a clip.** Put it under `Assets/Resources` and pass its path relative to `Resources`, without an extension. Ensure **Allow Audio** is enabled.

**Scene loading fails.** Ensure the scene is included in Build Settings / Build Profiles and **Allow Scene Loading** is enabled.

**The editor reports a GUID conflict after installing the package.** The package and project contain duplicate copies of the same assets. Remove the old copies from `Assets/Editor` and `Assets/Plugins/MoonSharp`, plus old UniLuau scripts under `Assets/Scripts`, then let Unity reimport. Keep a backup or commit before deleting project assets.
