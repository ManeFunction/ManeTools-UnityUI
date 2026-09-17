using UnityEditor;
using UnityEditor.EventSystems;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mane.Unity.UI.Editor
{
    /// <summary>
    /// Scene helpers for placing UI under a Canvas and ensuring EventSystem exists.
    /// </summary>
    public static class UICanvasTools
    {
        private const string UiLayerName = "UI";

        /// <summary>
        /// Parents <paramref name="element"/> under a Canvas (creating one if needed) and selects it.
        /// </summary>
        public static void PlaceUIElement(GameObject element, MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null)
            {
                parent = GetOrCreateCanvas();

                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null && !prefabStage.IsPartOfPrefabContents(parent))
                    parent = prefabStage.prefabContentsRoot;
            }

            if (parent.GetComponentsInParent<Canvas>(true).Length == 0)
            {
                GameObject canvas = CreateCanvas();
                Undo.SetTransformParent(canvas.transform, parent.transform, "");
                parent = canvas;
            }

            SetParentAndAlign(element, parent);
            GameObjectUtility.EnsureUniqueNameForSibling(element);

            Undo.RegisterFullObjectHierarchyUndo(parent == null ? element : parent, "");
            Undo.SetCurrentGroupName("Create " + element.name);

            Selection.activeGameObject = element;
        }

        /// <summary>
        /// Returns a valid Canvas in the current stage, or creates one.
        /// </summary>
        public static GameObject GetOrCreateCanvas()
        {
            GameObject selected = Selection.activeGameObject;
            Canvas canvas = selected != null ? selected.GetComponentInParent<Canvas>() : null;
            if (IsValidCanvas(canvas))
                return canvas.gameObject;

            Canvas[] canvases = StageUtility.GetCurrentStageHandle().FindComponentsOfType<Canvas>();
            foreach (var c in canvases)
            {
                if (IsValidCanvas(c))
                    return c.gameObject;
            }

            return CreateCanvas();
        }

        /// <summary>
        /// True when <paramref name="canvas"/> is an active, non-hidden object in the current stage.
        /// </summary>
        public static bool IsValidCanvas(Canvas canvas)
        {
            if (canvas == null || !canvas.gameObject.activeInHierarchy)
                return false;

            if (EditorUtility.IsPersistent(canvas) || (canvas.hideFlags & HideFlags.HideInHierarchy) != 0)
                return false;

            return StageUtility.GetStageHandle(canvas.gameObject) == StageUtility.GetCurrentStageHandle();
        }

        /// <summary>
        /// Creates a Screen Space Overlay Canvas with scaler and raycaster.
        /// </summary>
        public static GameObject CreateCanvas()
        {
            GameObject root = ObjectFactory.CreateGameObject("Canvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.layer = LayerMask.NameToLayer(UiLayerName);
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            StageUtility.PlaceGameObjectInCurrentStage(root);

            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                Undo.SetTransformParent(root.transform, prefabStage.prefabContentsRoot.transform, "");
                Undo.SetCurrentGroupName("Create " + root.name);
                return root;
            }

            Undo.SetCurrentGroupName("Create " + root.name);
            EnsureEventSystem();
            return root;
        }

        /// <summary>
        /// Creates an EventSystem in the current stage if none exists.
        /// </summary>
        public static void EnsureEventSystem()
        {
            StageHandle stage = StageUtility.GetCurrentStageHandle();
            if (stage.FindComponentOfType<EventSystem>() != null)
                return;

            GameObject eventSystem = ObjectFactory.CreateGameObject("EventSystem");
            StageUtility.PlaceGameObjectInCurrentStage(eventSystem);
            ObjectFactory.AddComponent<EventSystem>(eventSystem);
            InputModuleComponentFactory.AddInputModule(eventSystem);
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
        }

        /// <summary>
        /// Parents <paramref name="child"/> under <paramref name="parent"/>, resets local transform, copies layer.
        /// </summary>
        public static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (parent == null)
                return;

            Undo.SetTransformParent(child.transform, parent.transform, "");

            RectTransform rectTransform = child.transform as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                Vector3 localPosition = rectTransform.localPosition;
                localPosition.z = 0f;
                rectTransform.localPosition = localPosition;
            }
            else
            {
                child.transform.localPosition = Vector3.zero;
            }

            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            SetLayerRecursively(child, parent.layer);
        }

        /// <summary>
        /// Focuses the Hierarchy and starts rename on the next editor tick.
        /// </summary>
        public static void BeginHierarchyRename()
        {
            EditorApplication.delayCall += () =>
            {
                EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
                EditorApplication.ExecuteMenuItem("Edit/Rename");
            };
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform transform = go.transform;
            for (int i = 0; i < transform.childCount; i++)
                SetLayerRecursively(transform.GetChild(i).gameObject, layer);
        }
    }
}
