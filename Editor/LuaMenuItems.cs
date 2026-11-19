using UnityEditor;
using UnityEngine;
public static class LuaMenuItems
{
    static void CreateFromTemplate(string templateName, string fileName)
    {
        string packagePath = "Packages/com.tomany.uniluau/Editor/" + templateName;
        string assetPath = "Assets/Editor/" + templateName;
        string path = AssetDatabase.LoadAssetAtPath<TextAsset>(packagePath) != null ? packagePath : assetPath;
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, fileName);
    }

    [MenuItem("Assets/Create/Scripting/Lua Script", false, 81)]
    public static void CreateLuaScript()
    {
        CreateFromTemplate("LuaTemplate.txt", "NewLuaScript.lua");
    }
    [MenuItem("Assets/Create/Scripting/Lua Module Script", false, 82)]
    public static void CreateLuauScript()
    {
        CreateFromTemplate("LuaModuleTemplate.txt", "NewLuauScript.luau");
    }

    [MenuItem("Assets/Create/Scripting/Lua Gameplay Script", false, 83)]
    public static void CreateGameplayScript()
    {
        CreateFromTemplate("LuaGameplayTemplate.txt", "NewGameplayScript.lua");
    }

    [MenuItem("Assets/Create/Scripting/Lua Lifecycle Script", false, 84)]
    public static void CreateLifecycleScript()
    {
        CreateFromTemplate("LuaLifecycleTemplate.txt", "NewLifecycleScript.lua");
    }
}
