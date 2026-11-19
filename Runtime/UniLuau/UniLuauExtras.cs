using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Serialization.Json;
public static class LuaAudio
{
    public static AudioClip LoadClip(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        string path = id.Contains("/") ? id : "Sounds/" + id;
        var clip = Resources.Load<AudioClip>(path);
        if (clip == null) Debug.LogWarning("Sound not found: Assets/Resources/" + path);
        return clip;
    }
}
public static class LuaUnityTypes
{
    static readonly Dictionary<string, Type> typeCache = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
    static readonly Dictionary<string, Type> componentCache = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
    static List<Type> componentTypes;
    static readonly Dictionary<string, string> aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "text", "UnityEngine.UI.Text" },
        { "legacytext", "UnityEngine.UI.Text" },
        { "textmeshpro", "TMPro.TextMeshPro" },
        { "tmp", "TMPro.TextMeshPro" },
        { "textmeshprotext", "TMPro.TextMeshPro" },
        { "texttextmeshpro", "TMPro.TextMeshProUGUI" },
        { "textmeshproui", "TMPro.TextMeshProUGUI" },
        { "tmpui", "TMPro.TextMeshProUGUI" },
        { "textmeshprotextui", "TMPro.TextMeshProUGUI" },
        { "textmeshprougui", "TMPro.TextMeshProUGUI" },
        { "textmeshproinputfield", "TMPro.TMP_InputField" },
        { "inputfieldtextmeshpro", "TMPro.TMP_InputField" },
        { "tmpinputfield", "TMPro.TMP_InputField" },
        { "dropdowntextmeshpro", "TMPro.TMP_Dropdown" },
        { "textmeshprodropdown", "TMPro.TMP_Dropdown" },
        { "rawimage", "UnityEngine.UI.RawImage" },
        { "button", "UnityEngine.UI.Button" },
        { "image", "UnityEngine.UI.Image" },
        { "panel", "UnityEngine.UI.Image" },
        { "slider", "UnityEngine.UI.Slider" },
        { "toggle", "UnityEngine.UI.Toggle" },
        { "togglegroup", "UnityEngine.UI.ToggleGroup" },
        { "scrollbar", "UnityEngine.UI.Scrollbar" },
        { "scrollrect", "UnityEngine.UI.ScrollRect" },
        { "inputfield", "UnityEngine.UI.InputField" },
        { "legacydropdown", "UnityEngine.UI.Dropdown" },
        { "canvas", "UnityEngine.Canvas" },
        { "cameras", "UnityEngine.Camera" },
        { "camera", "UnityEngine.Camera" },
        { "renderer", "UnityEngine.MeshRenderer" },
        { "meshrenderer", "UnityEngine.MeshRenderer" },
        { "meshfilter", "UnityEngine.MeshFilter" },
        { "light", "UnityEngine.Light" },
        { "audiosource", "UnityEngine.AudioSource" },
        { "rigidbody", "UnityEngine.Rigidbody" },
        { "boxcollider", "UnityEngine.BoxCollider" },
        { "spherecollider", "UnityEngine.SphereCollider" },
        { "capsulecollider", "UnityEngine.CapsuleCollider" },
        { "meshcollider", "UnityEngine.MeshCollider" },
        { "charactercontroller", "UnityEngine.CharacterController" },
        { "particlesystem", "UnityEngine.ParticleSystem" },
        { "animator", "UnityEngine.Animator" },
    };
    static IEnumerable<System.Reflection.Assembly> LoadedAssemblies
    {
        get { return UnityEngine.Assemblies.CurrentAssemblies.GetLoadedAssemblies(); }
    }
    public static void ClearCache()
    {
        typeCache.Clear();
        componentCache.Clear();
        componentTypes = null;
    }
    public static Type FindType(string fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return null;
        Type cached;
        if (typeCache.TryGetValue(fullName, out cached)) return cached;
        Type found = null;
        foreach (var assembly in LoadedAssemblies)
        {
            found = assembly.GetType(fullName, false);
            if (found != null) break;
        }
        typeCache[fullName] = found;
        return found;
    }
    static List<Type> ComponentTypes
    {
        get
        {
            if (componentTypes != null) return componentTypes;
            componentTypes = new List<Type>();
            foreach (var assembly in LoadedAssemblies)
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException e) { types = e.Types; }
                catch { continue; }
                foreach (var type in types)
                {
                    if (type == null || !type.IsClass || type.IsAbstract || type.IsGenericTypeDefinition) continue;
                    if (!typeof(Component).IsAssignableFrom(type)) continue;
                    if (type == typeof(Transform)) continue;
                    componentTypes.Add(type);
                }
            }
            return componentTypes;
        }
    }
    public static Type ResolveComponent(string className)
    {
        if (string.IsNullOrWhiteSpace(className)) return null;
        string key = Normalize(className);
        Type cached;
        if (componentCache.TryGetValue(key, out cached)) return cached;
        Type result = null;
        string aliasTarget;
        if (aliases.TryGetValue(key, out aliasTarget))
        {
            result = FindType(aliasTarget);
            if (result == null)
            {
                int dot = aliasTarget.LastIndexOf('.');
                string aliasShortName = Normalize(dot >= 0 ? aliasTarget.Substring(dot + 1) : aliasTarget);
                foreach (var type in ComponentTypes)
                {
                    if (Normalize(type.Name) == aliasShortName)
                    {
                        result = type;
                        break;
                    }
                }
            }
        }
        if (result == null)
        {
            result = FindType(className);
            if (result == null || !typeof(Component).IsAssignableFrom(result))
            {
                result = null;
                foreach (var type in ComponentTypes)
                {
                    if (Normalize(type.Name) == key || Normalize(type.FullName) == key)
                    {
                        result = type;
                        break;
                    }
                }
            }
        }
        if (result == null)
        {
            foreach (var type in ComponentTypes)
            {
                AddComponentMenu menu;
                try { menu = type.GetCustomAttribute<AddComponentMenu>(false); }
                catch { continue; }
                if (menu == null || string.IsNullOrEmpty(menu.componentMenu)) continue;
                string menuLabel = menu.componentMenu;
                int slash = menuLabel.LastIndexOf('/');
                string shortLabel = slash >= 0 ? menuLabel.Substring(slash + 1) : menuLabel;
                if (Normalize(menuLabel) == key || Normalize(shortLabel) == key)
                {
                    result = type;
                    break;
                }
            }
        }
        if (result != null && (!typeof(Component).IsAssignableFrom(result) || result.IsAbstract))
            result = null;
        componentCache[key] = result;
        return result;
    }
    public static bool IsUiComponent(Type type)
    {
        if (type == null) return false;
        if (type == typeof(RectTransform) || type == typeof(Canvas) || type == typeof(CanvasRenderer)) return true;
        if (typeof(UnityEngine.EventSystems.UIBehaviour).IsAssignableFrom(type)) return true;
        if (typeof(Graphic).IsAssignableFrom(type)) return true;
        return false;
    }
    public static string Normalize(string name)
    {
        if (string.IsNullOrEmpty(name)) return "";
        var chars = new List<char>();
        foreach (char c in name)
            if (char.IsLetterOrDigit(c)) chars.Add(char.ToLowerInvariant(c));
        return new string(chars.ToArray());
    }
}
public static class LuaPrefabObjects
{
    [MoonSharpHidden]
    public static GameObject Create(string className, bool ui = false)
    {
        GameObject prefab = string.IsNullOrEmpty(className) ? null : Resources.Load<GameObject>("Prefabs/" + className);
        GameObject instance = prefab != null
            ? UnityEngine.Object.Instantiate(prefab)
            : ui
                ? new GameObject(className, typeof(RectTransform), typeof(CanvasRenderer))
                : new GameObject(className);
        instance.name = className;
        instance.SetActive(false);
        return instance;
    }
    [MoonSharpHidden]
    public static void SetParent(GameObject instance, object parent)
    {
        if (instance == null) return;
        var worldObject = parent as LuaInstance;
        var scene = parent as LuaScene;
        if (worldObject != null && worldObject.Go != null)
        {
            instance.transform.SetParent(worldObject.Go.transform, false);
            instance.SetActive(true);
        }
        else if (scene != null)
        {
            instance.transform.SetParent(null, true);
            if (scene.UnityScene.IsValid())
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(instance, scene.UnityScene);
            instance.SetActive(true);
        }
        else
        {
            instance.transform.SetParent(null, false);
            instance.SetActive(false);
        }
    }
}
[MoonSharpUserData]
public class LuaMaterial
{
    static readonly string[] ColorProperties = { "_BaseColor", "_Color", "_FaceColor", "_TintColor" };
    readonly Material material;
    [MoonSharpHidden]
    public LuaMaterial(Material material) { this.material = material; }
    [MoonSharpHidden]
    public Material UnityMaterial { get { return material; } }
    public string Name
    {
        get { return material != null ? material.name : ""; }
        set { if (material != null) material.name = value ?? ""; }
    }
    public string Shader
    {
        get { return material != null && material.shader != null ? material.shader.name : ""; }
    }
    public Color3 Color
    {
        get
        {
            if (material == null) return new Color3(1, 1, 1);
            foreach (string property in ColorProperties)
            {
                if (!material.HasProperty(property)) continue;
                Color c = material.GetColor(property);
                return new Color3(c.r, c.g, c.b);
            }
            return new Color3(1, 1, 1);
        }
        set
        {
            if (material == null || value == null) return;
            foreach (string property in ColorProperties)
            {
                if (!material.HasProperty(property)) continue;
                Color c = value.ToUnity();
                c.a = material.GetColor(property).a;
                material.SetColor(property, c);
                return;
            }
        }
    }
    public Color3 MainColor
    {
        get { return Color; }
        set { Color = value; }
    }
    public bool HasProperty(string property)
    {
        return material != null && !string.IsNullOrEmpty(property) && material.HasProperty(property);
    }
    public Color3 GetColor(string property)
    {
        if (material == null || string.IsNullOrEmpty(property) || !material.HasProperty(property))
            return new Color3(1, 1, 1);
        Color c = material.GetColor(property);
        return new Color3(c.r, c.g, c.b);
    }
    public void SetColor(string property, Color3 value)
    {
        if (material == null || value == null || string.IsNullOrEmpty(property)) return;
        if (!material.HasProperty(property)) return;
        Color c = value.ToUnity();
        c.a = material.GetColor(property).a;
        material.SetColor(property, c);
    }
    public float GetFloat(string property)
    {
        return material != null && !string.IsNullOrEmpty(property) && material.HasProperty(property)
            ? material.GetFloat(property)
            : 0f;
    }
    public void SetFloat(string property, float value)
    {
        if (material != null && !string.IsNullOrEmpty(property) && material.HasProperty(property))
            material.SetFloat(property, value);
    }
    public override string ToString()
    {
        return material != null ? material.name + " (" + Shader + ")" : "No material";
    }
}
public static class LuaFactory
{
    public static object Create(string cls, object parent = null)
    {
        if (string.IsNullOrWhiteSpace(cls))
            throw new ScriptRuntimeException(
                "GameObject.new needs a class name, for example GameObject.new('Cube') " +
                "or GameObject.new('Text - TextMeshPro', parent).");
        object created;
        switch (cls)
        {
            case "BindableEvent":    created = new LuaBindableEvent(LuaWorld.Instance.Scheduler); break;
            case "BindableFunction": created = new LuaBindableFunction(); break;
            case "RemoteEvent":      created = new LuaRemoteEvent(LuaWorld.Instance.Scheduler, LuaWorld.Instance.Network); break;
            case "ScreenGui":
            case "Frame":
            case "ScrollingFrame":
            case "TextLabel":
            case "TextButton":
            case "ImageLabel":
            case "ImageButton":
            case "TextBox":          created = new LuaGuiObject(cls); break;
            case "UIListLayout":
            case "UIGridLayout":
            case "LayoutElement":    created = new LuaGuiLayout(cls); break;
            default:                 created = CreateUnityObject(cls); break;
        }
        ApplyParent(created, parent);
        return created;
    }
    static void ApplyParent(object created, object parent)
    {
        if (created == null || parent == null) return;
        var instance = created as LuaInstance;
        if (instance != null) { instance.Parent = parent; return; }
        var gui = created as LuaGuiObject;
        if (gui != null) { gui.Parent = parent; return; }
        var layout = created as LuaGuiLayout;
        if (layout != null) { layout.Parent = parent as LuaGuiObject; return; }
        var bindable = created as LuaBindableEvent;
        if (bindable != null) { bindable.Parent = parent; return; }
        var bindableFunction = created as LuaBindableFunction;
        if (bindableFunction != null) { bindableFunction.Parent = parent; return; }
        var remote = created as LuaRemoteEvent;
        if (remote != null) { remote.Parent = parent; return; }
    }
    static object CreateUnityObject(string cls)
    {
        var prefab = Resources.Load<GameObject>("Prefabs/" + cls);
        if (prefab != null)
        {
            return LuaInstance.FromPrefab(prefab, cls);
        }
        if (string.Equals(cls, "GameObject", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(cls, "Empty", StringComparison.OrdinalIgnoreCase))
        {
            var empty = new GameObject(cls);
            empty.SetActive(true);
            return LuaInstance.WrapAs(empty, cls);
        }
        PrimitiveType primitive;
        if (TryGetPrimitive(cls, out primitive))
        {
            var go = GameObject.CreatePrimitive(primitive);
            go.name = cls;
            go.SetActive(true);
            return LuaInstance.WrapAs(go, cls);
        }
        Type type = LuaUnityTypes.ResolveComponent(cls);
        if (type == null)
            throw new ScriptRuntimeException(
                "GameObject.new('" + cls + "') is not a Unity class. Use a Unity component name " +
                "(for example 'Rigidbody' or 'Text - TextMeshPro'), a primitive " +
                "('Cube', 'Sphere', 'Capsule', 'Cylinder', 'Plane', 'Quad'), a UI class, " +
                "or add a prefab at Assets/Resources/Prefabs/" + cls + ".prefab.");
        if (type == typeof(RectTransform))
        {
            var rectGo = new GameObject(cls, typeof(RectTransform));
            rectGo.SetActive(true);
            return LuaInstance.WrapAs(rectGo, cls);
        }
        GameObject host = LuaUnityTypes.IsUiComponent(type)
            ? new GameObject(cls, typeof(RectTransform))
            : new GameObject(cls);
        Component component;
        try { component = host.AddComponent(type); }
        catch (Exception e)
        {
            UnityEngine.Object.Destroy(host);
            throw new ScriptRuntimeException(
                "Cannot add Unity component '" + cls + "' (" + type.FullName + "): " + e.Message);
        }
        if (component == null)
        {
            UnityEngine.Object.Destroy(host);
            throw new ScriptRuntimeException(
                "Unity did not add component '" + cls + "' (" + type.FullName + ").");
        }
        if (type == typeof(Canvas))
        {
            if (host.GetComponent<CanvasScaler>() == null) host.AddComponent<CanvasScaler>();
            if (host.GetComponent<GraphicRaycaster>() == null) host.AddComponent<GraphicRaycaster>();
        }
        host.SetActive(true);
        return LuaInstance.WrapAs(host, cls);
    }
    static bool TryGetPrimitive(string cls, out PrimitiveType primitive)
    {
        switch ((cls ?? "").Trim().ToLowerInvariant())
        {
            case "cube":     primitive = PrimitiveType.Cube;     return true;
            case "sphere":   primitive = PrimitiveType.Sphere;   return true;
            case "capsule":  primitive = PrimitiveType.Capsule;  return true;
            case "cylinder": primitive = PrimitiveType.Cylinder; return true;
            case "plane":    primitive = PrimitiveType.Plane;    return true;
            case "quad":     primitive = PrimitiveType.Quad;     return true;
            default:         primitive = PrimitiveType.Cube;     return false;
        }
    }
}
[MoonSharpUserData]
public class LuaBindableFunction
{
    readonly GameObject go;
    object parent;
    public DynValue OnInvoke { get; set; }
    [MoonSharpHidden]
    public LuaBindableFunction()
    {
        go = LuaPrefabObjects.Create("BindableFunction");
        LuaWorld.Instance.Scheduler.TrackCleanup(Destroy);
    }
    public object Parent
    {
        get { return parent; }
        set { parent = value; LuaPrefabObjects.SetParent(go, value); }
    }
    public string Name
    {
        get { return go != null ? go.name : "BindableFunction"; }
        set { if (go != null) go.name = value ?? "BindableFunction"; }
    }
    public void Destroy()
    {
        parent = null;
        if (go != null) UnityEngine.Object.Destroy(go);
    }
    public DynValue Invoke(params object[] args)
    {
        if (OnInvoke == null || OnInvoke.Type != DataType.Function) return DynValue.Nil;
        var lua = LuaWorld.Instance.Lua;
        var dyn = new DynValue[args.Length];
        for (int i = 0; i < args.Length; i++) dyn[i] = DynValue.FromObject(lua, args[i]);
        DynValue result = lua.Call(OnInvoke, dyn);
        if (result.Type == DataType.Tuple && result.Tuple.Length > 0) result = result.Tuple[0];
        return result;
    }
}
[MoonSharpUserData]
public class LuaRemoteEvent
{
    public Signal OnServerEvent, OnClientEvent;
    readonly GameObject go;
    object parent;
    readonly LuaNetworkService network;
    [MoonSharpHidden]
    public LuaRemoteEvent(Scheduler s, LuaNetworkService network = null)
    {
        this.network = network;
        OnServerEvent = new Signal(s);
        OnClientEvent = new Signal(s);
        go = LuaPrefabObjects.Create("RemoteEvent");
        if (network != null) network.Register(this);
        s.TrackCleanup(Destroy);
    }
    public object Parent
    {
        get { return parent; }
        set { parent = value; LuaPrefabObjects.SetParent(go, value); }
    }
    public string Name
    {
        get { return go != null ? go.name : "RemoteEvent"; }
        set
        {
            if (network != null) network.Unregister(this);
            if (go != null) go.name = value ?? "RemoteEvent";
            if (network != null) network.Register(this);
        }
    }
    public void Destroy()
    {
        parent = null;
        if (network != null) network.Unregister(this);
        if (go != null) UnityEngine.Object.Destroy(go);
    }
    public void FireServer(params object[] args)
    {
        if (network != null && network.IsConnected) network.SendServer(Name, args);
        else OnServerEvent.Fire(args);
    }
    public void FireClient(params object[] args)
    {
        if (network != null && network.IsConnected)
            throw new ScriptRuntimeException("FireClient requires a target client ID. Use FireClientTo(clientId, ...)." );
        else OnClientEvent.Fire(args);
    }
    public void FireClientTo(ulong clientId, params object[] args)
    {
        if (network != null && network.IsConnected) network.SendClient(Name, clientId, args);
        else OnClientEvent.Fire(args);
    }
    public void FireAllClients(params object[] args)
    {
        if (network != null && network.IsConnected) network.SendAllClients(Name, args);
        else OnClientEvent.Fire(args);
    }
}
[MoonSharpUserData]
public class LuaPrefab
{
    public string Name;
    readonly GameObject prefab;
    [MoonSharpHidden] public LuaPrefab(GameObject p) { prefab = p; Name = p.name; }
    public LuaInstance Clone()
    {
        return LuaInstance.FromPrefab(prefab);
    }
}
[MoonSharpUserData]
public class LuaLighting
{
    float clockTime = 14f;
    static Light Sun()
    {
        if (RenderSettings.sun != null) return RenderSettings.sun;
        foreach (var l in UnityEngine.Object.FindObjectsByType<Light>())
            if (l.type == LightType.Directional) return l;
        return null;
    }
    public Color3 Ambient
    {
        get { var c = RenderSettings.ambientLight; return new Color3(c.r, c.g, c.b); }
        set
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = value.ToUnity();
        }
    }
    public bool FogEnabled
    {
        get { return RenderSettings.fog; }
        set { RenderSettings.fog = value; RenderSettings.fogMode = FogMode.Linear; }
    }
    public Color3 FogColor
    {
        get { var c = RenderSettings.fogColor; return new Color3(c.r, c.g, c.b); }
        set { RenderSettings.fogColor = value.ToUnity(); }
    }
    public float FogStart { get { return RenderSettings.fogStartDistance; } set { RenderSettings.fogStartDistance = value; } }
    public float FogEnd   { get { return RenderSettings.fogEndDistance; }   set { RenderSettings.fogEndDistance = value; } }
    public float Brightness
    {
        get { var s = Sun(); return s != null ? s.intensity : 0f; }
        set { var s = Sun(); if (s != null) s.intensity = value; }
    }
    public float ClockTime
    {
        get { return clockTime; }
        set
        {
            clockTime = value;
            var s = Sun();
            if (s != null) s.transform.rotation = Quaternion.Euler(value / 24f * 360f - 90f, 170f, 0f);
        }
    }
}
[MoonSharpUserData]
public class LuaSoundService
{
    public float Volume { get { return AudioListener.volume; } set { AudioListener.volume = Mathf.Clamp01(value); } }
    public void PlaySound(string soundId) { PlaySound(soundId, 1f); }
    public void PlaySound(string soundId, float volume)
    {
        var clip = LuaAudio.LoadClip(soundId);
        if (clip == null) return;
        Vector3 pos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        AudioSource.PlayClipAtPoint(clip, pos, volume);
    }
}
[Serializable]
public class LuaStoreFile
{
    public List<string> keys = new List<string>();
    public List<string> values = new List<string>();
}
[MoonSharpUserData]
public class LuaDataStoreService
{
    public LuaDataStore GetDataStore(string name) { return new LuaDataStore(name); }
}
[MoonSharpUserData]
public class LuaDataStore
{
    readonly string path;
    readonly Dictionary<string, string> data = new Dictionary<string, string>();
    [MoonSharpHidden]
    public LuaDataStore(string name)
    {
        string dir = Path.Combine(Application.persistentDataPath, "LuaData");
        Directory.CreateDirectory(dir);
        foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
        path = Path.Combine(dir, name + ".json");
        Load();
    }
    void Load()
    {
        if (!File.Exists(path)) return;
        try
        {
            var f = JsonUtility.FromJson<LuaStoreFile>(File.ReadAllText(path));
            for (int i = 0; i < f.keys.Count && i < f.values.Count; i++) data[f.keys[i]] = f.values[i];
        }
        catch (Exception e) { Debug.LogWarning("Could not read data store " + path + ": " + e.Message); }
    }
    void Save()
    {
        var f = new LuaStoreFile();
        foreach (var pair in data) { f.keys.Add(pair.Key); f.values.Add(pair.Value); }
        File.WriteAllText(path, JsonUtility.ToJson(f));
    }
    public void SetAsync(string key, DynValue value)
    {
        if (value.IsNil()) { RemoveAsync(key); return; }
        var wrapper = new Table(LuaWorld.Instance.Lua);
        wrapper.Set("v", value);
        data[key] = JsonTableConverter.TableToJson(wrapper);
        Save();
    }
    public DynValue GetAsync(string key)
    {
        string json;
        if (!data.TryGetValue(key, out json)) return DynValue.Nil;
        Table t = JsonTableConverter.JsonToTable(json, LuaWorld.Instance.Lua);
        return t.Get("v");
    }
    public DynValue UpdateAsync(string key, DynValue fn)
    {
        DynValue result = LuaWorld.Instance.Lua.Call(fn, GetAsync(key));
        if (result.Type == DataType.Tuple && result.Tuple.Length > 0) result = result.Tuple[0];
        SetAsync(key, result);
        return result;
    }
    public double IncrementAsync(string key, double delta)
    {
        DynValue old = GetAsync(key);
        double value = (old.Type == DataType.Number ? old.Number : 0) + delta;
        SetAsync(key, DynValue.NewNumber(value));
        return value;
    }
    public void RemoveAsync(string key)
    {
        if (data.Remove(key)) Save();
    }
    public Table ListKeys()
    {
        var t = new Table(LuaWorld.Instance.Lua);
        foreach (var k in data.Keys) t.Append(DynValue.NewString(k));
        return t;
    }
}
