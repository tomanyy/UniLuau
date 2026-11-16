using System;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;
[MoonSharpUserData]
public class LuaContextActionService
{
    class Binding
    {
        public string Name;
        public DynValue Callback;
        public object Owner;
        public HashSet<string> Inputs;
        public int Priority;
        public LuaGuiObject TouchScreen;
        public LuaGuiObject TouchButton;
    }
    readonly Scheduler scheduler;
    readonly Dictionary<string, Binding> bindings = new Dictionary<string, Binding>();
    int nextPriority;
    int touchButtonCount;
    [MoonSharpHidden]
    public LuaContextActionService(Scheduler scheduler, Signal began, Signal ended)
    {
        this.scheduler = scheduler;
        began.Raised += args => Process(args, "Begin");
        ended.Raised += args => Process(args, "End");
    }
    public void BindAction(string actionName, DynValue callback, bool createTouchButton, params object[] inputs)
    {
        Bind(actionName, callback, createTouchButton, 0, inputs);
    }
    public void BindActionAtPriority(string actionName, DynValue callback, bool createTouchButton, int priority, params object[] inputs)
    {
        Bind(actionName, callback, createTouchButton, priority, inputs);
    }
    void Bind(string actionName, DynValue callback, bool createTouchButton, int priority, object[] inputs)
    {
        if (string.IsNullOrEmpty(actionName)) throw new ScriptRuntimeException("Action name cannot be empty");
        if (callback == null || callback.Type != DataType.Function) throw new ScriptRuntimeException("Action callback must be a function");
        UnbindAction(actionName);
        var binding = new Binding
        {
            Name = actionName,
            Callback = callback,
            Owner = scheduler.CurrentOwner,
            Priority = priority == 0 ? ++nextPriority : priority,
            Inputs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        };
        if (inputs != null)
            foreach (object input in inputs)
            {
                string name = InputName(input);
                if (!string.IsNullOrEmpty(name)) binding.Inputs.Add(name);
            }
        if (createTouchButton) binding.TouchScreen = CreateTouchButton(binding);
        bindings[actionName] = binding;
        scheduler.TrackCleanup(() =>
        {
            Binding current;
            if (bindings.TryGetValue(actionName, out current) && ReferenceEquals(current, binding))
                UnbindAction(actionName);
        });
    }
    public void UnbindAction(string actionName)
    {
        Binding binding;
        if (string.IsNullOrEmpty(actionName) || !bindings.TryGetValue(actionName, out binding)) return;
        bindings.Remove(actionName);
        if (binding.TouchScreen != null) binding.TouchScreen.Destroy();
        binding.TouchScreen = null;
        binding.TouchButton = null;
    }
    public void UnbindAllActions()
    {
        foreach (string name in new List<string>(bindings.Keys)) UnbindAction(name);
    }
    public void SetTitle(string actionName, string title)
    {
        Binding binding;
        if (bindings.TryGetValue(actionName, out binding) && binding.TouchButton != null)
            binding.TouchButton.Text = title ?? actionName;
    }
    public Table GetAllBoundActionInfo()
    {
        var result = new Table(LuaWorld.Instance.Lua);
        foreach (var pair in bindings)
        {
            var info = new Table(LuaWorld.Instance.Lua);
            info["Name"] = DynValue.NewString(pair.Key);
            info["Priority"] = DynValue.NewNumber(pair.Value.Priority);
            info["CreateTouchButton"] = DynValue.NewBoolean(pair.Value.TouchScreen != null);
            var inputs = new Table(LuaWorld.Instance.Lua);
            foreach (string input in pair.Value.Inputs) inputs.Append(DynValue.NewString(input));
            info["Inputs"] = DynValue.NewTable(inputs);
            result[pair.Key] = DynValue.NewTable(info);
        }
        return result;
    }
    void Process(object[] args, string state)
    {
        if (args == null || args.Length == 0) return;
        var input = args[0] as LuaInput;
        if (input == null) return;
        bool processed = args.Length > 1 && args[1] is bool && (bool)args[1];
        if (processed) return;                                              
        var matches = new List<Binding>();
        foreach (Binding binding in bindings.Values)
            if (binding.Inputs.Contains(input.KeyCode) || binding.Inputs.Contains(input.UserInputType)) matches.Add(binding);
        matches.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        foreach (Binding binding in matches)
            scheduler.SpawnOwned(binding.Owner, binding.Callback, binding.Name, input, state);
    }
    void ProcessTouchButton(Binding binding)
    {
        var input = new LuaInput("None", "Touch");
        scheduler.SpawnOwned(binding.Owner, binding.Callback, binding.Name, input, "Begin");
        scheduler.SpawnOwned(binding.Owner, binding.Callback, binding.Name, input, "End");
    }
    LuaGuiObject CreateTouchButton(Binding binding)
    {
        var screen = (LuaGuiObject)LuaFactory.Create("ScreenGui");
        screen.Name = "ContextAction_" + binding.Name;
        screen.Parent = new LuaGuiRoot();
        var button = (LuaGuiObject)LuaFactory.Create("TextButton");
        button.Name = "TouchButton_" + binding.Name;
        button.Text = binding.Name;
        button.Size = new UDim2(0, 132, 0, 52);
        button.AnchorPoint = new Vec2(1, 1);
        button.Position = new UDim2(0, Screen.width - 20, 0, Screen.height - 20 - touchButtonCount * 60);
        button.Parent = screen;
        button.AddNativeClickListener(() => ProcessTouchButton(binding));
        binding.TouchButton = button;
        touchButtonCount++;
        return screen;
    }
    static string InputName(object input)
    {
        if (input == null) return null;
        var value = input as DynValue;
        return value != null ? value.CastToString() : input.ToString();
    }
}
[MoonSharpUserData]
public class LuaPhysicsService
{
    readonly Dictionary<string, int> layers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<int, string> names = new Dictionary<int, string>();
    [MoonSharpHidden]
    public LuaPhysicsService()
    {
        layers["Default"] = 0;
        names[0] = "Default";
    }
    public void RegisterCollisionGroup(string name)
    {
        if (string.IsNullOrEmpty(name)) throw new ScriptRuntimeException("Collision group name cannot be empty");
        if (layers.ContainsKey(name)) throw new ScriptRuntimeException("Collision group already exists: " + name);
        var used = new HashSet<int>(layers.Values);
        foreach (GameObject go in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
            if (go != null && go.layer >= 8) used.Add(go.layer);
        for (int layer = 8; layer < 32; layer++)
        {
            if (used.Contains(layer)) continue;
            layers[name] = layer;
            names[layer] = name;
            return;
        }
        throw new ScriptRuntimeException("No free Unity layers remain for collision group " + name + ". Reserve layers 8-31 for Lua collision groups.");
    }
    public void CollisionGroupSetCollidable(string name1, string name2, bool collidable)
    {
        int a = GetLayer(name1);
        int b = GetLayer(name2);
        Physics.IgnoreLayerCollision(a, b, !collidable);
    }
    public void SetPartCollisionGroup(LuaInstance instance, string groupName)
    {
        if (instance == null || instance.Go == null) return;
        int layer = GetLayer(groupName);
        foreach (Transform item in instance.Go.GetComponentsInChildren<Transform>(true))
            item.gameObject.layer = layer;
    }
    public string GetCollisionGroupName(int layer)
    {
        string name;
        return names.TryGetValue(layer, out name) ? name : "Default";
    }
    public Table GetRegisteredCollisionGroups()
    {
        var result = new Table(LuaWorld.Instance.Lua);
        foreach (var pair in layers)
        {
            var info = new Table(LuaWorld.Instance.Lua);
            info["name"] = DynValue.NewString(pair.Key);
            info["layer"] = DynValue.NewNumber(pair.Value);
            result.Append(DynValue.NewTable(info));
        }
        return result;
    }
    int GetLayer(string name)
    {
        int layer;
        if (!string.IsNullOrEmpty(name) && layers.TryGetValue(name, out layer)) return layer;
        throw new ScriptRuntimeException("Collision group is not registered: " + name);
    }
}
