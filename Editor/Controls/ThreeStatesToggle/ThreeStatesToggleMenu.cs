using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Mane.Unity.UI.Editor
{
    internal static class ThreeStatesToggleMenu
    {
        private const string PrefabGuid = "89a06538f68ed48bfba7cbdd93317998";
        private const int MenuPriority = 2102;

        [MenuItem("GameObject/UI (Canvas)/Three States Toggle", false, MenuPriority)]
        private static void CreateThreeStateToggle(MenuCommand menuCommand)
        {
            if (Selection.gameObjects.Length > 1 && menuCommand.context != Selection.activeGameObject)
                return;

            GameObject prefab = LoadPrefab();
            if (prefab == null)
                return;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            StageUtility.PlaceGameObjectInCurrentStage(instance);
            Undo.RegisterCreatedObjectUndo(instance, "Create Three States Toggle");
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.UserAction);

            UICanvasTools.PlaceUIElement(instance, menuCommand);
            UICanvasTools.BeginHierarchyRename();
        }

        private static GameObject LoadPrefab()
        {
            string path = AssetDatabase.GUIDToAssetPath(PrefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                Debug.LogError($"ThreeStatesToggle prefab was not found (guid {PrefabGuid}).");

            return prefab;
        }
    }
}
