using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LuaBehaviour))]
public class LuaBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var script = serializedObject.FindProperty("script");
        if (script != null && script.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Assign a .lua or .luau asset above. You can also drop one onto this object in the Hierarchy.", MessageType.Info);
        }
    }
}

[InitializeOnLoad]
public static class LuaScriptDropHandler
{
    static LuaScriptDropHandler()
    {
        EditorApplication.hierarchyWindowItemByEntityIdOnGUI += OnHierarchyItemGUI;
        EditorApplication.projectWindowItemOnGUI += OnProjectItemGUI;
        SceneView.duringSceneGui += OnSceneGUI;
        EditorApplication.delayCall += EnsureSpecialPrefabs;
    }

    static void EnsureSpecialPrefabs()
    {
        const string resourcesPath = "Assets/Resources";
        const string prefabFolder = resourcesPath + "/Prefabs";
        if (!AssetDatabase.IsValidFolder(resourcesPath)) AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(prefabFolder)) AssetDatabase.CreateFolder(resourcesPath, "Prefabs");

        string[] classes = {
            "BindableEvent", "BindableFunction", "RemoteEvent",
            "ScreenGui", "Frame", "ScrollingFrame", "TextLabel", "TextButton", "ImageLabel", "ImageButton", "TextBox"
        };

        bool changed = false;
        foreach (string className in classes)
        {
            string path = prefabFolder + "/" + className + ".prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) continue;

            bool ui = className == "ScreenGui" || className == "Frame" || className == "ScrollingFrame" || className == "TextLabel" ||
                      className == "TextButton" || className == "ImageLabel" || className == "ImageButton" || className == "TextBox";
            var template = ui
                ? new GameObject(className, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image))
                : new GameObject(className);
            PrefabUtility.SaveAsPrefabAsset(template, path);
            Object.DestroyImmediate(template);
            changed = true;
        }

        if (changed) AssetDatabase.Refresh();
    }

    static void OnHierarchyItemGUI(EntityId entityId, Rect row)
    {
        Event evt = Event.current;
        if (evt == null || !row.Contains(evt.mousePosition) || !HasLuaScript()) return;

        if (evt.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.Use();
        }
        else if (evt.type == EventType.DragPerform)
        {
            var target = EditorUtility.EntityIdToObject(entityId) as GameObject;
            TextAsset script = GetDraggedScript();
            if (script == null) return;

            DragAndDrop.AcceptDrag();
            if (target != null)
                AddScript(target, script);
            else
                CreateObjectFromScript(script);
            evt.Use();
        }
    }

    static void OnProjectItemGUI(string guid, Rect row)
    {
        Event evt = Event.current;
        if (evt == null || !row.Contains(evt.mousePosition) || !HasLuaScript()) return;

        string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
        if (Path.GetExtension(prefabPath) != ".prefab") return;

        if (evt.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.Use();
        }
        else if (evt.type == EventType.DragPerform)
        {
            TextAsset script = GetDraggedScript();
            DragAndDrop.AcceptDrag();
            AddScriptToPrefab(prefabPath, script);
            evt.Use();
        }
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        Event evt = Event.current;
        if (evt == null || !HasLuaScript()) return;

        if (evt.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.Use();
        }
        else if (evt.type == EventType.DragPerform)
        {
            TextAsset script = GetDraggedScript();
            if (script == null) return;
            DragAndDrop.AcceptDrag();

            Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
            RaycastHit hit;
            Vector3 position = Physics.Raycast(ray, out hit) ? hit.point : ray.GetPoint(10f);
            var go = new GameObject(Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(script)));
            go.transform.position = position;
            Undo.RegisterCreatedObjectUndo(go, "Create Lua GameObject");
            AddScript(go, script);
            Selection.activeGameObject = go;
            evt.Use();
        }
    }

    [MenuItem("Assets/Add Lua Script to New GameObject", true)]
    static bool ValidateCreateObjectFromScript()
    {
        return GetSelectedScript() != null;
    }

    [MenuItem("Assets/Add Lua Script to New GameObject", false, 2000)]
    static void CreateObjectFromScript()
    {
        TextAsset script = GetSelectedScript();
        if (script == null) return;

        CreateObjectFromScript(script);
    }

    static void CreateObjectFromScript(TextAsset script)
    {
        if (script == null) return;
        var go = new GameObject(Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(script)));
        Undo.RegisterCreatedObjectUndo(go, "Create Lua GameObject");
        AddScript(go, script);
        Selection.activeGameObject = go;
    }

    static bool HasLuaScript()
    {
        return GetDraggedScript() != null;
    }

    static TextAsset GetDraggedScript()
    {
        if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length == 1)
        {
            var script = AsLuaScript(DragAndDrop.objectReferences[0]);
            if (script != null) return script;
        }

                                                                              
                                                                             
        if (DragAndDrop.paths != null && DragAndDrop.paths.Length == 1)
        {
            string path = DragAndDrop.paths[0];
            string extension = Path.GetExtension(path).ToLowerInvariant();
            if (extension == ".lua" || extension == ".luau")
                return AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        }
        return null;
    }

    static TextAsset GetSelectedScript()
    {
        return Selection.objects != null && Selection.objects.Length == 1
            ? AsLuaScript(Selection.objects[0])
            : null;
    }

    static TextAsset AsLuaScript(Object candidate)
    {
        var asset = candidate as TextAsset;
        if (asset == null) return null;
        string extension = Path.GetExtension(AssetDatabase.GetAssetPath(asset)).ToLowerInvariant();
        return extension == ".lua" || extension == ".luau" ? asset : null;
    }

    static void AddScriptToPrefab(string prefabPath, TextAsset script)
    {
        if (script == null) return;
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            var behaviour = root.AddComponent<LuaBehaviour>();
            behaviour.script = script;
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static void AddScript(GameObject target, TextAsset script)
    {
        if (target == null || script == null) return;

        var behaviour = Undo.AddComponent<LuaBehaviour>(target);
        var serialized = new SerializedObject(behaviour);
        serialized.FindProperty("script").objectReferenceValue = script;
        serialized.ApplyModifiedProperties();
        PrefabUtility.RecordPrefabInstancePropertyModifications(behaviour);
        EditorUtility.SetDirty(behaviour);
    }
}
