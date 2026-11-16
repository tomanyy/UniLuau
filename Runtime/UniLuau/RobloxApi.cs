using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Serialization.Json;
[MoonSharpUserData]
public class Vec2
{
    public float X, Y;
    public Vec2(float x, float y) { X = x; Y = y; }
    public Vec2(Vector2 v) : this(v.x, v.y) { }
    [MoonSharpHidden] public Vector2 ToUnity() { return new Vector2(X, Y); }
    public float Magnitude { get { return ToUnity().magnitude; } }
    public Vec2 Unit { get { return new Vec2(ToUnity().normalized); } }
    public float Dot(Vec2 o) { return Vector2.Dot(ToUnity(), o.ToUnity()); }
    public Vec2 Lerp(Vec2 o, float t) { return new Vec2(Vector2.Lerp(ToUnity(), o.ToUnity(), t)); }
    public static Vec2 operator +(Vec2 a, Vec2 b) { return new Vec2(a.X + b.X, a.Y + b.Y); }
    public static Vec2 operator -(Vec2 a, Vec2 b) { return new Vec2(a.X - b.X, a.Y - b.Y); }
    public static Vec2 operator -(Vec2 a) { return new Vec2(-a.X, -a.Y); }
    public static Vec2 operator *(Vec2 a, float s) { return new Vec2(a.X * s, a.Y * s); }
    public static Vec2 operator *(float s, Vec2 a) { return new Vec2(a.X * s, a.Y * s); }
    public static Vec2 operator /(Vec2 a, float s) { return new Vec2(a.X / s, a.Y / s); }
    public override string ToString() { return X + ", " + Y; }
}
[MoonSharpUserData]
public class Vec3
{
    public float X, Y, Z;
    public Vec3(float x, float y, float z) { X = x; Y = y; Z = z; }
    public Vec3(Vector3 v) : this(v.x, v.y, v.z) { }
    [MoonSharpHidden] public Vector3 ToUnity() { return new Vector3(X, Y, Z); }
    public float Magnitude { get { return ToUnity().magnitude; } }
    public Vec3 Unit { get { return new Vec3(ToUnity().normalized); } }
    public float Dot(Vec3 o) { return Vector3.Dot(ToUnity(), o.ToUnity()); }
    public Vec3 Cross(Vec3 o) { return new Vec3(Vector3.Cross(ToUnity(), o.ToUnity())); }
    public Vec3 Lerp(Vec3 o, float t) { return new Vec3(Vector3.Lerp(ToUnity(), o.ToUnity(), t)); }
    public float Angle(Vec3 o) { return Vector3.Angle(ToUnity(), o.ToUnity()) * Mathf.Deg2Rad; }
    public static Vec3 operator +(Vec3 a, Vec3 b) { return new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z); }
    public static Vec3 operator -(Vec3 a, Vec3 b) { return new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z); }
    public static Vec3 operator -(Vec3 a) { return new Vec3(-a.X, -a.Y, -a.Z); }
    public static Vec3 operator *(Vec3 a, float s) { return new Vec3(a.X * s, a.Y * s, a.Z * s); }
    public static Vec3 operator *(float s, Vec3 a) { return new Vec3(a.X * s, a.Y * s, a.Z * s); }
    public static Vec3 operator /(Vec3 a, float s) { return new Vec3(a.X / s, a.Y / s, a.Z / s); }
    public override string ToString() { return X + ", " + Y + ", " + Z; }
}
[MoonSharpUserData]
public class Color3
{
    public float R, G, B;
    public Color3(float r, float g, float b) { R = r; G = g; B = b; }
    [MoonSharpHidden] public Color ToUnity() { return new Color(R, G, B); }
    public Color3 Lerp(Color3 o, float t)
    {
        return new Color3(R + (o.R - R) * t, G + (o.G - G) * t, B + (o.B - B) * t);
    }
    public string ToHex() { return "#" + ColorUtility.ToHtmlStringRGB(ToUnity()); }
    public DynValue ToHSV()
    {
        float h, s, v;
        Color.RGBToHSV(ToUnity(), out h, out s, out v);
        return DynValue.NewTuple(
            DynValue.NewNumber(h), DynValue.NewNumber(s), DynValue.NewNumber(v));
    }
    [MoonSharpHidden]
    public static Color3 FromHex(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return new Color3(0, 0, 0);
        hex = hex.Trim().TrimStart('#');
        if (hex.Length == 3)
            hex = new string(new[] { hex[0], hex[0], hex[1], hex[1], hex[2], hex[2] });
        if (hex.Length != 6) return new Color3(0, 0, 0);
        int r, g, b;
        if (!int.TryParse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out r) ||
            !int.TryParse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out g) ||
            !int.TryParse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out b))
            return new Color3(0, 0, 0);
        return new Color3(r / 255f, g / 255f, b / 255f);
    }
    public override string ToString() { return R + ", " + G + ", " + B; }
}
[MoonSharpUserData]
public class LuaCFrame
{
    [MoonSharpHidden] public readonly Vector3 Pos;
    [MoonSharpHidden] public readonly Quaternion Rot;
    [MoonSharpHidden] public LuaCFrame(Vector3 pos, Quaternion rot) { Pos = pos; Rot = rot; }
    [MoonSharpHidden]
    public static LuaCFrame LookAt(Vector3 from, Vector3 to)
    {
        Vector3 d = to - from;
        return new LuaCFrame(from, d.sqrMagnitude > 1e-8f ? Quaternion.LookRotation(d) : Quaternion.identity);
    }
    public Vec3 Position    { get { return new Vec3(Pos); } }
    public Vec3 LookVector  { get { return new Vec3(Rot * Vector3.forward); } }
    public Vec3 RightVector { get { return new Vec3(Rot * Vector3.right); } }
    public Vec3 UpVector    { get { return new Vec3(Rot * Vector3.up); } }
    public LuaCFrame Inverse()
    {
        Quaternion inv = Quaternion.Inverse(Rot);
        return new LuaCFrame(inv * -Pos, inv);
    }
    public LuaCFrame Lerp(LuaCFrame o, float t)
    {
        return new LuaCFrame(Vector3.Lerp(Pos, o.Pos, t), Quaternion.Slerp(Rot, o.Rot, t));
    }
    public DynValue ToEulerAnglesXYZ()
    {
        Vector3 e = Rot.eulerAngles;
        return DynValue.NewTuple(
            DynValue.NewNumber(NormalizeRadians(e.x)),
            DynValue.NewNumber(NormalizeRadians(e.y)),
            DynValue.NewNumber(NormalizeRadians(e.z)));
    }
    static double NormalizeRadians(float degrees)
    {
        return (Mathf.Repeat(degrees + 180f, 360f) - 180f) * Mathf.Deg2Rad;
    }
    public LuaCFrame ToWorldSpace(LuaCFrame o)  { return this * o; }
    public LuaCFrame ToObjectSpace(LuaCFrame o) { return Inverse() * o; }
    public Vec3 PointToWorldSpace(Vec3 v)  { return new Vec3(Pos + Rot * v.ToUnity()); }
    public Vec3 PointToObjectSpace(Vec3 v) { return new Vec3(Quaternion.Inverse(Rot) * (v.ToUnity() - Pos)); }
    public static LuaCFrame operator *(LuaCFrame a, LuaCFrame b) { return new LuaCFrame(a.Pos + a.Rot * b.Pos, a.Rot * b.Rot); }
    public static Vec3 operator *(LuaCFrame a, Vec3 v)           { return new Vec3(a.Pos + a.Rot * v.ToUnity()); }
    public static LuaCFrame operator +(LuaCFrame a, Vec3 v)      { return new LuaCFrame(a.Pos + v.ToUnity(), a.Rot); }
    public static LuaCFrame operator -(LuaCFrame a, Vec3 v)      { return new LuaCFrame(a.Pos - v.ToUnity(), a.Rot); }
    public override string ToString() { return Pos.x + ", " + Pos.y + ", " + Pos.z; }
}
[MoonSharpUserData]
public class LuaRay
{
    public Vec3 Origin, Direction;
    [MoonSharpHidden] public LuaRay(Vector3 origin, Vector3 direction)
    {
        Origin = new Vec3(origin);
        Direction = new Vec3(direction.normalized);
    }
}
[MoonSharpUserData]
public class LuaRandom
{
    readonly System.Random rng;
    [MoonSharpHidden] public LuaRandom() { rng = new System.Random(); }
    [MoonSharpHidden] public LuaRandom(int seed) { rng = new System.Random(seed); }
    public double NextNumber() { return rng.NextDouble(); }
    public double NextNumber(double min, double max) { return min + (max - min) * rng.NextDouble(); }
    public int NextInteger(int min, int max) { return rng.Next(min, max + 1); }                          
}
[MoonSharpUserData]
public class TaskHandle
{
    [MoonSharpHidden] public bool Cancelled;
}
[MoonSharpUserData]
public class SignalWaitToken
{
    [MoonSharpHidden] public readonly Signal Signal;
    [MoonSharpHidden] public SignalWaitToken(Signal s) { Signal = s; }
}
[MoonSharpUserData]
public class Connection
{
    readonly Signal signal;
    bool connected = true;
    [MoonSharpHidden] public readonly DynValue Fn;
    [MoonSharpHidden] public readonly object Owner;
    [MoonSharpHidden] public bool Once;
    [MoonSharpHidden]
    public Connection(Signal s, DynValue fn, object owner) { signal = s; Fn = fn; Owner = owner; }
    public bool Connected { get { return connected; } }
    public void Disconnect() { signal.Remove(this); }
    [MoonSharpHidden] public void MarkDisconnected() { connected = false; }
}
[MoonSharpUserData]
public class Signal
{
    readonly Scheduler scheduler;
    readonly List<Connection> connections = new List<Connection>();
    readonly List<Action<object[]>> waiters = new List<Action<object[]>>();
    [MoonSharpHidden] public event Action<object[]> Raised;
    [MoonSharpHidden] public Signal(Scheduler s) { scheduler = s; }
    public Connection Connect(DynValue fn)
    {
        var c = new Connection(this, fn, scheduler.CurrentOwner);
        connections.Add(c);
        scheduler.Register(c);
        return c;
    }
    public Connection Once(DynValue fn)
    {
        var c = Connect(fn);
        c.Once = true;
        return c;
    }
    public DynValue Wait()
    {
        var token = DynValue.FromObject(LuaWorld.Instance.Lua, new SignalWaitToken(this));
        return DynValue.NewYieldReq(new[] { token });
    }
    [MoonSharpHidden] public void AddWaiter(Action<object[]> w) { waiters.Add(w); }
    [MoonSharpHidden]
    public void Remove(Connection c)
    {
        connections.Remove(c);
        c.MarkDisconnected();
        scheduler.Unregister(c);
    }
    [MoonSharpHidden]
    public void Fire(params object[] args)
    {
        foreach (var c in connections.ToArray())
        {
            if (c.Once) Remove(c);
            scheduler.SpawnOwned(c.Owner, c.Fn, args);
        }
        if (waiters.Count > 0)
        {
            var ws = waiters.ToArray();
            waiters.Clear();
            foreach (var w in ws) w(args);
        }
        var raised = Raised;
        if (raised != null) raised(args);
    }
}
public class Scheduler
{
    class Job
    {
        public DynValue Co, DelayedFn;
        public object[] Args;
        public Action Callback;
        public float WakeAt;
        public object Owner;
        public TaskHandle Handle;
    }
    readonly Script script;
    readonly List<Job> jobs = new List<Job>();
    readonly List<Connection> allConnections = new List<Connection>();
    readonly HashSet<object> deadOwners = new HashSet<object>();
    readonly Dictionary<object, List<Action>> cleanups = new Dictionary<object, List<Action>>();
    float now;
    public object CurrentOwner;
    public Scheduler(Script s) { script = s; }
    bool IsDead(object owner) { return owner != null && deadOwners.Contains(owner); }
    public void Register(Connection c) { allConnections.Add(c); }
    public void Unregister(Connection c) { allConnections.Remove(c); }
    public void TrackCleanup(Action a)
    {
        if (CurrentOwner == null) return;
        List<Action> list;
        if (!cleanups.TryGetValue(CurrentOwner, out list)) { list = new List<Action>(); cleanups[CurrentOwner] = list; }
        list.Add(a);
    }
    public void TrackObject(GameObject go)
    {
        TrackCleanup(() => { if (go != null) UnityEngine.Object.Destroy(go); });
    }
    public void KillOwner(object owner)
    {
        if (owner == null) return;
        deadOwners.Add(owner);
        jobs.RemoveAll(j => j.Owner == owner);
        foreach (var c in allConnections.ToArray())
            if (c.Owner == owner) c.Disconnect();
        List<Action> list;
        if (cleanups.TryGetValue(owner, out list))
        {
            cleanups.Remove(owner);
            foreach (var a in list) a();
        }
    }
    public TaskHandle Spawn(DynValue fn, params object[] args)
    {
        return SpawnOwned(CurrentOwner, fn, args);
    }
    public TaskHandle SpawnOwned(object owner, DynValue fn, params object[] args)
    {
        var handle = new TaskHandle();
        if (IsDead(owner)) return handle;
        try
        {
            var dynArgs = new DynValue[args.Length];
            for (int i = 0; i < args.Length; i++) dynArgs[i] = DynValue.FromObject(script, args[i]);
            Resume(new Job { Co = script.CreateCoroutine(fn), Owner = owner, Handle = handle }, dynArgs);
        }
        catch (InterpreterException e)
        {
            LuaErrors.Log(e);
        }
        return handle;
    }
    public TaskHandle Delay(float seconds, DynValue fn, params object[] args)
    {
        var handle = new TaskHandle();
        jobs.Add(new Job { DelayedFn = fn, Args = args, WakeAt = now + seconds, Owner = CurrentOwner, Handle = handle });
        return handle;
    }
    public void DelayCall(float seconds, Action callback)
    {
        jobs.Add(new Job { Callback = callback, WakeAt = now + seconds });
    }
    void Resume(Job job, DynValue[] args)
    {
        if (job.Handle != null && job.Handle.Cancelled) return;
        if (IsDead(job.Owner)) return;
        object previous = CurrentOwner;
        CurrentOwner = job.Owner;
        try
        {
            DynValue result = job.Co.Coroutine.Resume(args);
            if (job.Co.Coroutine.State == CoroutineState.Suspended)
            {
                if (result.Type == DataType.Tuple && result.Tuple.Length > 0) result = result.Tuple[0];
                SignalWaitToken token = null;
                if (result.Type == DataType.UserData) token = result.UserData.Object as SignalWaitToken;
                if (token != null)
                {
                    token.Signal.AddWaiter(a =>
                    {
                        var dyn = new DynValue[a.Length];
                        for (int n = 0; n < a.Length; n++) dyn[n] = DynValue.FromObject(script, a[n]);
                        Resume(job, dyn);
                    });
                }
                else
                {
                    float wait = result.Type == DataType.Number ? (float)result.Number : 0f;
                    job.WakeAt = now + wait;
                    jobs.Add(job);
                }
            }
        }
        catch (InterpreterException e)
        {
            LuaErrors.Log(e);
        }
        finally
        {
            CurrentOwner = previous;
        }
    }
    public void Tick(float dt)
    {
        now += dt;
        var due = jobs.FindAll(j => j.WakeAt <= now);
        foreach (var j in due)
        {
            jobs.Remove(j);
            if (j.Handle != null && j.Handle.Cancelled) continue;
            if (IsDead(j.Owner)) continue;
            if (j.Callback != null) j.Callback();
            else if (j.Co == null) SpawnOwned(j.Owner, j.DelayedFn, j.Args ?? new object[0]);
            else Resume(j, new DynValue[0]);
        }
    }
}
[MoonSharpUserData]
public class LuaInstance
{
    static readonly Dictionary<GameObject, LuaInstance> cache = new Dictionary<GameObject, LuaInstance>();
    static readonly HashSet<GameObject> uiMaterialInstances = new HashSet<GameObject>();
    static GameObject prefabHolder;
    [MoonSharpHidden] public readonly GameObject Go;
    string className = "GameObject";
    object lastParent;
    bool hasCachedParent;
    Signal touched, touchEnded, childAdded, childRemoved, destroying;
    Signal changed;
    readonly Dictionary<string, Signal> changedSignals = new Dictionary<string, Signal>();
    readonly Dictionary<string, Signal> attributeSignals = new Dictionary<string, Signal>();
    readonly Dictionary<string, object> attributes = new Dictionary<string, object>();
    LuaInstance(GameObject go) { Go = go; }
    [MoonSharpHidden]
    public static LuaInstance Wrap(GameObject go)
    {
        if (go == null) return null;
        LuaInstance inst;
        if (!cache.TryGetValue(go, out inst)) { inst = new LuaInstance(go); cache[go] = inst; }
        return inst;
    }
    [MoonSharpHidden]
    public static LuaInstance WrapAs(GameObject go, string instanceClassName)
    {
        var inst = Wrap(go);
        if (inst != null && !string.IsNullOrEmpty(instanceClassName))
            inst.className = instanceClassName;
        return inst;
    }
    [MoonSharpHidden]
    public static void ClearMaterialCache() { uiMaterialInstances.Clear(); }
    [MoonSharpHidden]
    public static LuaInstance FromPrefab(GameObject prefab)
    {
        return FromPrefab(prefab, prefab != null ? prefab.name : "GameObject");
    }
    [MoonSharpHidden]
    public static LuaInstance FromPrefab(GameObject prefab, string instanceClassName)
    {
        if (prefabHolder == null)
        {
            prefabHolder = new GameObject("[LuaPrefabHolder]");
            prefabHolder.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(prefabHolder);
        }
        var go = UnityEngine.Object.Instantiate(prefab, prefabHolder.transform);
        go.SetActive(false);
        go.transform.SetParent(null, true);
        go.name = prefab.name;
        LuaWorld.Instance.Scheduler.TrackObject(go);
        var inst = Wrap(go);
        inst.className = string.IsNullOrEmpty(instanceClassName) ? "GameObject" : instanceClassName;
        return inst;
    }
    static Signal NewSignal() { return new Signal(LuaWorld.Instance.Scheduler); }
    static void NotifyAdded(object parent, LuaInstance child)
    {
        var pi = parent as LuaInstance;
        if (pi != null) { pi.RaiseChildAdded(child); return; }
        var scene = parent as LuaScene;
        if (scene != null) scene.ChildAdded.Fire(child);
    }
    static void NotifyRemoved(object parent, LuaInstance child)
    {
        var pi = parent as LuaInstance;
        if (pi != null) { pi.RaiseChildRemoved(child); return; }
        var scene = parent as LuaScene;
        if (scene != null) scene.ChildRemoved.Fire(child);
    }
    [MoonSharpHidden] public void RaiseChildAdded(LuaInstance c)   { if (childAdded != null) childAdded.Fire(c); }
    [MoonSharpHidden] public void RaiseChildRemoved(LuaInstance c) { if (childRemoved != null) childRemoved.Fire(c); }
    public string Name
    {
        get { return Go.name; }
        set { Go.name = value ?? ""; RaiseChanged("Name"); }
    }
    public string ClassName { get { return className; } }
    public LuaTransform Transform { get { return new LuaTransform(this); } }
    public bool Visible
    {
        get { return Go != null && Go.activeSelf; }
        set
        {
            if (Go == null) return;
            if (!value && Go.transform.parent == null && Go.activeSelf)
            {
                object parent = Parent;
                lastParent = parent;
                hasCachedParent = parent != null;
            }
            Go.SetActive(value);
            RaiseChanged("Visible");
            RaiseChanged("Active");
        }
    }
    public bool Active { get { return Visible; } set { Visible = value; } }
    public string Tag
    {
        get { return Go != null ? Go.tag : "Untagged"; }
        set
        {
            string next = string.IsNullOrEmpty(value) ? "Untagged" : value;
            try { Go.tag = next; }
            catch (Exception e)
            {
                throw new ScriptRuntimeException(
                    "Tag '" + next + "' is not defined in Unity's Tag Manager. " + e.Message);
            }
            RaiseChanged("Tag");
        }
    }
    public int Layer
    {
        get { return Go != null ? Go.layer : 0; }
        set
        {
            if (value < 0 || value > 31)
                throw new ScriptRuntimeException("Layer must be between 0 and 31.");
            Go.layer = value;
            RaiseChanged("Layer");
        }
    }
    public string LayerName
    {
        get { return LayerMask.LayerToName(Go != null ? Go.layer : 0); }
        set
        {
            string next = value ?? "";
            int index = LayerMask.NameToLayer(next);
            if (index < 0)
            {
                for (int i = 0; i < 32; i++)
                {
                    if (string.Equals(LayerMask.LayerToName(i), next, StringComparison.OrdinalIgnoreCase))
                    {
                        index = i;
                        break;
                    }
                }
            }
            if (index < 0)
                throw new ScriptRuntimeException(
                    "Layer '" + next + "' is not defined in Unity's Layers settings.");
            Go.layer = index;
            RaiseChanged("Layer");
        }
    }
    public LuaMaterial Material
    {
        get
        {
            if (Go == null) return null;
            var renderer = Go.GetComponent<Renderer>();
            if (renderer != null) return new LuaMaterial(renderer.material);
            var graphic = Go.GetComponent<Graphic>();
            if (graphic == null) return null;
            if (graphic.material != null && !uiMaterialInstances.Contains(Go))
            {
                graphic.material = new Material(graphic.material);
                uiMaterialInstances.Add(Go);
            }
            return new LuaMaterial(graphic.material);
        }
        set
        {
            if (Go == null || value == null) return;
            var renderer = Go.GetComponent<Renderer>();
            if (renderer != null) { renderer.material = value.UnityMaterial; return; }
            var graphic = Go.GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.material = value.UnityMaterial;
                uiMaterialInstances.Add(Go);
            }
        }
    }
    public Color3 Color
    {
        get { var m = Material; return m != null ? m.Color : new Color3(1, 1, 1); }
        set { var m = Material; if (m != null) m.Color = value; }
    }
    [MoonSharpHidden]
    public bool TryGetUnityComponent(string label, out object value)
    {
        value = null;
        Component namedComponent;
        if (!TryFindUnityComponent(label, out namedComponent)) return false;
        value = namedComponent is Transform
            ? (object)new LuaTransform(this)
            : new LuaUnityComponent(this, namedComponent);
        return true;
    }
    public LuaUnityComponent AddComponent(string className)
    {
        if (Go == null) throw new ScriptRuntimeException("GameObject has been destroyed.");
        Type type = LuaUnityTypes.ResolveComponent(className);
        if (type == null)
            throw new ScriptRuntimeException(
                "Unity component not found: " + className + ". Use a component type name " +
                "(for example 'Rigidbody') or an AddComponentMenu label.");
        if (type == typeof(Transform))
            throw new ScriptRuntimeException("A GameObject already has a Transform.");
        if (type == typeof(RectTransform) && Go.transform is RectTransform)
            return new LuaUnityComponent(this, Go.transform);
        Component component;
        try { component = Go.AddComponent(type); }
        catch (Exception e)
        {
            throw new ScriptRuntimeException(
                "Cannot add Unity component '" + className + "' (" + type.FullName + "): " + e.Message);
        }
        if (component == null)
            throw new ScriptRuntimeException(
                "Unity did not add component '" + className + "' (" + type.FullName + ").");
        RaiseChanged("Components");
        return new LuaUnityComponent(this, component);
    }
    public LuaUnityComponent GetComponent(string className)
    {
        if (Go == null) return null;
        Component component;
        if (TryFindUnityComponent(className, out component) && component != null)
            return new LuaUnityComponent(this, component);
        Type type = LuaUnityTypes.ResolveComponent(className);
        if (type == null) return null;
        component = Go.GetComponent(type);
        return component != null ? new LuaUnityComponent(this, component) : null;
    }
    [MoonSharpHidden]
    public bool TryGetUnityObjectProperty(object target, string label, out object value)
    {
        value = null;
        PropertyInfo property; FieldInfo field; object found;
        if (!TryFindUnityObjectMember(target, label, out found, out property, out field)) return false;
        object raw = property != null ? property.GetValue(found, null) : field.GetValue(found);
        if (raw == null) return true;
        if (raw.GetType().IsEnum) value = raw.ToString();
        else if (raw is Vector3) value = new Vec3((Vector3)raw);
        else if (raw is Vector2) value = new Vec2((Vector2)raw);
        else if (raw is Quaternion) value = new LuaCFrame(Vector3.zero, (Quaternion)raw);
        else if (raw is Color) { Color c = (Color)raw; value = new Color3(c.r, c.g, c.b); }
        else if (raw is GameObject) value = LuaInstance.Wrap((GameObject)raw);
        else if (raw is Transform) value = new LuaTransform(LuaInstance.Wrap(((Transform)raw).gameObject));
        else if (raw is Material) value = new LuaMaterial((Material)raw);
        else if (raw is UnityEngine.Object) value = new LuaUnityComponent(this, raw);
        else value = raw;
        return true;
    }
    [MoonSharpHidden]
    public bool TrySetUnityComponentProperty(Component component, string label, DynValue value, out string error)
    {
        return TrySetUnityObjectProperty((object)component, label, value, out error);
    }
    [MoonSharpHidden]
    public bool TrySetUnityObjectProperty(object target, string label, DynValue value, out string error)
    {
        error = null;
        object found;
        PropertyInfo property;
        FieldInfo field;
        if (!TryFindUnityObjectMember(target, label, out found, out property, out field)) return false;
        Type targetType = property != null ? property.PropertyType : field.FieldType;
        if (property != null && property.GetSetMethod() == null) return false;
        try
        {
            object converted = ConvertUnityValue(value, targetType);
            if (property != null) property.SetValue(found, converted, null);
            else field.SetValue(found, converted);
            RaiseChanged(label);
            return true;
        }
        catch (Exception e)
        {
            error = "Cannot set Unity property '" + label + "' on " + found.GetType().Name + ": " + e.Message;
            return false;
        }
    }
    static bool TryFindUnityObjectMember(object candidate, string label, out object found, out PropertyInfo property, out FieldInfo field)
    {
        found = null; property = null; field = null;
        if (candidate == null) return false;
        string wanted = NormalizeUnityMemberName(label);
        Type type = candidate.GetType();
        foreach (var p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (p.GetIndexParameters().Length != 0 || NormalizeUnityMemberName(p.Name) != wanted) continue;
            if (p.GetGetMethod() == null && p.GetSetMethod() == null) continue;
            found = candidate; property = p; return true;
        }
        foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            if (NormalizeUnityMemberName(f.Name) != wanted) continue;
            found = candidate; field = f; return true;
        }
        return false;
    }
    bool TryFindUnityComponent(string label, out Component result)
    {
        result = null;
        if (Go == null) return false;
        string wanted = NormalizeUnityMemberName(label);
        if ((wanted == "additionalcameradata" || wanted == "universaladditionalcameradata") && Go.GetComponent<Camera>() != null)
        {
            Type type = FindType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData");
            if (type != null)
            {
                result = Go.GetComponent(type) as Component;
                if (result == null) result = Go.AddComponent(type);
                return result != null;
            }
        }
        foreach (var candidate in Go.GetComponents<Component>())
        {
            if (candidate == null) continue;
            string typeName = candidate.GetType().Name;
            string shortName = typeName.EndsWith("Component", StringComparison.Ordinal)
                ? typeName.Substring(0, typeName.Length - "Component".Length) : typeName;
            if (wanted == NormalizeUnityMemberName(typeName) || wanted == NormalizeUnityMemberName(shortName))
            {
                result = candidate;
                return true;
            }
            if (wanted == "renderer" && candidate is Renderer) { result = candidate; return true; }
            if (wanted == "collider" && candidate is Collider) { result = candidate; return true; }
        }
        Type resolved = LuaUnityTypes.ResolveComponent(label);
        if (resolved != null && typeof(Component).IsAssignableFrom(resolved))
        {
            result = Go.GetComponent(resolved);
            if (result != null) return true;
        }
        return false;
    }
    static string NormalizeUnityMemberName(string name)
    {
        var chars = new List<char>();
        foreach (char c in name ?? "")
            if (char.IsLetterOrDigit(c)) chars.Add(char.ToLowerInvariant(c));
        return new string(chars.ToArray());
    }
    static Type FindType(string fullName)
    {
        return LuaUnityTypes.FindType(fullName);
    }
    static object ConvertUnityValue(DynValue value, Type targetType)
    {
        Type nullable = Nullable.GetUnderlyingType(targetType);
        if (nullable != null) targetType = nullable;
        if (value == null || value.IsNil())
        {
            if (!targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null) return null;
            throw new InvalidCastException("nil cannot be assigned to " + targetType.Name);
        }
        object raw = value.ToObject();
        var unityObject = raw as LuaUnityComponent;
        if (unityObject != null) raw = unityObject.UnityObject;
        var unityMaterial = raw as LuaMaterial;
        if (unityMaterial != null) raw = unityMaterial.UnityMaterial;
        if (targetType.IsEnum)
        {
            string enumName = value.CastToString();
            if (string.IsNullOrEmpty(enumName)) enumName = Convert.ToString(raw, CultureInfo.InvariantCulture);
            return Enum.Parse(targetType, enumName, true);
        }
        if (targetType == typeof(Vector3) && raw is Vec3) return ((Vec3)raw).ToUnity();
        if (targetType == typeof(Vector2) && raw is Vec2) return ((Vec2)raw).ToUnity();
        if (targetType == typeof(Color) && raw is Color3) return ((Color3)raw).ToUnity();
        if (targetType == typeof(Quaternion) && raw is LuaCFrame) return ((LuaCFrame)raw).Rot;
        if (targetType == typeof(Transform) && raw is LuaTransform) return ((LuaTransform)raw).UnityTransform;
        if (targetType == typeof(GameObject) && raw is LuaInstance) return ((LuaInstance)raw).Go;
        if (targetType.IsInstanceOfType(raw)) return raw;
        return Convert.ChangeType(raw, targetType, CultureInfo.InvariantCulture);
    }
    public object Parent
    {
        get
        {
            if (Go == null) return null;
            if (Go.transform.parent != null)
            {
                if (hasCachedParent && lastParent is LuaGuiObject)
                {
                    var guiParent = (LuaGuiObject)lastParent;
                    if (guiParent.RectTransform == Go.transform.parent ||
                        guiParent.ContentRoot == Go.transform.parent)
                        return lastParent;
                }
                object parent = Wrap(Go.transform.parent.gameObject);
                lastParent = parent;
                hasCachedParent = parent != null;
                return parent;
            }
            if (!Go.activeSelf) return hasCachedParent ? lastParent : null;
            Scene scene = Go.scene;
            object sceneParent = scene.IsValid() && scene.isLoaded ? LuaScene.Get(scene.name) : null;
            lastParent = sceneParent;
            hasCachedParent = sceneParent != null;
            return sceneParent;
        }
        set
        {
            object oldParent = Parent;
            bool applied = true;
            if (value is LuaInstance)
            {
                var parentInstance = (LuaInstance)value;
                if (parentInstance.Go == null) { applied = false; return; }
                Go.transform.SetParent(parentInstance.Go.transform, true);
                Go.SetActive(true);
            }
            else if (value is LuaScene)
            {
                var scene = (LuaScene)value;
                Go.transform.SetParent(null, true);
                if (scene.UnityScene.IsValid()) SceneManager.MoveGameObjectToScene(Go, scene.UnityScene);
                Go.SetActive(true);
            }
            else if (value is LuaGuiObject)
            {
                var gui = (LuaGuiObject)value;
                if (gui.Destroyed || gui.ContentRoot == null) { applied = false; return; }
                Go.transform.SetParent(gui.ContentRoot, false);
                Go.SetActive(true);
            }
            else if (value == null)
            {
                Go.transform.SetParent(null, true);
                Go.SetActive(false);
            }
            else
            {
                applied = false;
            }
            if (!applied) return;
            lastParent = value;
            hasCachedParent = value != null;
            object newParent = Parent;
            if (!ReferenceEquals(oldParent, newParent))
            {
                NotifyRemoved(oldParent, this);
                NotifyAdded(newParent, this);
            }
        }
    }
    LuaTouchRelay Relay()
    {
        var r = Go.GetComponent<LuaTouchRelay>();
        if (r == null) r = Go.AddComponent<LuaTouchRelay>();
        return r;
    }
    public Signal Touched
    {
        get
        {
            if (touched == null) { touched = NewSignal(); Relay().signal = touched; }
            return touched;
        }
    }
    public Signal TouchEnded
    {
        get
        {
            if (touchEnded == null) { touchEnded = NewSignal(); Relay().endedSignal = touchEnded; }
            return touchEnded;
        }
    }
    public Signal ChildAdded   { get { if (childAdded == null)   childAdded   = NewSignal(); return childAdded; } }
    public Signal ChildRemoved { get { if (childRemoved == null) childRemoved = NewSignal(); return childRemoved; } }
    public Signal Destroying   { get { if (destroying == null)   destroying   = NewSignal(); return destroying; } }
    public Signal Changed      { get { if (changed == null)      changed      = NewSignal(); return changed; } }
    public Signal GetPropertyChangedSignal(string property)
    {
        Signal s;
        if (!changedSignals.TryGetValue(property, out s))
        {
            s = NewSignal();
            changedSignals[property] = s;
        }
        return s;
    }
    [MoonSharpHidden]
    void RaiseChanged(string property)
    {
        if (changed != null) changed.Fire(property);
        Signal s;
        if (changedSignals.TryGetValue(property, out s)) s.Fire();
    }
    [MoonSharpHidden]
    public void NotifyUnityPropertyChanged(string property) { RaiseChanged(property); }
    public void Destroy()
    {
        if (Go == null) return;
        object parent = Parent;
        if (destroying != null) destroying.Fire();
        NotifyRemoved(parent, this);
        cache.Remove(Go);
        uiMaterialInstances.Remove(Go);
        UnityEngine.Object.Destroy(Go);
    }
    public void ClearAllChildren()
    {
        foreach (Transform t in Go.transform)
            Wrap(t.gameObject).Destroy();
    }
    public LuaInstance Clone()
    {
        var copy = UnityEngine.Object.Instantiate(Go);
        copy.SetActive(false);
        LuaWorld.Instance.Scheduler.TrackObject(copy);
        var inst = Wrap(copy);
        inst.className = className;
        return inst;
    }
    public LuaInstance FindFirstChild(string name)
    {
        var t = Go.transform.Find(name);
        return t != null ? Wrap(t.gameObject) : null;
    }
    public LuaInstance FindFirstChild(string name, bool recursive)
    {
        if (!recursive) return FindFirstChild(name);
        foreach (var t in Go.GetComponentsInChildren<Transform>(true))
            if (t != Go.transform && t.name == name) return Wrap(t.gameObject);
        return null;
    }
    public LuaInstance FindFirstChildOfClass(string cls)
    {
        foreach (Transform t in Go.transform)
        {
            if (HasComponent(t.gameObject, cls)) return Wrap(t.gameObject);
        }
        return null;
    }
    public LuaInstance FindFirstChildWhichIsA(string cls) { return FindFirstChildWhichIsA(cls, false); }
    public LuaInstance FindFirstChildWhichIsA(string cls, bool recursive)
    {
        foreach (Transform t in Go.transform)
        {
            var inst = Wrap(t.gameObject);
            if (inst != null && inst.IsA(cls)) return inst;
        }
        if (!recursive) return null;
        foreach (var t in Go.GetComponentsInChildren<Transform>(true))
        {
            if (t == Go.transform) continue;
            var inst = Wrap(t.gameObject);
            if (inst != null && inst.IsA(cls)) return inst;
        }
        return null;
    }
    public LuaInstance FindFirstAncestor(string ancestorName)
    {
        var t = Go.transform.parent;
        while (t != null)
        {
            if (t.name == ancestorName) return Wrap(t.gameObject);
            t = t.parent;
        }
        return null;
    }
    public LuaInstance FindFirstAncestorOfClass(string cls)
    {
        var t = Go.transform.parent;
        while (t != null)
        {
            if (HasComponent(t.gameObject, cls)) return Wrap(t.gameObject);
            t = t.parent;
        }
        return null;
    }
    public LuaInstance FindFirstAncestorWhichIsA(string cls)
    {
        var t = Go.transform.parent;
        while (t != null)
        {
            var inst = Wrap(t.gameObject);
            if (inst != null && inst.IsA(cls)) return inst;
            t = t.parent;
        }
        return null;
    }
    public string GetFullName()
    {
        var names = new List<string>();
        var t = Go.transform;
        while (t != null)
        {
            if (t.name.StartsWith("[Lua")) break;
            names.Insert(0, t.name);
            t = t.parent;
        }
        return string.Join(".", names);
    }
    public LuaInstance WaitForChild(string name) { return FindFirstChild(name); }
    public Table GetChildren()
    {
        var list = new List<LuaInstance>();
        foreach (Transform t in Go.transform) list.Add(Wrap(t.gameObject));
        return LuaWorld.ToTable(list);
    }
    public Table GetDescendants()
    {
        var list = new List<LuaInstance>();
        foreach (var t in Go.GetComponentsInChildren<Transform>(true))
            if (t != Go.transform) list.Add(Wrap(t.gameObject));
        return LuaWorld.ToTable(list);
    }
    public bool IsDescendantOf(LuaInstance other)
    {
        return other != null && Go.transform != other.Go.transform && Go.transform.IsChildOf(other.Go.transform);
    }
    public void SetAttribute(string name, DynValue value)
    {
        object next = value.ToObject();
        object old;
        bool had = attributes.TryGetValue(name, out old);
        attributes[name] = next;
        if (had && Equals(old, next)) return;
        Signal s;
        if (attributeSignals.TryGetValue(name, out s)) s.Fire();
    }
    public object GetAttribute(string name)
    {
        object v;
        return attributes.TryGetValue(name, out v) ? v : null;
    }
    public Table GetAttributes()
    {
        var t = new Table(LuaWorld.Instance.Lua);
        foreach (var pair in attributes) t[pair.Key] = pair.Value;
        return t;
    }
    public Signal GetAttributeChangedSignal(string name)
    {
        Signal s;
        if (!attributeSignals.TryGetValue(name, out s))
        {
            s = NewSignal();
            attributeSignals[name] = s;
        }
        return s;
    }
    public void LookAt(Vec3 target) { Go.transform.LookAt(target.ToUnity()); }
    public bool IsA(string cls)
    {
        if (string.Equals(className, cls, StringComparison.OrdinalIgnoreCase)) return true;
        if (cls == "GameObject") return true;
        return HasComponent(Go, cls);
    }
    static bool HasComponent(GameObject go, string cls)
    {
        if (go == null || string.IsNullOrEmpty(cls)) return false;
        string wanted = NormalizeUnityMemberName(cls);
        foreach (var candidate in go.GetComponents<Component>())
        {
            if (candidate == null) continue;
            string typeName = candidate.GetType().Name;
            string shortName = typeName.EndsWith("Component", StringComparison.Ordinal)
                ? typeName.Substring(0, typeName.Length - "Component".Length) : typeName;
            if (wanted == NormalizeUnityMemberName(typeName) || wanted == NormalizeUnityMemberName(shortName))
                return true;
        }
        Type resolved = LuaUnityTypes.ResolveComponent(cls);
        return resolved != null && go.GetComponent(resolved) != null;
    }
}
[MoonSharpUserData]
public class LuaTransform
{
    readonly LuaInstance owner;
    [MoonSharpHidden]
    public LuaTransform(LuaInstance instance) { owner = instance; }
    Transform Target { get { return owner != null && owner.Go != null ? owner.Go.transform : null; } }
    public Vec3 Position
    {
        get { return Target != null ? new Vec3(Target.position) : new Vec3(0, 0, 0); }
        set { if (Target != null && value != null) { Target.position = value.ToUnity(); owner.NotifyUnityPropertyChanged("Transform.Position"); } }
    }
    public Vec3 LocalPosition
    {
        get { return Target != null ? new Vec3(Target.localPosition) : new Vec3(0, 0, 0); }
        set { if (Target != null && value != null) { Target.localPosition = value.ToUnity(); owner.NotifyUnityPropertyChanged("Transform.LocalPosition"); } }
    }
    public Vec3 Scale
    {
        get { return Target != null ? new Vec3(Target.localScale) : new Vec3(1, 1, 1); }
        set { if (Target != null && value != null) { Target.localScale = value.ToUnity(); owner.NotifyUnityPropertyChanged("Transform.Scale"); } }
    }
    public Vec3 LocalScale { get { return Scale; } set { Scale = value; } }
    public Vec3 LossyScale { get { return Target != null ? new Vec3(Target.lossyScale) : Scale; } }
    public Vec3 Forward { get { return Target != null ? new Vec3(Target.forward) : new Vec3(0, 0, 1); } }
    public Vec3 Right { get { return Target != null ? new Vec3(Target.right) : new Vec3(1, 0, 0); } }
    public Vec3 Up { get { return Target != null ? new Vec3(Target.up) : new Vec3(0, 1, 0); } }
    public int ChildCount { get { return Target != null ? Target.childCount : 0; } }
    public LuaInstance Parent
    {
        get { return Target != null && Target.parent != null ? LuaInstance.Wrap(Target.parent.gameObject) : null; }
        set
        {
            if (Target == null) return;
            Target.SetParent(value != null ? value.Go.transform : null, true);
            owner.NotifyUnityPropertyChanged("Transform.Parent");
        }
    }
    public LuaInstance Find(string childName)
    {
        if (Target == null) return null;
        Transform child = Target.Find(childName);
        return child != null ? LuaInstance.Wrap(child.gameObject) : null;
    }
    public LuaInstance GetChild(int index)
    {
        if (Target == null || index < 1 || index > Target.childCount) return null;
        return LuaInstance.Wrap(Target.GetChild(index - 1).gameObject);
    }
    [MoonSharpHidden]
    public Transform UnityTransform { get { return Target; } }
    public Vec3 Rotation
    {
        get { return Target != null ? new Vec3(Target.eulerAngles) : new Vec3(0, 0, 0); }
        set { if (Target != null && value != null) { Target.eulerAngles = value.ToUnity(); owner.NotifyUnityPropertyChanged("Transform.Rotation"); } }
    }
    public Vec3 LocalRotation
    {
        get { return Target != null ? new Vec3(Target.localEulerAngles) : new Vec3(0, 0, 0); }
        set { if (Target != null && value != null) { Target.localEulerAngles = value.ToUnity(); owner.NotifyUnityPropertyChanged("Transform.LocalRotation"); } }
    }
}
[MoonSharpUserData]
public class LuaUnityComponent
{
    readonly LuaInstance owner;
    readonly object component;
    [MoonSharpHidden]
    public LuaUnityComponent(LuaInstance instance, object value) { owner = instance; component = value; }
    public string ClassName { get { return component != null ? component.GetType().Name : "DestroyedComponent"; } }
    [MoonSharpHidden]
    public object UnityObject { get { return component; } }
    [MoonSharpHidden]
    public bool TryGetProperty(string label, out object value)
    {
        if (owner == null) { value = null; return false; }
        return owner.TryGetUnityObjectProperty(component, label, out value);
    }
    [MoonSharpHidden]
    public bool TrySetProperty(string label, DynValue value, out string error)
    {
        if (owner == null) { error = null; return false; }
        return owner.TrySetUnityObjectProperty(component, label, value, out error);
    }
    [MoonSharpHidden]
    public LuaUnityComponentSection GetSection(string name)
    {
        return new LuaUnityComponentSection(this, name);
    }
}
[MoonSharpUserData]
public class LuaUnityComponentSection
{
    readonly LuaUnityComponent component;
    [MoonSharpHidden]
    public LuaUnityComponentSection(LuaUnityComponent component, string name)
    {
        this.component = component;
        Name = name ?? "";
    }
    public string Name { get; private set; }
    public string ClassName { get { return component != null ? component.ClassName : "DestroyedComponent"; } }
    [MoonSharpHidden]
    public object UnityObject { get { return component != null ? component.UnityObject : null; } }
    [MoonSharpHidden]
    public bool TryGetProperty(string label, out object value)
    {
        if (component == null) { value = null; return false; }
        return component.TryGetProperty(label, out value);
    }
    [MoonSharpHidden]
    public bool TrySetProperty(string label, DynValue value, out string error)
    {
        if (component == null) { error = null; return false; }
        return component.TrySetProperty(label, value, out error);
    }
    [MoonSharpHidden]
    public LuaUnityComponentSection GetSection(string name)
    {
        return new LuaUnityComponentSection(component, name);
    }
}
[MoonSharpUserData]
public class LuaRayResult
{
    public LuaInstance Instance;
    public Vec3 Position, Normal;
    public float Distance;
    [MoonSharpHidden]
    public LuaRayResult(RaycastHit h)
    {
        Instance = LuaInstance.Wrap(h.collider.gameObject);
        Position = new Vec3(h.point);
        Normal = new Vec3(h.normal);
        Distance = h.distance;
    }
}
[MoonSharpUserData]
public class LuaRaycastParams
{
    public string FilterType = "Exclude";
    public Table FilterDescendantsInstances;
    [MoonSharpHidden]
    public bool Allows(GameObject go)
    {
        bool inList = false;
        if (FilterDescendantsInstances != null)
        {
            foreach (var v in FilterDescendantsInstances.Values)
            {
                var inst = v.Type == DataType.UserData ? v.UserData.Object as LuaInstance : null;
                if (inst != null && inst.Go != null && go.transform.IsChildOf(inst.Go.transform)) { inList = true; break; }
            }
        }
        return FilterType == "Include" ? inList : !inList;
    }
}
[MoonSharpUserData]
public class LuaScene
{
    static readonly Dictionary<string, LuaScene> cache = new Dictionary<string, LuaScene>(StringComparer.Ordinal);
    readonly string sceneName;
    public Signal ChildAdded, ChildRemoved;
    [MoonSharpHidden]
    public LuaScene(string name)
    {
        sceneName = name;
        ChildAdded = new Signal(LuaWorld.Instance.Scheduler);
        ChildRemoved = new Signal(LuaWorld.Instance.Scheduler);
    }
    [MoonSharpHidden]
    public static LuaScene Get(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        LuaScene scene;
        if (!cache.TryGetValue(name, out scene))
        {
            scene = new LuaScene(name);
            cache[name] = scene;
        }
        return scene;
    }
    [MoonSharpHidden]
    public static void ClearCache() { cache.Clear(); }
    public string Name { get { return sceneName; } }
    [MoonSharpHidden]
    public Scene UnityScene
    {
        get
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            return scene.IsValid() && scene.isLoaded ? scene : default(Scene);
        }
    }
    public LuaInstance CurrentCamera
    {
        get
        {
            Camera fallback = null;
            Scene scene = UnityScene;
            if (scene.IsValid())
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var camera in root.GetComponentsInChildren<Camera>(true))
                    {
                        if (camera.CompareTag("MainCamera")) return LuaInstance.Wrap(camera.gameObject);
                        if (fallback == null) fallback = camera;
                    }
                }
            }
            Camera main = Camera.main;
            if (main != null) return LuaInstance.Wrap(main.gameObject);
            return fallback != null ? LuaInstance.Wrap(fallback.gameObject) : null;
        }
    }
    public float Gravity
    {
        get { return -Physics.gravity.y; }
        set { Physics.gravity = new Vector3(0, -value, 0); }
    }
    public LuaInstance FindChild(string name)
    {
        Scene scene = UnityScene;
        if (!scene.IsValid()) return null;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == name) return LuaInstance.Wrap(root);
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t != root.transform && t.name == name) return LuaInstance.Wrap(t.gameObject);
        }
        return null;
    }
    public LuaInstance FindFirstChild(string name) { return FindChild(name); }
    public LuaInstance FindFirstChild(string name, bool recursive)
    {
        Scene scene = UnityScene;
        if (!scene.IsValid()) return null;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == name) return LuaInstance.Wrap(root);
            if (!recursive) continue;
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t != root.transform && t.name == name) return LuaInstance.Wrap(t.gameObject);
        }
        return null;
    }
    public LuaInstance FindFirstChildOfClass(string cls)
    {
        Scene scene = UnityScene;
        if (!scene.IsValid()) return null;
        foreach (var root in scene.GetRootGameObjects())
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (LuaInstance.Wrap(t.gameObject).IsA(cls)) return LuaInstance.Wrap(t.gameObject);
        return null;
    }
    public LuaInstance WaitForChild(string name) { return FindFirstChild(name); }
    public Table GetChildren()
    {
        var list = new List<LuaInstance>();
        Scene scene = UnityScene;
        if (scene.IsValid())
            foreach (var root in scene.GetRootGameObjects())
                list.Add(LuaInstance.Wrap(root));
        return LuaWorld.ToTable(list);
    }
    public Table GetDescendants()
    {
        var list = new List<LuaInstance>();
        Scene scene = UnityScene;
        if (scene.IsValid())
            foreach (var root in scene.GetRootGameObjects())
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    list.Add(LuaInstance.Wrap(t.gameObject));
        return LuaWorld.ToTable(list);
    }
    public LuaRayResult Raycast(Vec3 origin, Vec3 direction)
    {
        return Raycast(origin, direction, null);
    }
    public LuaRayResult Raycast(Vec3 origin, Vec3 direction, LuaRaycastParams p)
    {
        Vector3 d = direction.ToUnity();
        return Closest(Physics.RaycastAll(origin.ToUnity(), d.normalized, d.magnitude), p);
    }
    public LuaRayResult RaycastFromMouse(float maxDistance)
    {
        return RaycastFromMouse(maxDistance, null);
    }
    public LuaRayResult RaycastFromMouse(float maxDistance, LuaRaycastParams p)
    {
        var cam = Camera.main;
        var m = Mouse.current;
        if (cam == null || m == null) return null;
        Ray ray = cam.ScreenPointToRay(m.position.ReadValue());
        return Closest(Physics.RaycastAll(ray, maxDistance > 0 ? maxDistance : 1000f), p);
    }
    public Table GetPartBoundsInRadius(Vec3 position, float radius)
    {
        return ToInstances(Physics.OverlapSphere(position.ToUnity(), radius));
    }
    public Table GetPartBoundsInBox(LuaCFrame cframe, Vec3 size)
    {
        return ToInstances(Physics.OverlapBox(cframe.Pos, size.ToUnity() * 0.5f, cframe.Rot));
    }
    static LuaRayResult Closest(RaycastHit[] hits, LuaRaycastParams p)
    {
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var h in hits)
            if (p == null || p.Allows(h.collider.gameObject)) return new LuaRayResult(h);
        return null;
    }
    static Table ToInstances(Collider[] cols)
    {
        var seen = new HashSet<GameObject>();
        var list = new List<LuaInstance>();
        foreach (var c in cols)
            if (seen.Add(c.gameObject)) list.Add(LuaInstance.Wrap(c.gameObject));
        return LuaWorld.ToTable(list);
    }
}
[MoonSharpUserData]
public class LuaInput
{
    public string KeyCode;
    public string UserInputType;
    [MoonSharpHidden] public LuaInput(string key, string type) { KeyCode = key; UserInputType = type; }
}
[MoonSharpUserData]
public class LuaMouse
{
    public float X { get { var m = Mouse.current; return m == null ? 0f : m.position.ReadValue().x; } }
    public float Y { get { var m = Mouse.current; return m == null ? 0f : Screen.height - m.position.ReadValue().y; } }
    bool Cast(out RaycastHit hit)
    {
        hit = new RaycastHit();
        var cam = Camera.main;
        var m = Mouse.current;
        if (cam == null || m == null) return false;
        return Physics.Raycast(cam.ScreenPointToRay(m.position.ReadValue()), out hit, 1000f);
    }
    public LuaInstance Target
    {
        get { RaycastHit h; return Cast(out h) ? LuaInstance.Wrap(h.collider.gameObject) : null; }
    }
    public LuaCFrame Hit
    {
        get
        {
            RaycastHit h;
            if (Cast(out h)) return new LuaCFrame(h.point, Quaternion.identity);
            var cam = Camera.main;
            var m = Mouse.current;
            if (cam == null || m == null) return new LuaCFrame(Vector3.zero, Quaternion.identity);
            return new LuaCFrame(cam.ScreenPointToRay(m.position.ReadValue()).GetPoint(1000f), Quaternion.identity);
        }
    }
}
[MoonSharpUserData]
public class LuaRunService
{
    public Signal Stepped, RenderStepped, Heartbeat;
    public Signal FixedUpdate, LateUpdate;
    [MoonSharpHidden]
    public LuaRunService(Signal stepped, Signal renderStepped, Signal heartbeat, Signal fixedUpdate, Signal lateUpdate)
    {
        Stepped = stepped;
        RenderStepped = renderStepped;
        Heartbeat = heartbeat;
        FixedUpdate = fixedUpdate;
        LateUpdate = lateUpdate;
    }
    public Signal Update { get { return Heartbeat; } }
}
[MoonSharpUserData]
public class LuaUserInputService
{
    public Signal InputBegan, InputEnded, TouchStarted, TouchEnded, TouchMoved;
    readonly LuaMouse mouse = new LuaMouse();
    [MoonSharpHidden]
    public static readonly string[] PadButtonNames =
    {
        "ButtonA", "ButtonB", "ButtonX", "ButtonY", "ButtonL1", "ButtonR1", "ButtonL2", "ButtonR2",
        "ButtonL3", "ButtonR3", "ButtonStart", "ButtonSelect", "DPadUp", "DPadDown", "DPadLeft", "DPadRight"
    };
    [MoonSharpHidden]
    public static ButtonControl PadButton(Gamepad g, string name)
    {
        switch (name)
        {
            case "ButtonA":      return g.buttonSouth;
            case "ButtonB":      return g.buttonEast;
            case "ButtonX":      return g.buttonWest;
            case "ButtonY":      return g.buttonNorth;
            case "ButtonL1":     return g.leftShoulder;
            case "ButtonR1":     return g.rightShoulder;
            case "ButtonL2":     return g.leftTrigger;
            case "ButtonR2":     return g.rightTrigger;
            case "ButtonL3":     return g.leftStickButton;
            case "ButtonR3":     return g.rightStickButton;
            case "ButtonStart":  return g.startButton;
            case "ButtonSelect": return g.selectButton;
            case "DPadUp":       return g.dpad.up;
            case "DPadDown":     return g.dpad.down;
            case "DPadLeft":     return g.dpad.left;
            case "DPadRight":    return g.dpad.right;
        }
        return null;
    }
    [MoonSharpHidden]
    public LuaUserInputService(Signal began, Signal ended, Signal touchStarted, Signal touchEnded, Signal touchMoved)
    {
        InputBegan = began;
        InputEnded = ended;
        TouchStarted = touchStarted;
        TouchEnded = touchEnded;
        TouchMoved = touchMoved;
    }
    public bool IsKeyDown(string key)
    {
        if (key.StartsWith("Button") || key.StartsWith("DPad"))
        {
            var pad = Gamepad.current;
            var b = pad != null ? PadButton(pad, key) : null;
            return b != null && b.isPressed;
        }
        var kb = Keyboard.current;
        Key k;
        return kb != null && Enum.TryParse(key, out k) && k != Key.None && kb[k].isPressed;
    }
    public bool IsMouseButtonPressed(string type)
    {
        var m = Mouse.current;
        if (m == null) return false;
        return type == "MouseButton1" ? m.leftButton.isPressed : (type == "MouseButton2" && m.rightButton.isPressed);
    }
    public LuaMouse GetMouse() { return mouse; }
    public Vec3 GetMouseDelta()
    {
        var m = Mouse.current;
        if (m == null) return new Vec3(0, 0, 0);
        Vector2 d = m.delta.ReadValue();
        return new Vec3(d.x, d.y, 0);
    }
    public float GetMouseWheel()
    {
        var m = Mouse.current;
        return m == null ? 0f : Math.Sign(m.scroll.ReadValue().y);
    }
    public Vec2 GetMouseLocation()
    {
        var m = Mouse.current;
        if (m == null) return new Vec2(0, 0);
        Vector2 p = m.position.ReadValue();
        return new Vec2(p.x, Screen.height - p.y);
    }
    public string MouseBehavior
    {
        get { return Cursor.lockState == CursorLockMode.Locked ? "LockCenter" : "Default"; }
        set
        {
            bool locked = value == "LockCenter";
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
    public bool GamepadConnected { get { return Gamepad.current != null; } }
    public Vec2 GetGamepadThumbstick(string name)
    {
        var pad = Gamepad.current;
        if (pad == null) return new Vec2(0, 0);
        return new Vec2(name == "Thumbstick2" ? pad.rightStick.ReadValue() : pad.leftStick.ReadValue());
    }
    public float GetGamepadTrigger(string name)
    {
        var pad = Gamepad.current;
        if (pad == null) return 0f;
        var b = PadButton(pad, name);
        return b != null ? b.ReadValue() : 0f;
    }
    public bool TouchEnabled { get { return Touchscreen.current != null; } }
}
[MoonSharpUserData]
public class LuaScriptRef
{
    public LuaInstance Parent;
    public string Name;
    public Signal AttributeChanged;
    readonly Dictionary<string, object> attributes = new Dictionary<string, object>();
    readonly Dictionary<string, Signal> changedSignals = new Dictionary<string, Signal>();
    [MoonSharpHidden]
    public LuaScriptRef(LuaInstance parent, string name)
    {
        Parent = parent;
        Name = name;
        AttributeChanged = new Signal(LuaWorld.Instance.Scheduler);
    }
    public object GetAttribute(string name)
    {
        object v;
        return attributes.TryGetValue(name, out v) ? v : null;
    }
    public void SetAttribute(string name, DynValue value) { Apply(name, value.ToObject()); }
    public Table GetAttributes()
    {
        var t = new Table(LuaWorld.Instance.Lua);
        foreach (var pair in attributes) t[pair.Key] = pair.Value;
        return t;
    }
    public Signal GetAttributeChangedSignal(string name)
    {
        Signal s;
        if (!changedSignals.TryGetValue(name, out s))
        {
            s = new Signal(LuaWorld.Instance.Scheduler);
            changedSignals[name] = s;
        }
        return s;
    }
    [MoonSharpHidden] public void Load(string name, object value) { attributes[name] = value; }
    [MoonSharpHidden]
    public void Apply(string name, object value)
    {
        object old;
        bool had = attributes.TryGetValue(name, out old);
        attributes[name] = value;
        if (had && Equals(old, value)) return;
        Signal s;
        if (changedSignals.TryGetValue(name, out s)) s.Fire();
        AttributeChanged.Fire(name);
    }
}
[MoonSharpUserData]
public class LuaDebris
{
    public void AddItem(LuaInstance item) { AddItem(item, 10f); }
    public void AddItem(LuaInstance item, float seconds)
    {
        if (item == null) return;
        LuaWorld.Instance.Scheduler.DelayCall(seconds, () =>
        {
            if (item.Go != null) item.Destroy();                                                        
        });
    }
}
[MoonSharpUserData]
public class LuaBindableEvent
{
    public Signal Event;
    readonly GameObject go;
    object parent;
    [MoonSharpHidden]
    public LuaBindableEvent(Scheduler s)
    {
        Event = new Signal(s);
        go = LuaPrefabObjects.Create("BindableEvent");
        s.TrackCleanup(Destroy);
    }
    public object Parent
    {
        get { return parent; }
        set { parent = value; LuaPrefabObjects.SetParent(go, value); }
    }
    public string Name
    {
        get { return go != null ? go.name : "BindableEvent"; }
        set { if (go != null) go.name = value ?? "BindableEvent"; }
    }
    public void Destroy()
    {
        parent = null;
        if (go != null) UnityEngine.Object.Destroy(go);
    }
    public void Fire(params object[] args) { Event.Fire(args); }
}
[MoonSharpUserData]
public class LuaCollectionService
{
    readonly Dictionary<string, HashSet<LuaInstance>> tags = new Dictionary<string, HashSet<LuaInstance>>();
    public void AddTag(LuaInstance inst, string tag)
    {
        if (inst == null) return;
        HashSet<LuaInstance> set;
        if (!tags.TryGetValue(tag, out set)) { set = new HashSet<LuaInstance>(); tags[tag] = set; }
        set.Add(inst);
    }
    public void RemoveTag(LuaInstance inst, string tag)
    {
        HashSet<LuaInstance> set;
        if (tags.TryGetValue(tag, out set)) set.Remove(inst);
    }
    public bool HasTag(LuaInstance inst, string tag)
    {
        HashSet<LuaInstance> set;
        return tags.TryGetValue(tag, out set) && set.Contains(inst);
    }
    public Table GetTagged(string tag)
    {
        var list = new List<LuaInstance>();
        HashSet<LuaInstance> set;
        if (tags.TryGetValue(tag, out set))
        {
            set.RemoveWhere(i => i.Go == null);                            
            list.AddRange(set);
        }
        return LuaWorld.ToTable(list);
    }
    public Table GetTags(LuaInstance inst)
    {
        var t = new Table(LuaWorld.Instance.Lua);
        foreach (var pair in tags)
            if (pair.Value.Contains(inst)) t.Append(DynValue.NewString(pair.Key));
        return t;
    }
}
[MoonSharpUserData]
public class LuaHttpService
{
    public string JSONEncode(Table t) { return JsonTableConverter.TableToJson(t); }
    public Table JSONDecode(string json) { return JsonTableConverter.JsonToTable(json, LuaWorld.Instance.Lua); }
    public string GenerateGUID() { return Guid.NewGuid().ToString(); }
}
[MoonSharpUserData]
public class LuaGame
{
    readonly LuaRunService run;
    readonly LuaUserInputService uis;
    readonly LuaDebris debris = new LuaDebris();
    readonly LuaCollectionService collection = new LuaCollectionService();
    readonly LuaHttpService http = new LuaHttpService();
    readonly Dictionary<string, DynValue> luaServices = new Dictionary<string, DynValue>();
    const string WorkspaceMessage =
        "Workspace does not exist in Unity. Use game.<SceneName> (for example game.SampleScene) instead.";
    [MoonSharpHidden]
    public LuaGame(LuaRunService r, LuaUserInputService u) { run = r; uis = u; }
    [MoonSharpHidden]
    public LuaScene FindScene(string name)
    {
        if (string.Equals(name, "Workspace", StringComparison.OrdinalIgnoreCase))
            throw new ScriptRuntimeException(WorkspaceMessage);
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded && string.Equals(scene.name, name, StringComparison.OrdinalIgnoreCase))
                return LuaScene.Get(scene.name);
        }
        return null;
    }
    public Table GetScenes()
    {
        var table = new Table(LuaWorld.Instance.Lua);
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded) table.Append(DynValue.FromObject(LuaWorld.Instance.Lua, LuaScene.Get(scene.name)));
        }
        return table;
    }
    [MoonSharpHidden]
    public void RegisterService(string name, DynValue svc) { luaServices[name] = svc; }
    public object GetService(string name)
    {
        switch (name)
        {
            case "RunService": return run;
            case "UserInputService": return uis;
            case "Debris": return debris;
            case "CollectionService": return collection;
            case "HttpService": return http;
        }
        if (string.Equals(name, "Workspace", StringComparison.OrdinalIgnoreCase))
            throw new ScriptRuntimeException(WorkspaceMessage);
        DynValue s;
        if (luaServices.TryGetValue(name, out s)) return s;
        throw new ScriptRuntimeException("Unknown service: " + name);
    }
}
public class LuaWorld
{
    static LuaWorld inst;
    public static LuaWorld Instance { get { if (inst == null) inst = new LuaWorld(); return inst; } }
    public static bool Exists { get { return inst != null; } }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset()
    {
        inst = null;
        LuaScene.ClearCache();
        LuaUnityTypes.ClearCache();
        LuaInstance.ClearMaterialCache();
    }
    public Script Lua;
    public Scheduler Scheduler;
    public LuaPhysicsService PhysicsService;
    public LuaContextActionService ContextActionService;
    public LuaRunService RunService;
    Signal heartbeat, renderStepped, stepped;
    Signal fixedUpdate, lateUpdate;
    Signal inputBegan, inputEnded, touchStarted, touchEnded, touchMoved;
    readonly Dictionary<string, DynValue> moduleCache = new Dictionary<string, DynValue>();
    readonly HashSet<string> loading = new HashSet<string>();
    public void ClearModules() { moduleCache.Clear(); }
    static float N(CallbackArguments a, int i) { return a.Count > i ? (float)(a[i].CastToNumber() ?? 0) : 0f; }
    static bool SameLuaValue(DynValue a, DynValue b)
    {
        if (a == null || b == null || a.Type != b.Type) return false;
        switch (a.Type)
        {
            case DataType.Nil:     return true;
            case DataType.Number:  return a.Number == b.Number;
            case DataType.String:  return a.String == b.String;
            case DataType.Boolean: return a.Boolean == b.Boolean;
            default:               return a.ReferenceID == b.ReferenceID;
        }
    }
    static object[] Rest(CallbackArguments a, int from)
    {
        int n = Math.Max(0, a.Count - from);
        var r = new object[n];
        for (int i = 0; i < n; i++) r[i] = a[from + i];
        return r;
    }
    static Table MakeEnum(Script lua, params string[] names)
    {
        var t = new Table(lua);
        foreach (var n in names) t[n] = n;
        return t;
    }
    public static Table ToTable(List<LuaInstance> items)
    {
        var t = new Table(Instance.Lua);
        foreach (var i in items) t.Append(DynValue.FromObject(Instance.Lua, i));
        return t;
    }
    LuaWorld()
    {
        LuaIndexing.Register();
        UserData.RegisterAssembly();
        Lua = new Script(CoreModules.Preset_SoftSandbox);
        Lua.Options.DebugPrint = s => Debug.Log(s);
        Scheduler = new Scheduler(Lua);
        heartbeat     = new Signal(Scheduler);
        renderStepped = new Signal(Scheduler);
        stepped       = new Signal(Scheduler);
        fixedUpdate   = new Signal(Scheduler);
        lateUpdate    = new Signal(Scheduler);
        inputBegan    = new Signal(Scheduler);
        inputEnded    = new Signal(Scheduler);
        touchStarted  = new Signal(Scheduler);
        touchEnded    = new Signal(Scheduler);
        touchMoved    = new Signal(Scheduler);
        var run  = new LuaRunService(stepped, renderStepped, heartbeat, fixedUpdate, lateUpdate);
        RunService = run;
        var uis  = new LuaUserInputService(inputBegan, inputEnded, touchStarted, touchEnded, touchMoved);
        var game = new LuaGame(run, uis);
        PhysicsService = new LuaPhysicsService();
        ContextActionService = new LuaContextActionService(Scheduler, inputBegan, inputEnded);
        var g = Lua.Globals;
        g["game"] = game;
        g["warn"] = (Action<string>)(s => Debug.LogWarning(s));
        SetupCore(g);
        SetupTypes(g);
        SetupServices(game);
        var go = new GameObject("[LuaWorld]");
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.AddComponent<LuaWorldDriver>().world = this;
        go.AddComponent<LuaGuiRenderer>();
    }
    void SetupCore(Table g)
    {
        var math = g.Get("math").Table;
        math["clamp"] = (Func<double, double, double, double>)((x, lo, hi) => Math.Max(lo, Math.Min(hi, x)));
        math["sign"]  = (Func<double, double>)(x => Math.Sign(x));
        math["round"] = (Func<double, double>)(x => Math.Round(x, MidpointRounding.AwayFromZero));
        math["lerp"]  = (Func<double, double, double, double>)((a, b, t) => a + (b - a) * t);
        DynValue osValue = g.Get("os");
        Table os = osValue.Type == DataType.Table ? osValue.Table : new Table(Lua);
        g["os"] = os;
        os["time"] = DynValue.NewCallback((ctx, a) =>
        {
            DateTime value = DateTime.Now;
            if (a.Count > 0 && a[0].Type == DataType.Table)
            {
                Table fields = a[0].Table;
                int year = (int)(fields.Get("year").CastToNumber() ?? value.Year);
                int month = (int)(fields.Get("month").CastToNumber() ?? value.Month);
                int day = (int)(fields.Get("day").CastToNumber() ?? value.Day);
                int hour = (int)(fields.Get("hour").CastToNumber() ?? 12);
                int minute = (int)(fields.Get("min").CastToNumber() ?? 0);
                int second = (int)(fields.Get("sec").CastToNumber() ?? 0);
                value = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
            }
            return DynValue.NewNumber(new DateTimeOffset(value).ToUnixTimeSeconds());
        });
        os["date"] = DynValue.NewCallback((ctx, a) =>
        {
            string format = a.Count > 0 && a[0].Type == DataType.String ? a[0].String : "%c";
            double timestamp = a.Count > 1 ? (a[1].CastToNumber() ?? 0) : DateTimeOffset.Now.ToUnixTimeSeconds();
            bool utc = format.StartsWith("!");
            if (utc) format = format.Substring(1);
            DateTime value = DateTimeOffset.FromUnixTimeSeconds((long)timestamp).ToOffset(
                utc ? TimeSpan.Zero : TimeZoneInfo.Local.GetUtcOffset(DateTimeOffset.FromUnixTimeSeconds((long)timestamp))).DateTime;
            if (format == "*t")
            {
                var fields = new Table(ctx.GetScript());
                fields["year"] = DynValue.NewNumber(value.Year);
                fields["month"] = DynValue.NewNumber(value.Month);
                fields["day"] = DynValue.NewNumber(value.Day);
                fields["hour"] = DynValue.NewNumber(value.Hour);
                fields["min"] = DynValue.NewNumber(value.Minute);
                fields["sec"] = DynValue.NewNumber(value.Second);
                fields["wday"] = DynValue.NewNumber((int)value.DayOfWeek + 1);
                fields["yday"] = DynValue.NewNumber(value.DayOfYear);
                fields["isdst"] = DynValue.NewBoolean(!utc && TimeZoneInfo.Local.IsDaylightSavingTime(value));
                return DynValue.NewTable(fields);
            }
            return DynValue.NewString(FormatLuaDate(format, value));
        });
        math["noise"] = DynValue.NewCallback((ctx, a) =>
        {
            if (a.Count == 0) return DynValue.NewNumber(0);
            float x = N(a, 0) * 1.21f + 0.37f;
            float v;
            if (a.Count == 1)
                v = Mathf.PerlinNoise(x, 0.73f);
            else if (a.Count == 2)
                v = Mathf.PerlinNoise(x, N(a, 1) * 1.13f + 0.19f);
            else
            {
                float y = N(a, 1) * 1.13f + 0.19f;
                float z = N(a, 2) * 1.07f + 0.53f;
                float ab = Mathf.PerlinNoise(x + z * 0.31f, y - z * 0.17f);
                float bc = Mathf.PerlinNoise(y + z * 0.13f, z + x * 0.23f);
                float ca = Mathf.PerlinNoise(z + x * 0.19f, x - y * 0.11f);
                v = (ab + bc + ca) / 3f;
            }
            return DynValue.NewNumber(v - 0.5f);
        });
        var table = g.Get("table").Table;
        table["find"] = DynValue.NewCallback((ctx, a) =>
        {
            if (a.Count < 2 || a[0].Type != DataType.Table) return DynValue.Nil;
            Table t = a[0].Table;
            DynValue needle = a[1];
            int start = a.Count > 2 ? (int)(a[2].CastToNumber() ?? 1) : 1;
            for (int i = Math.Max(1, start); i <= t.Length; i++)
                if (SameLuaValue(t.Get(i), needle)) return DynValue.NewNumber(i);
            return DynValue.Nil;
        });
        table["clone"] = DynValue.NewCallback((ctx, a) =>
        {
            if (a.Count == 0 || a[0].Type != DataType.Table) return DynValue.Nil;
            var copy = new Table(ctx.GetScript());
            foreach (TablePair pair in a[0].Table.Pairs) copy.Set(pair.Key, pair.Value);
            return DynValue.NewTable(copy);
        });
        table["clear"] = DynValue.NewCallback((ctx, a) =>
        {
            if (a.Count > 0 && a[0].Type == DataType.Table) a[0].Table.Clear();
            return DynValue.Nil;
        });
        table["create"] = DynValue.NewCallback((ctx, a) =>
        {
            int count = Math.Max(0, (int)N(a, 0));
            var created = new Table(ctx.GetScript());
            if (a.Count > 1 && !a[1].IsNil())
                for (int i = 1; i <= count; i++) created.Set(i, a[1]);
            return DynValue.NewTable(created);
        });
        table["pack"] = DynValue.NewCallback((ctx, a) =>
        {
            var packed = new Table(ctx.GetScript());
            for (int i = 0; i < a.Count; i++) packed.Set(i + 1, a[i]);
            packed.Set("n", DynValue.NewNumber(a.Count));
            return DynValue.NewTable(packed);
        });
        var stringLib = g.Get("string").Table;
        stringLib["split"] = DynValue.NewCallback((ctx, a) =>
        {
            string s = a.Count > 0 ? (a[0].CastToString() ?? "") : "";
            string sep = a.Count > 1 ? (a[1].CastToString() ?? "") : ",";
            var t = new Table(ctx.GetScript());
            if (sep.Length == 0)
            {
                foreach (char c in s) t.Append(DynValue.NewString(c.ToString()));
            }
            else
            {
                foreach (string part in s.Split(new[] { sep }, StringSplitOptions.None))
                    t.Append(DynValue.NewString(part));
            }
            return DynValue.NewTable(t);
        });
        g["typeof"] = DynValue.NewCallback((ctx, a) =>
        {
            DynValue v = a.Count > 0 ? a[0] : DynValue.Nil;
            if (v.Type == DataType.UserData && v.UserData.Object != null)
            {
                object o = v.UserData.Object;
                if (o is Vec3) return DynValue.NewString("Vector3");
                if (o is Vec2) return DynValue.NewString("Vector2");
                if (o is Color3) return DynValue.NewString("Color3");
                if (o is LuaCFrame) return DynValue.NewString("CFrame");
                if (o is LuaScene) return DynValue.NewString("Scene");
                if (o is LuaTransform) return DynValue.NewString("Transform");
                if (o is LuaUnityComponent) return DynValue.NewString("UnityComponent");
                if (o is LuaRay) return DynValue.NewString("Ray");
                if (o is UDim2) return DynValue.NewString("UDim2");
                if (o is NumberRange) return DynValue.NewString("NumberRange");
                if (o is NumberSequence) return DynValue.NewString("NumberSequence");
                if (o is NumberSequenceKeypoint) return DynValue.NewString("NumberSequenceKeypoint");
                if (o is ColorSequence) return DynValue.NewString("ColorSequence");
                if (o is ColorSequenceKeypoint) return DynValue.NewString("ColorSequenceKeypoint");
                if (o is LuaInstance) return DynValue.NewString("GameObject");
                if (o is LuaGuiObject || o is LuaGuiLayout) return DynValue.NewString("Instance");
                if (o is LuaUnityComponentSection) return DynValue.NewString("UnityComponentSection");
                if (o is Signal) return DynValue.NewString("RBXScriptSignal");
                if (o is Connection) return DynValue.NewString("RBXScriptConnection");
                return DynValue.NewString(o.GetType().Name);
            }
            return DynValue.NewString(v.Type.ToLuaTypeString());
        });
        var waitFn = DynValue.NewCallback((ctx, a) =>
            DynValue.NewYieldReq(new[] { DynValue.NewNumber(a.Count > 0 ? (a[0].CastToNumber() ?? 0) : 0) }));
        g["wait"] = waitFn;
        var task = new Table(Lua);
        task["wait"]  = waitFn;
        task["spawn"] = DynValue.NewCallback((ctx, a) => DynValue.FromObject(Lua, Scheduler.Spawn(a[0], Rest(a, 1))));
        task["defer"] = DynValue.NewCallback((ctx, a) => DynValue.FromObject(Lua, Scheduler.Delay(0f, a[0], Rest(a, 1))));
        task["delay"] = DynValue.NewCallback((ctx, a) => DynValue.FromObject(Lua, Scheduler.Delay(N(a, 0), a[1], Rest(a, 2))));
        task["cancel"] = DynValue.NewCallback((ctx, a) =>
        {
            var h = a.Count > 0 && a[0].Type == DataType.UserData ? a[0].UserData.Object as TaskHandle : null;
            if (h != null) h.Cancelled = true;
            return DynValue.Nil;
        });
        g["task"] = task;
        g["require"] = DynValue.NewCallback((ctx, a) =>
        {
            string modName = a[0].CastToString();
            DynValue cached;
            if (moduleCache.TryGetValue(modName, out cached)) return cached;
            if (loading.Contains(modName))
                throw new ScriptRuntimeException("Circular require: " + modName);
            var asset = Resources.Load<TextAsset>("Lua/" + modName);
            if (asset == null)
                throw new ScriptRuntimeException("Module not found: Assets/Resources/Lua/" + modName + ".lua");
            string source = Luau.Transpile(asset.text);
            LuaErrors.Sources[modName] = source;
            var env = new Table(Lua);
            var meta = new Table(Lua);
            meta["__index"] = Lua.Globals;
            env.MetaTable = meta;
            loading.Add(modName);
            try
            {
                DynValue result = ctx.Call(Lua.LoadString(source, env, modName));
                if (result.Type == DataType.Tuple && result.Tuple.Length > 0) result = result.Tuple[0];
                if (result.IsNil()) result = DynValue.True;
                moduleCache[modName] = result;
                return result;
            }
            finally { loading.Remove(modName); }
        });
    }
    static string FormatLuaDate(string format, DateTime value)
    {
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < format.Length; i++)
        {
            if (format[i] != '%' || i + 1 >= format.Length) { result.Append(format[i]); continue; }
            char code = format[++i];
            switch (code)
            {
                case 'a': result.Append(value.ToString("ddd", System.Globalization.CultureInfo.InvariantCulture)); break;
                case 'A': result.Append(value.ToString("dddd", System.Globalization.CultureInfo.InvariantCulture)); break;
                case 'b': result.Append(value.ToString("MMM", System.Globalization.CultureInfo.InvariantCulture)); break;
                case 'B': result.Append(value.ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture)); break;
                case 'c': result.Append(value.ToString("ddd MMM dd HH:mm:ss yyyy", System.Globalization.CultureInfo.InvariantCulture)); break;
                case 'd': result.Append(value.ToString("dd")); break;
                case 'H': result.Append(value.ToString("HH")); break;
                case 'I': result.Append(value.ToString("hh")); break;
                case 'j': result.Append(value.DayOfYear.ToString("000")); break;
                case 'm': result.Append(value.ToString("MM")); break;
                case 'M': result.Append(value.ToString("mm")); break;
                case 'p': result.Append(value.ToString("tt", System.Globalization.CultureInfo.InvariantCulture)); break;
                case 'S': result.Append(value.ToString("ss")); break;
                case 'U': result.Append(((value.DayOfYear - 1 + 7 - (int)value.DayOfWeek) / 7).ToString("00")); break;
                case 'w': result.Append(((int)value.DayOfWeek).ToString()); break;
                case 'W': result.Append(((value.DayOfYear - 1 + 7 - (((int)value.DayOfWeek + 6) % 7)) / 7).ToString("00")); break;
                case 'x': result.Append(value.ToString("MM/dd/yy", System.Globalization.CultureInfo.InvariantCulture)); break;
                case 'X': result.Append(value.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)); break;
                case 'Y': result.Append(value.ToString("yyyy")); break;
                case 'y': result.Append(value.ToString("yy")); break;
                case '%': result.Append('%'); break;
                default: result.Append('%').Append(code); break;
            }
        }
        return result.ToString();
    }
    void SetupTypes(Table g)
    {
        var v2 = new Table(Lua);
        v2["new"] = DynValue.NewCallback((c, a) => DynValue.FromObject(Lua, new Vec2(N(a, 0), N(a, 1))));
        v2["zero"] = new Vec2(0, 0);
        v2["one"]  = new Vec2(1, 1);
        g["Vector2"] = v2;
        var v3 = new Table(Lua);
        v3["new"] = DynValue.NewCallback((c, a) =>
            DynValue.FromObject(Lua, new Vec3(N(a, 0), N(a, 1), N(a, 2))));
        v3["zero"]  = new Vec3(0, 0, 0);
        v3["one"]   = new Vec3(1, 1, 1);
        v3["xAxis"] = new Vec3(1, 0, 0);
        v3["yAxis"] = new Vec3(0, 1, 0);
        v3["zAxis"] = new Vec3(0, 0, 1);
        g["Vector3"] = v3;
        var c3 = new Table(Lua);
        c3["new"] = DynValue.NewCallback((c, a) =>
            DynValue.FromObject(Lua, new Color3(N(a, 0), N(a, 1), N(a, 2))));
        c3["fromRGB"] = DynValue.NewCallback((c, a) =>
            DynValue.FromObject(Lua, new Color3(N(a, 0) / 255f, N(a, 1) / 255f, N(a, 2) / 255f)));
        c3["fromHex"] = DynValue.NewCallback((c, a) =>
        {
            string hex = a.Count > 0 ? (a[0].CastToString() ?? "") : "";
            hex = hex.Trim();
            if (hex.StartsWith("#")) hex = hex.Substring(1);
            if (hex.Length == 3)
                hex = "" + hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2];
            Color col;
            if (ColorUtility.TryParseHtmlString("#" + hex, out col))
                return DynValue.FromObject(Lua, new Color3(col.r, col.g, col.b));
            return DynValue.FromObject(Lua, new Color3(1, 1, 1));
        });
        c3["fromHSV"] = DynValue.NewCallback((c, a) =>
        {
            Color col = Color.HSVToRGB(Mathf.Clamp01(N(a, 0)), Mathf.Clamp01(N(a, 1)), Mathf.Clamp01(N(a, 2)));
            return DynValue.FromObject(Lua, new Color3(col.r, col.g, col.b));
        });
        g["Color3"] = c3;
        var cf = new Table(Lua);
        cf["new"] = DynValue.NewCallback((c, a) =>
        {
            LuaCFrame r;
            if (a.Count == 0)
                r = new LuaCFrame(Vector3.zero, Quaternion.identity);
            else if (a[0].Type == DataType.UserData)
            {
                Vec3 p = (Vec3)a[0].UserData.Object;
                if (a.Count >= 2 && a[1].Type == DataType.UserData)
                    r = LuaCFrame.LookAt(p.ToUnity(), ((Vec3)a[1].UserData.Object).ToUnity());
                else
                    r = new LuaCFrame(p.ToUnity(), Quaternion.identity);
            }
            else
                r = new LuaCFrame(new Vector3(N(a, 0), N(a, 1), N(a, 2)), Quaternion.identity);
            return DynValue.FromObject(Lua, r);
        });
        cf["lookAt"] = DynValue.NewCallback((c, a) =>
            DynValue.FromObject(Lua, LuaCFrame.LookAt(((Vec3)a[0].UserData.Object).ToUnity(), ((Vec3)a[1].UserData.Object).ToUnity())));
        cf["Angles"] = DynValue.NewCallback((c, a) =>
            DynValue.FromObject(Lua, new LuaCFrame(Vector3.zero,
                Quaternion.Euler(N(a, 0) * Mathf.Rad2Deg, N(a, 1) * Mathf.Rad2Deg, N(a, 2) * Mathf.Rad2Deg))));
        cf["fromEulerAnglesXYZ"] = DynValue.NewCallback((c, a) =>
            DynValue.FromObject(Lua, new LuaCFrame(Vector3.zero,
                Quaternion.Euler(N(a, 0) * Mathf.Rad2Deg, N(a, 1) * Mathf.Rad2Deg, N(a, 2) * Mathf.Rad2Deg))));
        cf["fromOrientation"] = DynValue.NewCallback((c, a) =>
            DynValue.FromObject(Lua, new LuaCFrame(Vector3.zero,
                Quaternion.Euler(N(a, 0) * Mathf.Rad2Deg, N(a, 1) * Mathf.Rad2Deg, N(a, 2) * Mathf.Rad2Deg))));
        cf["fromAxisAngle"] = DynValue.NewCallback((c, a) =>
        {
            Vec3 axis = a.Count > 0 && a[0].Type == DataType.UserData ? a[0].UserData.Object as Vec3 : null;
            Vector3 v = axis != null ? axis.ToUnity() : Vector3.up;
            return DynValue.FromObject(Lua, new LuaCFrame(Vector3.zero,
                Quaternion.AngleAxis(N(a, 1) * Mathf.Rad2Deg, v.sqrMagnitude > 0.0001f ? v.normalized : Vector3.up)));
        });
        g["CFrame"] = cf;
        var ud = new Table(Lua);
        ud["new"] = DynValue.NewCallback((c, a) =>
            DynValue.FromObject(Lua, new UDim2(N(a, 0), N(a, 1), N(a, 2), N(a, 3))));
        ud["fromScale"] = DynValue.NewCallback((c, a) =>
            DynValue.FromObject(Lua, new UDim2(N(a, 0), 0, N(a, 1), 0)));
        ud["fromOffset"] = DynValue.NewCallback((c, a) =>
            DynValue.FromObject(Lua, new UDim2(0, N(a, 0), 0, N(a, 1))));
        g["UDim2"] = ud;
        var numberRange = new Table(Lua);
        numberRange["new"] = DynValue.NewCallback((c, a) =>
            DynValue.FromObject(Lua, a.Count >= 2
                ? new NumberRange(N(a, 0), N(a, 1))
                : new NumberRange(N(a, 0), N(a, 0))));
        g["NumberRange"] = numberRange;
        var numberKeypoint = new Table(Lua);
        numberKeypoint["new"] = DynValue.NewCallback((c, a) =>
            DynValue.FromObject(Lua, new NumberSequenceKeypoint(N(a, 0), N(a, 1), N(a, 2))));
        g["NumberSequenceKeypoint"] = numberKeypoint;
        var numberSequence = new Table(Lua);
        numberSequence["new"] = DynValue.NewCallback((c, a) =>
        {
            if (a.Count == 0) return DynValue.FromObject(Lua, new NumberSequence(0f));
            if (a.Count >= 2 && a[0].Type == DataType.Number && a[1].Type == DataType.Number)
                return DynValue.FromObject(Lua, new NumberSequence(N(a, 0), N(a, 1)));
            if (a[0].Type == DataType.Table)
                return DynValue.FromObject(Lua, new NumberSequence(a[0].Table));
            if (a[0].Type == DataType.UserData && a[0].ToObject() is NumberSequenceKeypoint)
            {
                var keypoints = new Table(Lua);
                keypoints.Append(a[0]);
                return DynValue.FromObject(Lua, new NumberSequence(keypoints));
            }
            return DynValue.FromObject(Lua, new NumberSequence(N(a, 0)));
        });
        g["NumberSequence"] = numberSequence;
        var colorKeypoint = new Table(Lua);
        colorKeypoint["new"] = DynValue.NewCallback((c, a) =>
        {
            var color = a.Count > 1 ? a[1].ToObject() as Color3 : null;
            return DynValue.FromObject(Lua,
                new ColorSequenceKeypoint(N(a, 0), color ?? new Color3(1, 1, 1)));
        });
        g["ColorSequenceKeypoint"] = colorKeypoint;
        var colorSequence = new Table(Lua);
        colorSequence["new"] = DynValue.NewCallback((c, a) =>
        {
            if (a.Count == 0) return DynValue.FromObject(Lua, new ColorSequence(new Color3(1, 1, 1)));
            if (a[0].Type == DataType.Table)
                return DynValue.FromObject(Lua, new ColorSequence(a[0].Table));
            if (a.Count >= 2 && a[0].Type == DataType.UserData && a[1].Type == DataType.UserData)
            {
                var first = a[0].ToObject() as Color3;
                var second = a[1].ToObject() as Color3;
                if (first != null && second != null)
                    return DynValue.FromObject(Lua, new ColorSequence(first, second));
            }
            if (a[0].Type == DataType.UserData && a[0].ToObject() is ColorSequenceKeypoint)
            {
                var keypoints = new Table(Lua);
                keypoints.Append(a[0]);
                return DynValue.FromObject(Lua, new ColorSequence(keypoints));
            }
            var single = a[0].ToObject() as Color3;
            return DynValue.FromObject(Lua, new ColorSequence(single ?? new Color3(1, 1, 1)));
        });
        g["ColorSequence"] = colorSequence;
        var ray = new Table(Lua);
        ray["new"] = DynValue.NewCallback((c, a) =>
            DynValue.FromObject(Lua, new LuaRay(((Vec3)a[0].UserData.Object).ToUnity(), ((Vec3)a[1].UserData.Object).ToUnity())));
        g["Ray"] = ray;
        var rp = new Table(Lua);
        rp["new"] = DynValue.NewCallback((c, a) => DynValue.FromObject(Lua, new LuaRaycastParams()));
        g["RaycastParams"] = rp;
        var rnd = new Table(Lua);
        rnd["new"] = DynValue.NewCallback((c, a) =>
            DynValue.FromObject(Lua, a.Count > 0 ? new LuaRandom((int)(a[0].CastToNumber() ?? 0)) : new LuaRandom()));
        g["Random"] = rnd;
        var gameObject = new Table(Lua);
        gameObject["new"] = DynValue.NewCallback((c, a) =>
        {
            string cls = a.Count > 0 ? a[0].CastToString() : null;
            object parent = a.Count > 1 ? a[1].ToObject() : null;
            return DynValue.FromObject(Lua, LuaFactory.Create(cls, parent));
        });
        g["GameObject"] = gameObject;
        var keyCode = new Table(Lua);
        foreach (var keyName in Enum.GetNames(typeof(Key))) keyCode[keyName] = keyName;
        foreach (var padName in LuaUserInputService.PadButtonNames) keyCode[padName] = padName;
        var enumTable = new Table(Lua);
        enumTable["KeyCode"] = keyCode;
        enumTable["UserInputType"] = MakeEnum(Lua, "Keyboard", "MouseButton1", "MouseButton2", "Touch", "Gamepad1");
        enumTable["MouseBehavior"] = MakeEnum(Lua, "Default", "LockCenter");
        enumTable["UserInputState"] = MakeEnum(Lua, "Begin", "Change", "End");
        enumTable["ContextActionResult"] = MakeEnum(Lua, "Pass", "Sink");
        g["Enum"] = enumTable;
    }
    void SetupServices(LuaGame game)
    {
        Func<string, DynValue> findPrefab = n =>
        {
            var prefab = string.IsNullOrEmpty(n) ? null : Resources.Load<GameObject>("Prefabs/" + n);
            return prefab != null ? DynValue.FromObject(Lua, new LuaPrefab(prefab)) : DynValue.Nil;
        };
        var findFn = DynValue.NewCallback((ctx, a) => findPrefab(a[1].CastToString()));
        var rs = new Table(Lua);
        rs["FindFirstChild"] = findFn;
        rs["WaitForChild"] = findFn;
        rs["GetChildren"] = DynValue.NewCallback((ctx, a) =>
        {
            var t = new Table(Lua);
            foreach (var p in Resources.LoadAll<GameObject>("Prefabs"))
                t.Append(DynValue.FromObject(Lua, new LuaPrefab(p)));
            return DynValue.NewTable(t);
        });
        var rsMeta = new Table(Lua);
        rsMeta["__index"] = DynValue.NewCallback((ctx, a) => findPrefab(a[1].CastToString()));
        rs.MetaTable = rsMeta;
        game.RegisterService("ReplicatedStorage", DynValue.NewTable(rs));
        game.RegisterService("ServerStorage", DynValue.NewTable(rs));
        game.RegisterService("Lighting", DynValue.FromObject(Lua, new LuaLighting()));
        game.RegisterService("SoundService", DynValue.FromObject(Lua, new LuaSoundService()));
        game.RegisterService("DataStoreService", DynValue.FromObject(Lua, new LuaDataStoreService()));
        game.RegisterService("PhysicsService", DynValue.FromObject(Lua, PhysicsService));
        game.RegisterService("ContextActionService", DynValue.FromObject(Lua, ContextActionService));
        var gui = DynValue.FromObject(Lua, new LuaGuiRoot());
        game.RegisterService("CoreGui", gui);
        game.RegisterService("StarterGui", gui);
        var services = Lua.DoString(@"
        local RunService = game:GetService('RunService')
        local function bounceOut(a)
            local n1, d1 = 7.5625, 2.75
            if a < 1 / d1 then
                return n1 * a * a
            elseif a < 2 / d1 then
                a = a - 1.5 / d1
                return n1 * a * a + 0.75
            elseif a < 2.5 / d1 then
                a = a - 2.25 / d1
                return n1 * a * a + 0.9375
            end
            a = a - 2.625 / d1
            return n1 * a * a + 0.984375
        end
        local easeIn = {
            Linear = function(a) return a end,
            Sine = function(a) return 1 - math.cos(a * math.pi / 2) end,
            Quad = function(a) return a * a end,
            Cubic = function(a) return a * a * a end,
            Back = function(a)
                local c1 = 1.70158
                return (c1 + 1) * a * a * a - c1 * a * a
            end,
            Elastic = function(a)
                if a == 0 or a == 1 then return a end
                return -(2 ^ (10 * a - 10)) * math.sin((a * 10 - 10.75) * (2 * math.pi / 3))
            end,
            Bounce = function(a) return 1 - bounceOut(1 - a) end,
        }
        local function ease(style, dir, a)
            local f = easeIn[style] or easeIn.Linear
            if dir == 'In' then return f(a) end
            if dir == 'Out' then return 1 - f(1 - a) end
            if a < 0.5 then return f(a * 2) / 2 end
            return 1 - f((1 - a) * 2) / 2
        end
        local function lerp(prop, a, b, t)
            if type(a) == 'number' and type(b) == 'number' then
                return a + (b - a) * t
            end
            if type(a) == 'table' or type(a) == 'userdata' then
                if a.Lerp then return a:Lerp(b, t) end
            end
            return b
        end
        Enum.EasingStyle = { Linear = 'Linear', Sine = 'Sine', Quad = 'Quad', Cubic = 'Cubic', Back = 'Back', Elastic = 'Elastic', Bounce = 'Bounce' }
        Enum.EasingDirection = { In = 'In', Out = 'Out', InOut = 'InOut' }
        TweenInfo = {
            new = function(time, style, direction, repeatCount, reverses, delayTime)
                return {
                    Time = time or 1,
                    EasingStyle = style or 'Quad',
                    EasingDirection = direction or 'Out',
                    RepeatCount = repeatCount or 0,
                    Reverses = reverses or false,
                    DelayTime = delayTime or 0,
                }
            end
        }
        local TweenService = {}
        function TweenService:GetValue(alpha, style, direction)
            return ease(style or 'Quad', direction or 'Out', math.clamp(alpha or 0, 0, 1))
        end
        function TweenService:Create(inst, info, goals)
            local tween = { PlaybackState = 'Begin' }
            local done = GameObject.new('BindableEvent')
            tween.Completed = done.Event
            local conn, from, to, origin
            local t, cycle, back = 0, 0, false
            local function current()
                local c = {}
                for prop in pairs(goals) do c[prop] = inst[prop] end
                return c
            end
            local function finish(state)
                if conn then
                    conn:Disconnect()
                    conn = nil
                end
                tween.PlaybackState = state
                done:Fire(state)
            end
            local function start(fromValues, toValues, startTime)
                from = fromValues
                to = toValues
                t = startTime
                cycle = 0
                back = false
                tween.PlaybackState = 'Playing'
                if conn then return end
                conn = RunService.Heartbeat:Connect(function(dt)
                    if tween.PlaybackState ~= 'Playing' then return end
                    t = t + dt
                    if t < 0 then return end
                    local a = math.min(t / math.max(info.Time, 0.0001), 1)
                    local e = ease(info.EasingStyle, info.EasingDirection, a)
                    for prop, goal in pairs(to) do
                        inst[prop] = lerp(prop, from[prop], goal, e)
                    end
                    if a >= 1 then
                        if info.Reverses and not back then
                            from, to = to, from
                            back = true
                            t = 0
                            return
                        end
                        if back then
                            from, to = to, from
                            back = false
                        end
                        cycle = cycle + 1
                        if info.RepeatCount < 0 or cycle <= info.RepeatCount then
                            t = 0
                        else
                            finish('Completed')
                        end
                    end
                end)
            end
            function tween:Play()
                if tween.PlaybackState == 'Playing' then return end
                if tween.PlaybackState == 'Paused' then
                    tween.PlaybackState = 'Playing'
                    return
                end
                origin = current()
                start(origin, goals, -info.DelayTime)
            end
            function tween:Pause()
                if tween.PlaybackState == 'Playing' then tween.PlaybackState = 'Paused' end
            end
            function tween:Cancel()
                if tween.PlaybackState == 'Playing' or tween.PlaybackState == 'Paused' then
                    finish('Cancelled')
                end
            end
            function tween:Reverse()
                local now = current()
                if conn then
                    conn:Disconnect()
                    conn = nil
                end
                start(now, origin or now, 0)
            end
            return tween
        end
        return { TweenService = TweenService }
        ");
        game.RegisterService("TweenService", services.Table.Get("TweenService"));
    }
    public void Tick(float dt)
    {
        Scheduler.Tick(dt);
        PollInput();
        stepped.Fire(dt);
        renderStepped.Fire(dt);
        heartbeat.Fire(dt);
    }
    public void FixedTick(float dt) { fixedUpdate.Fire(dt); }
    public void LateTick(float dt) { lateUpdate.Fire(dt); }
    void PollInput()
    {
        var kb = Keyboard.current;
        if (kb != null)
        {
            foreach (var key in kb.allKeys)
            {
                if (key.wasPressedThisFrame)  inputBegan.Fire(new LuaInput(key.keyCode.ToString(), "Keyboard"), false);
                if (key.wasReleasedThisFrame) inputEnded.Fire(new LuaInput(key.keyCode.ToString(), "Keyboard"), false);
            }
        }
        var mouse = Mouse.current;
        if (mouse != null)
        {
            Vector2 mp = mouse.position.ReadValue();
            bool overGui = LuaGui.IsOverGui(new Vector2(mp.x, Screen.height - mp.y));
            FireMouse(mouse.leftButton, "MouseButton1", overGui);
            FireMouse(mouse.rightButton, "MouseButton2", overGui);
        }
        var pad = Gamepad.current;
        if (pad != null)
        {
            foreach (var padName in LuaUserInputService.PadButtonNames)
            {
                var b = LuaUserInputService.PadButton(pad, padName);
                if (b == null) continue;
                if (b.wasPressedThisFrame)  inputBegan.Fire(new LuaInput(padName, "Gamepad1"), false);
                if (b.wasReleasedThisFrame) inputEnded.Fire(new LuaInput(padName, "Gamepad1"), false);
            }
        }
        var ts = Touchscreen.current;
        if (ts != null)
        {
            var pt = ts.primaryTouch;
            Vector2 tp = pt.position.ReadValue();
            var pos = new Vec2(tp.x, Screen.height - tp.y);
            if (pt.press.wasPressedThisFrame)
            {
                touchStarted.Fire(pos);
                inputBegan.Fire(new LuaInput("None", "Touch"), false);
            }
            else if (pt.press.wasReleasedThisFrame)
            {
                touchEnded.Fire(pos);
                inputEnded.Fire(new LuaInput("None", "Touch"), false);
            }
            else if (pt.press.isPressed && pt.delta.ReadValue().sqrMagnitude > 0f)
            {
                touchMoved.Fire(pos);
            }
        }
    }
    void FireMouse(ButtonControl b, string type, bool overGui)
    {
        if (b.wasPressedThisFrame)  inputBegan.Fire(new LuaInput("None", type), overGui);
        if (b.wasReleasedThisFrame) inputEnded.Fire(new LuaInput("None", type), false);
    }
}
