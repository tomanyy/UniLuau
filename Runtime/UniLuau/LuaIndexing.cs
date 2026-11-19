using System;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
public static class LuaIndexing
{
    public static void Register()
    {
        UserData.RegisterType(typeof(LuaInstance), new ChildIndexDescriptor(
            typeof(LuaInstance), (o, name) => ((LuaInstance)o).FindFirstChild(name)));
        UserData.RegisterType(typeof(LuaScene), new ChildIndexDescriptor(
            typeof(LuaScene), (o, name) => ((LuaScene)o).FindChild(name)));
        UserData.RegisterType(typeof(LuaUnityComponent), new ChildIndexDescriptor(
            typeof(LuaUnityComponent), null));
        UserData.RegisterType(typeof(LuaUnityComponentSection), new ChildIndexDescriptor(
            typeof(LuaUnityComponentSection), null));
        UserData.RegisterType(typeof(LuaGame), new ChildIndexDescriptor(
            typeof(LuaGame), (o, name) => ((LuaGame)o).FindScene(name)));
        UserData.RegisterType(typeof(LuaGuiObject), new ChildIndexDescriptor(
            typeof(LuaGuiObject), (o, name) => ((LuaGuiObject)o).FindFirstChild(name)));
        UserData.RegisterType(typeof(LuaScriptRef), new ChildIndexDescriptor(typeof(LuaScriptRef), null));
        UserData.RegisterType(typeof(LuaUserInputService), new ChildIndexDescriptor(typeof(LuaUserInputService), null));
        UserData.RegisterType(typeof(LuaContextActionService), new ChildIndexDescriptor(typeof(LuaContextActionService), null));
        UserData.RegisterType(typeof(LuaPhysicsService), new ChildIndexDescriptor(typeof(LuaPhysicsService), null));
        UserData.RegisterType(typeof(LuaLighting), new ChildIndexDescriptor(typeof(LuaLighting), null));
        UserData.RegisterType(typeof(LuaSoundService), new ChildIndexDescriptor(typeof(LuaSoundService), null));
        UserData.RegisterType(typeof(LuaDataStoreService), new ChildIndexDescriptor(typeof(LuaDataStoreService), null));
        UserData.RegisterType(typeof(LuaDataStore), new ChildIndexDescriptor(typeof(LuaDataStore), null));
        UserData.RegisterType(typeof(LuaBindableEvent), new ChildIndexDescriptor(typeof(LuaBindableEvent), null));
        UserData.RegisterType(typeof(LuaBindableFunction), new ChildIndexDescriptor(typeof(LuaBindableFunction), null));
        UserData.RegisterType(typeof(LuaRemoteEvent), new ChildIndexDescriptor(typeof(LuaRemoteEvent), null));
        UserData.RegisterType(typeof(LuaNetworkPeer));
        UserData.RegisterType(typeof(LuaPrefab), new ChildIndexDescriptor(typeof(LuaPrefab), null));
        UserData.RegisterType(typeof(LuaRaycastParams), new ChildIndexDescriptor(typeof(LuaRaycastParams), null));
        UserData.RegisterType(typeof(LuaCollectionService), new ChildIndexDescriptor(typeof(LuaCollectionService), null));
        UserData.RegisterType(typeof(LuaDebris), new ChildIndexDescriptor(typeof(LuaDebris), null));
    }
}
public class ChildIndexDescriptor : StandardUserDataDescriptor
{
    readonly Func<object, string, object> lookup;
    public ChildIndexDescriptor(Type type, Func<object, string, object> lookup)
        : base(type, InteropAccessMode.Default)
    {
        this.lookup = lookup;
    }
    public override DynValue Index(Script script, object obj, DynValue index, bool isDirectIndexing)
    {
        DynValue value = base.Index(script, obj, index, isDirectIndexing);
        if (value != null && !value.IsNil()) return value;
        if (obj == null || index.Type != DataType.String) return value;
        if (HasMemberNamed(index.String)) return value ?? DynValue.Nil;
        object unityValue;
        var instance = obj as LuaInstance;
        if (instance != null && instance.TryGetUnityComponent(index.String, out unityValue))
            return unityValue == null ? DynValue.Nil : DynValue.FromObject(script, unityValue);
        var component = obj as LuaUnityComponent;
        if (component != null)
        {
            if (component.TryGetProperty(index.String, out unityValue))
                return unityValue == null ? DynValue.Nil : DynValue.FromObject(script, unityValue);
            return DynValue.FromObject(script, component.GetSection(index.String));
        }
        var section = obj as LuaUnityComponentSection;
        if (section != null)
        {
            if (section.TryGetProperty(index.String, out unityValue))
                return unityValue == null ? DynValue.Nil : DynValue.FromObject(script, unityValue);
            return DynValue.FromObject(script, section.GetSection(index.String));
        }
        if (lookup != null)
        {
            object child = lookup(obj, index.String);
            if (child != null) return DynValue.FromObject(script, child);
        }
        return DynValue.Nil;
    }
    public override bool SetIndex(Script script, object obj, DynValue index, DynValue value, bool isDirectIndexing)
    {
        if (base.SetIndex(script, obj, index, value, isDirectIndexing)) return true;
        if (index.Type != DataType.String) return false;
        string error;
        var component = obj as LuaUnityComponent;
        if (component != null)
        {
            if (component.TrySetProperty(index.String, value, out error)) return true;
            throw new ScriptRuntimeException(error ?? ("Unity component property not found: " + index.String));
        }
        var section = obj as LuaUnityComponentSection;
        if (section != null)
        {
            if (section.TrySetProperty(index.String, value, out error)) return true;
            throw new ScriptRuntimeException(error ?? ("Unity component property not found: " + index.String));
        }
        return false;
    }
    bool HasMemberNamed(string name)
    {
        foreach (string member in MemberNames)
            if (string.Equals(member, name, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }
}
