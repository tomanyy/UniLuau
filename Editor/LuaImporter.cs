using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.AssetImporters;
[ScriptedImporter(1, new[] { "lua", "luau" })]
public class LuaImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var asset = new TextAsset(File.ReadAllText(ctx.assetPath));
        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.tomany.uniluau/Editor/lua.png");
        ctx.AddObjectToAsset("main", asset, icon);
        ctx.SetMainObject(asset);
    }
}
