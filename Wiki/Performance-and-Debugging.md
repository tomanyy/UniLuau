# Performance and debugging

While the Editor is in Play mode, select a GameObject with a Lua Behaviour to see its runtime status and scheduler metrics in the Inspector. Resume time is measured around each coroutine resume, so a long callback is visible immediately. The panel also reports resume count, active scheduled tasks, signal connections, pending delays, and runtime errors.

Use **Pause** to stop that behaviour between coroutine resumes, **Step** to allow one resume slice, **Resume** to continue, and **Restart** to rerun its script. This is scheduler-level control: it takes effect at coroutine yield/resume boundaries and cannot interrupt a Lua callback that is currently executing. The Lua API's `task` handles expose `Cancel()` and `IsCancelled`; connections expose `Disconnect()` and `Connected`.

Disabling or destroying a Lua Behaviour cancels its owned tasks, disconnects its signal handlers, removes signal waiters, and destroys objects tracked for cleanup. Hot reload performs the same owner cleanup before starting the new script.

Run the EditMode suite from Unity Test Runner, or use the repository's GitHub Actions workflow. The workflow opens `Tests/UnityProject`, which references this package as a local package. Configure the repository's `UNITY_LICENSE` secret for GameCI licensing; `UNITY_EMAIL` and `UNITY_PASSWORD` can be supplied if using a Unity account license.
