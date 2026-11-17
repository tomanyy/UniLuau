using MoonSharp.Interpreter;
using NUnit.Framework;
using UnityEngine;

public class SchedulerLifecycleTests
{
    [Test]
    public void OwnedCoroutineRecordsResumeMetrics()
    {
        var script = new Script(CoreModules.Preset_SoftSandbox);
        var scheduler = new Scheduler(script);
        script.DoString("calls = 0; function update() calls = calls + 1 end");
        var owner = new object();

        scheduler.SpawnOwned(owner, script.Globals.Get("update"));

        Assert.AreEqual(1, script.Globals.Get("calls").Number);
        var metrics = scheduler.GetOwnerMetrics(owner);
        Assert.AreEqual(1, metrics.Resumes);
        Assert.GreaterOrEqual(metrics.LastResumeMilliseconds, 0d);
        Assert.AreEqual(0, metrics.ActiveTasks);
    }

    [Test]
    public void KillingOwnerDisconnectsSignalsAndCancelsScheduledWork()
    {
        var script = new Script(CoreModules.Preset_SoftSandbox);
        var scheduler = new Scheduler(script);
        var signal = new Signal(scheduler);
        script.DoString("calls = 0; function update() calls = calls + 1 end");
        var owner = new object();
        var cleaned = false;
        scheduler.CurrentOwner = owner;
        signal.Connect(script.Globals.Get("update"));
        var delayed = scheduler.Delay(0, script.Globals.Get("update"));
        scheduler.TrackCleanup(() => cleaned = true);
        scheduler.CurrentOwner = null;

        scheduler.KillOwner(owner);
        signal.Fire();
        scheduler.Tick(1f);

        Assert.AreEqual(0, script.Globals.Get("calls").Number);
        Assert.IsTrue(delayed.IsCancelled);
        Assert.IsTrue(cleaned);
        Assert.IsNull(scheduler.GetOwnerMetrics(owner));
    }

    [Test]
    public void KillingOwnerRemovesSignalWaiters()
    {
        var script = new Script(CoreModules.Preset_SoftSandbox);
        var scheduler = new Scheduler(script);
        var signal = new Signal(scheduler);
        script.Globals["signal"] = DynValue.FromObject(script, signal);
        script.DoString("calls = 0; function waitForSignal() calls = calls + 1; signal:Wait(); calls = calls + 1 end");
        var owner = new object();
        var handle = scheduler.SpawnOwned(owner, script.Globals.Get("waitForSignal"));
        Assert.AreEqual(1, scheduler.GetOwnerMetrics(owner).ActiveTasks);

        scheduler.KillOwner(owner);
        signal.Fire();

        Assert.AreEqual(1, script.Globals.Get("calls").Number);
        Assert.IsTrue(handle.IsCancelled);
        Assert.IsNull(scheduler.GetOwnerMetrics(owner));
    }

    [Test]
    public void PauseAndStepResumeOnlyOneCoroutineSlice()
    {
        var script = new Script(CoreModules.Preset_SoftSandbox);
        var scheduler = new Scheduler(script);
        script.DoString("calls = 0; function update() calls = calls + 1; coroutine.yield(0); calls = calls + 1 end");
        var owner = new object();
        scheduler.PauseOwner(owner);
        scheduler.SpawnOwned(owner, script.Globals.Get("update"));
        Assert.AreEqual(0, script.Globals.Get("calls").Number);

        scheduler.StepOwner(owner);
        scheduler.Tick(0.1f);
        Assert.AreEqual(1, script.Globals.Get("calls").Number);
        scheduler.Tick(0.1f);
        Assert.AreEqual(1, script.Globals.Get("calls").Number);
    }
}
