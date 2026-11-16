using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MoonSharp.Interpreter;
using UnityEditor;
using UnityEngine;
[InitializeOnLoad]
public static class LuaBehaviourInspectorExtras
{
    sealed class Validation
    {
        public string Source;
        public string Error;
        public readonly List<string> MissingModules = new List<string>();
    }
    static readonly Dictionary<EntityId, Validation> validationCache = new Dictionary<EntityId, Validation>();
    static readonly Regex requires = new Regex(@"\brequire\s*\(?\s*['""]([^'""]+)['""]\s*\)?", RegexOptions.Compiled);
    static bool previewExpanded;
    static LuaBehaviourInspectorExtras()
    {
        Editor.finishedDefaultHeaderGUI += DrawInspectorHeader;
        EditorApplication.hierarchyWindowItemByEntityIdOnGUI += OnHierarchyItemGUI;
        EditorApplication.projectWindowItemOnGUI += OnProjectItemGUI;
        EditorApplication.projectChanged += validationCache.Clear;
        SceneView.duringSceneGui += OnSceneGUI;
    }
    static void DrawInspectorHeader(Editor editor)
    {
        var behaviour = editor.target as LuaBehaviour;
        if (behaviour != null)
        {
            DrawBehaviourTools(behaviour);
            return;
        }
        if (!(editor.target is Transform) || EditorUtility.IsPersistent(editor.target)) return;
        var evt = Event.current;
        if (evt == null) return;
        Rect dropArea = EditorGUILayout.GetControlRect(false, 28f);
        GUI.Box(dropArea, "Drop .lua / .luau scripts to attach them to this object");
        HandleDrop(evt, dropArea, scripts =>
        {
            foreach (var target in editor.targets)
            {
                var component = target as Transform;
                if (component != null) AddScripts(component.gameObject, scripts);
            }
        });
    }
    static void DrawBehaviourTools(LuaBehaviour behaviour)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        bool enabled = EditorGUILayout.ToggleLeft("Script Enabled", behaviour.enabled, GUILayout.Width(130f));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(behaviour, "Toggle Lua Script");
            behaviour.enabled = enabled;
            EditorUtility.SetDirty(behaviour);
        }
        string status = !behaviour.enabled ? "Disabled"
            : behaviour.script == null ? "Missing script"
            : EditorApplication.isPlaying ? behaviour.RuntimeStatus : "Ready";
        GUILayout.Label("Status: " + status, EditorStyles.miniLabel);
        if (behaviour.script != null && GUILayout.Button("Open", EditorStyles.miniButton, GUILayout.Width(48f)))
            AssetDatabase.OpenAsset(behaviour.script);
        EditorGUILayout.EndHorizontal();
        if (behaviour.script == null)
        {
            EditorGUILayout.HelpBox("Assign a .lua or .luau script asset.", MessageType.Warning);
            return;
        }
        Validation result = Validate(behaviour.script);
        if (!string.IsNullOrEmpty(result.Error))
            EditorGUILayout.HelpBox("Lua validation error: " + result.Error, MessageType.Error);
        foreach (string module in result.MissingModules)
            EditorGUILayout.HelpBox("Module not found in Assets/Resources/Lua: " + module, MessageType.Warning);
        if (!string.IsNullOrEmpty(behaviour.LastError) && EditorApplication.isPlaying)
            EditorGUILayout.HelpBox(behaviour.LastError, MessageType.Error);
        previewExpanded = EditorGUILayout.Foldout(previewExpanded, "Script Source Preview", true);
        if (previewExpanded)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextArea(behaviour.script.text, GUILayout.MinHeight(100f));
            EditorGUI.EndDisabledGroup();
        }
    }
    static Validation Validate(TextAsset asset)
    {
        EntityId id = asset.GetEntityId();
        Validation cached;
        if (validationCache.TryGetValue(id, out cached) && cached.Source == asset.text) return cached;
        var result = new Validation { Source = asset.text };
        try
        {
            string source = Luau.Transpile(asset.text);
            new Script(CoreModules.Preset_SoftSandbox).LoadString(source, null, asset.name);
        }
        catch (Exception e)
        {
            result.Error = e.Message;
        }
        var seen = new HashSet<string>();
        foreach (Match match in requires.Matches(asset.text))
        {
            string module = match.Groups[1].Value;
            if (seen.Add(module) && !ModuleExists(module)) result.MissingModules.Add(module);
        }
        validationCache[id] = result;
        return result;
    }
    static bool ModuleExists(string module)
    {
        string root = Path.Combine(Application.dataPath, "Resources", "Lua", module.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(root + ".lua") || File.Exists(root + ".luau");
    }
    static void OnHierarchyItemGUI(EntityId entityId, Rect row)
    {
        HandleManyOnly(Event.current, row, scripts =>
        {
            var target = EditorUtility.EntityIdToObject(entityId) as GameObject;
            if (target != null) AddScripts(target, scripts);
        });
    }
    static void OnProjectItemGUI(string guid, Rect row)
    {
        HandleManyOnly(Event.current, row, scripts =>
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.Equals(Path.GetExtension(path), ".prefab", StringComparison.OrdinalIgnoreCase)) return;
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                AddScripts(root, scripts, false);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        });
    }
    static void OnSceneGUI(SceneView view)
    {
        var evt = Event.current;
        if (evt == null || GetDraggedScripts().Length < 2) return;
        if (evt.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.Use();
        }
        else if (evt.type == EventType.DragPerform)
        {
            TextAsset[] scripts = GetDraggedScripts();
            if (scripts.Length < 2) return;
            DragAndDrop.AcceptDrag();
            Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
            RaycastHit hit;
            Vector3 position = Physics.Raycast(ray, out hit) ? hit.point : ray.GetPoint(10f);
            var go = new GameObject(Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(scripts[0])));
            go.transform.position = position;
            Undo.RegisterCreatedObjectUndo(go, "Create Lua GameObject");
            AddScripts(go, scripts);
            Selection.activeGameObject = go;
            evt.Use();
        }
    }
    static void HandleManyOnly(Event evt, Rect area, Action<TextAsset[]> perform)
    {
        if (evt == null || !area.Contains(evt.mousePosition)) return;
        TextAsset[] scripts = GetDraggedScripts();
        if (scripts.Length < 2) return;                                                               
        if (evt.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.Use();
        }
        else if (evt.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            perform(scripts);
            evt.Use();
        }
    }
    static void HandleDrop(Event evt, Rect area, Action<TextAsset[]> perform)
    {
        if (!area.Contains(evt.mousePosition)) return;
        TextAsset[] scripts = GetDraggedScripts();
        if (scripts.Length == 0) return;
        if (evt.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.Use();
        }
        else if (evt.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            perform(scripts);
            evt.Use();
        }
    }
    static TextAsset[] GetDraggedScripts()
    {
        var found = new List<TextAsset>();
        foreach (var candidate in DragAndDrop.objectReferences)
        {
            var asset = candidate as TextAsset;
            if (asset == null) continue;
            string ext = Path.GetExtension(AssetDatabase.GetAssetPath(asset));
            if ((ext == ".lua" || ext == ".luau") && !found.Contains(asset)) found.Add(asset);
        }
        return found.ToArray();
    }
    public static void AddScripts(GameObject target, TextAsset[] scripts, bool recordUndo = true)
    {
        if (target == null) return;
        foreach (var script in scripts)
        {
            var behaviour = recordUndo ? Undo.AddComponent<LuaBehaviour>(target) : target.AddComponent<LuaBehaviour>();
            if (recordUndo) Undo.RecordObject(behaviour, "Assign Lua Script");
            behaviour.script = script;
            if (recordUndo) PrefabUtility.RecordPrefabInstancePropertyModifications(behaviour);
            EditorUtility.SetDirty(behaviour);
        }
    }
}
[InitializeOnLoad]
public static class LuaScriptMultiCreateMenu
{
    static LuaScriptMultiCreateMenu()
    {
    }
    [MenuItem("Assets/Add Selected Lua Scripts to New GameObject", true)]
    static bool ValidateCreate()
    {
        return SelectedScripts().Length > 0;
    }
    [MenuItem("Assets/Add Selected Lua Scripts to New GameObject", false, 2001)]
    static void Create()
    {
        TextAsset[] scripts = SelectedScripts();
        if (scripts.Length == 0) return;
        var go = new GameObject(Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(scripts[0])));
        Undo.RegisterCreatedObjectUndo(go, "Create Lua GameObject");
        LuaBehaviourInspectorExtras.AddScripts(go, scripts);
        Selection.activeGameObject = go;
    }
    static TextAsset[] SelectedScripts()
    {
        var found = new List<TextAsset>();
        foreach (var candidate in Selection.objects)
        {
            var asset = candidate as TextAsset;
            if (asset == null) continue;
            string ext = Path.GetExtension(AssetDatabase.GetAssetPath(asset));
            if ((ext == ".lua" || ext == ".luau") && !found.Contains(asset)) found.Add(asset);
        }
        return found.ToArray();
    }
}
