using UnityEditor;

public static class LuaMenuItems
{
    [MenuItem("Assets/Create/Scripting/Lua Script", false, 81)]
    public static void CreateLuaScript()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile("Packages/com.tomany.uniluau/Editor/LuaTemplate.txt", "NewLuaScript.lua");
    }

    [MenuItem("Assets/Create/Scripting/Lua Module Script", false, 82)]
    public static void CreateLuauScript()
    {
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile("Packages/com.tomany.uniluau/Editor/LuaModuleTemplate.txt", "NewLuauScript.luau");
    }
}
