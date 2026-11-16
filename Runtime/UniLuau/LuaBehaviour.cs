using UnityEngine;
using MoonSharp.Interpreter;
[AddComponentMenu("Lua/Lua Behaviour")]
public class LuaBehaviour : MonoBehaviour
{
    public TextAsset script;
    [Tooltip("Values you can edit here and read from Lua with script:GetAttribute(\"Name\")")]
    public LuaValue[] values;
    [Tooltip("Editor only: restart this script automatically when its .lua file is saved during Play mode")]
    public bool hotReload = true;
    [Header("Unity API Capabilities")]
    [Tooltip("Allow unity.PlayAudio and unity.StopAudio on this GameObject.")]
    public bool allowAudio = true;
    [Tooltip("Allow unity.SetAnimationTrigger and unity.SetAnimationFloat on this GameObject.")]
    public bool allowAnimation = true;
    [Tooltip("Allow unity.LoadScene and unity.LoadSceneAsync.")]
    public bool allowSceneLoading;
    LuaScriptRef scriptRef;
    string loadedSource;
    bool hasStarted;
    public string RuntimeStatus { get; private set; } = "Not started";
    public string LastError { get; private set; }
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
        if (hasStarted && Application.isPlaying) Run();
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
        if (values != null)
        {
            foreach (var v in values)
                if (v != null && !string.IsNullOrEmpty(v.name))
                    scriptRef.Load(v.name, v.ToLua());
        }
        loadedSource = script.text;
        string source = Luau.Transpile(loadedSource);
        LuaErrors.Sources[script.name] = source;
        var env = new Table(lua);
        var meta = new Table(lua);
        meta["__index"] = lua.Globals;
        env.MetaTable = meta;
        env["script"] = scriptRef;
        env["unity"] = new LuaUnityApi(gameObject, allowAudio, allowAnimation, allowSceneLoading);
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
            LuaErrors.Log(e);                 
        }
#if UNITY_EDITOR
        snapshots = new string[values != null ? values.Length : 0];
        for (int i = 0; i < snapshots.Length; i++)
            if (values[i] != null) snapshots[i] = JsonUtility.ToJson(values[i]);
#endif
    }
    void Reload()
    {
        var world = LuaWorld.Instance;
        if (scriptRef != null) world.Scheduler.KillOwner(scriptRef);
        scriptRef = null;
        world.ClearModules();
        Debug.Log("[Lua] Reloaded " + script.name);
        Run();
    }
    void OnDestroy()
    {
        StopScript();
    }
    void OnDisable()
    {
        if (Application.isPlaying) StopScript();
    }
    void StopScript()
    {
        if (scriptRef != null && LuaWorld.Exists)
            LuaWorld.Instance.Scheduler.KillOwner(scriptRef);
        scriptRef = null;
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
        if (scriptRef == null || values == null || snapshots == null) return;
        if (snapshots.Length != values.Length) System.Array.Resize(ref snapshots, values.Length);
        for (int i = 0; i < values.Length; i++)
        {
            var v = values[i];
            if (v == null || string.IsNullOrEmpty(v.name)) continue;
            string snap = JsonUtility.ToJson(v);
            if (snap == snapshots[i]) continue;
            snapshots[i] = snap;
            scriptRef.Apply(v.name, v.ToLua());
        }
    }
#endif
}
