# Performance, Debugging, and Tests

## Inspector profiler and debugger controls

In Play mode, select a GameObject with a Lua Behaviour. Its Inspector shows runtime status and these per-script scheduler metrics:

- **Coroutine resumes:** number of Lua coroutine resume slices that have run.
- **Active tasks / connections:** scheduled or signal-waiting coroutines, and connected signal handlers.
- **Pending delays / errors:** scheduled `task.delay` or `task.defer` calls, and runtime errors since this run began.
- **Resume time (last / max):** wall-clock duration of the latest resume slice and the longest slice observed. This helps identify slow callbacks; it is not a full Unity CPU profiler.
- **Last error:** formatted Lua error with source context and stack frames where available.

The controls operate on that behaviour's scheduler owner:

- **Pause** prevents the next coroutine resume from starting.
- **Step** permits one coroutine resume slice, then pauses again.
- **Resume** allows scheduled work to continue.
- **Restart** cleans up the current run and starts the script again.

Pause and step work at coroutine yield/resume boundaries. They cannot interrupt a Lua callback that is already executing. They also do not stop Unity's own Update loop or other Lua Behaviours.

## Tasks, signals, and cleanup

`task.spawn`, `task.defer`, and `task.delay` return task handles. Cancel them from Lua with `task.cancel(handle)` or `handle:Cancel()`; inspect `handle.IsCancelled` if needed. For example:

```lua
local handle = task.delay(2, function()
    print("Two seconds elapsed")
end)

-- Cancel before the delay expires:
task.cancel(handle)
```

Signal connections have `Disconnect()` and `Connected`. When a behaviour is disabled, destroyed, restarted, or hot reloaded, UniLuau cancels its owned tasks, removes its signal waiters, disconnects its signal handlers, and runs its tracked object cleanup. This prevents work from an old run from resuming after teardown.

## Automated tests and CI

The EditMode scheduler tests are in `Tests/Editor/SchedulerLifecycleTests.cs`. They cover resume metrics, owner teardown, connection and delay cancellation, signal waiter cleanup, and pause/step behavior. Run them from the repository root with Unity 6 installed:

```text
unity test Tests/UnityProject --mode EditMode
```

The host project in `Tests/UnityProject` references UniLuau as a local package. GitHub Actions runs the same suite using GameCI. Add a repository secret named `UNITY_LICENSE` for licensing; `UNITY_EMAIL` and `UNITY_PASSWORD` may be used for an account license. A local run requires the Editor version recorded in `Tests/UnityProject/ProjectSettings/ProjectVersion.txt`.
