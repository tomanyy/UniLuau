# Audio, Animation, and Scenes

Each Lua Behaviour has a Unity-oriented `unity` API scoped to its GameObject.

## Audio

Put an `AudioClip` under your project's `Assets/Resources`, then call the path relative to `Resources` without a file extension. Enable **Allow Audio** on the Lua Behaviour.

```lua
unity.PlayAudio("Audio/ambience", true, 0.6)
unity.StopAudio()
```

The clip is played on an `AudioSource` on the same GameObject as the behaviour; one is added if needed.

## Animation

Add an `Animator` with the matching parameters to the same GameObject and enable **Allow Animation**.

```lua
unity.SetAnimationTrigger("OpenDoor")
unity.SetAnimationFloat("Speed", 1)
```

## Scene loading

Add the target scene to **Build Settings / Build Profiles**, enable **Allow Scene Loading**, then call:

```lua
unity.LoadSceneAsync("LevelTwo")
```

`unity.LoadScene(name)` is also available for synchronous loading. Scene loads can interrupt the current gameplay flow, so test them from a dedicated script.

The facade switches are not a security sandbox: older general-purpose bindings also expose Unity objects and components. Do not run untrusted scripts.
