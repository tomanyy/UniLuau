using UnityEngine;
using MoonSharp.Interpreter;
using UnityEngine.Serialization;
using UnityEngine.Serialization;
[AddComponentMenu("Lua/Lua Behaviour")]
public class LuaBehaviour : MonoBehaviour
{
    public TextAsset script;
    [Tooltip("Add Lua attributes here with their names, types, and default values. Read them in Lua with script:GetAttribute(\"Name\").")]
    [FormerlySerializedAs("values")]
    public LuaValue[] attributes;
    [Tooltip("Editor only: restart this script automatically when its .lua file is saved during Play mode")]
    public bool hotReload = true;
    LuaScriptRef scriptRef;
    string loadedSource;
    bool hasStarted;
    LuaScriptRef pendingDisableOwner;
    public string RuntimeStatus { get; private set; } = "Not started";
    public string LastError { get; private set; }
    public object RuntimeOwner { get { return scriptRef; } }
#if UNITY_EDITOR
    string[] snapshots;
    float nextCheck;
#endif
    void Start()
    {
        hasStarted = true;
        Run();
    }
    void OnEnable()
    {
        if (hasStarted && Application.isPlaying)
        {
            if (pendingDisableOwner != null && LuaWorld.Exists) LuaWorld.Instance.Scheduler.KillOwner(pendingDisableOwner);
            pendingDisableOwner = null;
            Run();
        }
    }
    void Run()
    {
        if (script == null)
        {
            RuntimeStatus = "Missing script";
            LastError = "Assign a .lua or .luau script asset.";
            Debug.LogError("LuaBehaviour has no script assigned", this);
            return;
        }
        RuntimeStatus = "Starting";
        LastError = null;
        var world = LuaWorld.Instance;
        var lua = world.Lua;
        scriptRef = new LuaScriptRef(LuaInstance.Wrap(gameObject), script.name);
        if (attributes != null)
        {
            foreach (var v in attributes)
                if (v != null && !string.IsNullOrEmpty(v.name))
                    scriptRef.Load(v.name, v.ToLua());
        }
        loadedSource = script.text;
        LuaErrors.AssetPaths[script.name] = GetScriptAssetPath();
        string source = Luau.Transpile(loadedSource);
        LuaErrors.Sources[script.name] = source;
        var env = new Table(lua);
        var meta = new Table(lua);
        meta["__index"] = lua.Globals;
        env.MetaTable = meta;
        env["script"] = scriptRef;
        var unityApi = new LuaUnityApi(gameObject, world.RunService);
        env["unity"] = unityApi;
        env["Unity"] = unityApi;
        try
        {
            var chunk = lua.LoadString(source, env, script.name);
            world.Scheduler.SpawnOwned(scriptRef, chunk);
            RuntimeStatus = "Running";
        }
        catch (InterpreterException e)
        {
            RuntimeStatus = "Error";
            LastError = LuaErrors.Format(e);
            LuaErrors.Log(e, this);
        }
        if (scriptRef != null) scriptRef.Enabled.Fire();
#if UNITY_EDITOR
        snapshots = new string[attributes != null ? attributes.Length : 0];
        for (int i = 0; i < snapshots.Length; i++)
            if (attributes[i] != null) snapshots[i] = JsonUtility.ToJson(attributes[i]);
#endif
    }
    void Reload()
    {
        var world = LuaWorld.Instance;
        if (scriptRef != null) world.Scheduler.KillOwner(scriptRef);
        scriptRef = null;
        world.ClearModules();
        if (script != null) Debug.Log("[Lua] Reloaded " + script.name);
        Run();
    }
    string GetScriptAssetPath()
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.GetAssetPath(script);
#else
        return script != null ? script.name : string.Empty;
#endif
    }
    public void RestartScript()
    {
        if (scriptRef != null && LuaWorld.Exists) LuaWorld.Instance.Scheduler.KillOwner(scriptRef);
        scriptRef = null;
        if (LuaWorld.Exists) LuaWorld.Instance.ClearModules();
        Run();
    }
    void OnDestroy()
    {
        if (scriptRef != null) scriptRef.Destroying.Fire();
        StopScript();
    }
    void OnDisable()
    {
        if (!Application.isPlaying) return;
        if (scriptRef != null) scriptRef.Disabled.Fire();
        pendingDisableOwner = scriptRef;
        if (pendingDisableOwner != null && LuaWorld.Exists)
        {
            var owner = pendingDisableOwner;
            LuaWorld.Instance.ScheduleEndOfTick(() =>
            {
                if (scriptRef == owner && (!enabled || !gameObject.activeInHierarchy)) StopScript();
                if (pendingDisableOwner == owner) pendingDisableOwner = null;
            });
        }
    }
    void OnApplicationPause(bool paused)
    {
        if (scriptRef != null) scriptRef.ApplicationPaused.Fire(paused);
    }
    void StopScript()
    {
        if (scriptRef != null && LuaWorld.Exists)
            LuaWorld.Instance.Scheduler.KillOwner(scriptRef);
        scriptRef = null;
        pendingDisableOwner = null;
        if (RuntimeStatus == "Running" || RuntimeStatus == "Starting") RuntimeStatus = "Disabled";
    }
#if UNITY_EDITOR
    void Update()
    {
        if (script == null) return;
        if (hotReload && Time.unscaledTime >= nextCheck)
        {
            nextCheck = Time.unscaledTime + 0.5f;
            if (loadedSource != null && script.text != loadedSource)
            {
                Reload();
                return;
            }
        }
        if (scriptRef == null || attributes == null || snapshots == null) return;
        if (snapshots.Length != attributes.Length) System.Array.Resize(ref snapshots, attributes.Length);
        for (int i = 0; i < attributes.Length; i++)
        {
            var v = attributes[i];
            if (v == null || string.IsNullOrEmpty(v.name)) continue;
            string snap = JsonUtility.ToJson(v);
            if (snap == snapshots[i]) continue;
            snapshots[i] = snap;
            scriptRef.Apply(v.name, v.ToLua());
        }
    }
#endif
}
