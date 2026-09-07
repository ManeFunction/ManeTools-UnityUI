using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Mane.Unity.UI.Editor
{
    internal static class SelectableNavigationVisualizer
    {
        public const string PrefKey = "SelectableEditor.ShowNavigation";

        private const float ArrowThickness = 2.5f;
        private const float ArrowHeadSize = 1.2f;

        private static int _subscribers;

        public static bool Enabled
        {
            get => EditorPrefs.GetBool(PrefKey);
            set
            {
                if (Enabled == value)
                    return;

                EditorPrefs.SetBool(PrefKey, value);
                SceneView.RepaintAll();
            }
        }

        public static void AddSubscriber()
        {
            if (_subscribers++ == 0)
                SceneView.duringSceneGui += OnSceneGUI;
        }

        public static void RemoveSubscriber()
        {
            _subscribers--;
            if (_subscribers > 0)
                return;

            _subscribers = 0;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView view)
        {
            if (!Enabled)
                return;

            Selectable[] selectables = Selectable.allSelectablesArray;
            for (int i = 0; i < selectables.Length; i++)
            {
                Selectable selectable = selectables[i];
                if (StageUtility.IsGameObjectRenderedByCamera(selectable.gameObject, Camera.current))
                    DrawNavigation(selectable);
            }
        }

        private static void DrawNavigation(Selectable selectable)
        {
            if (selectable == null)
                return;

            Transform transform = selectable.transform;
            bool active = Selection.transforms.Any(t => t == transform);

            Handles.color = new Color(1f, .6f, .2f, active ? 1f : .4f);
            DrawArrow(-Vector2.right, selectable, selectable.FindSelectableOnLeft());
            DrawArrow(Vector2.up, selectable, selectable.FindSelectableOnUp());

            Handles.color = new Color(1f, .9f, .1f, active ? 1f : .4f);
            DrawArrow(Vector2.right, selectable, selectable.FindSelectableOnRight());
            DrawArrow(-Vector2.up, selectable, selectable.FindSelectableOnDown());
        }

        private static void DrawArrow(Vector2 direction, Selectable from, Selectable to)
        {
            if (from == null || to == null)
                return;

            Transform fromTransform = from.transform;
            Transform toTransform = to.transform;
            Vector2 side = new(direction.y, -direction.x);
            Vector3 fromPoint = fromTransform.TransformPoint(PointOnRectEdge(fromTransform as RectTransform, direction));
            Vector3 toPoint = toTransform.TransformPoint(PointOnRectEdge(toTransform as RectTransform, -direction));
            float fromSize = HandleUtility.GetHandleSize(fromPoint) * .05f;
            float toSize = HandleUtility.GetHandleSize(toPoint) * .05f;
            fromPoint += fromTransform.TransformDirection(side) * fromSize;
            toPoint += toTransform.TransformDirection(side) * toSize;
            float length = Vector3.Distance(fromPoint, toPoint);
            Vector3 fromTangent = fromTransform.rotation * direction * length * .3f;
            Vector3 toTangent = toTransform.rotation * -direction * length * .3f;

            Handles.DrawBezier(fromPoint, toPoint, fromPoint + fromTangent, toPoint + toTangent,
                Handles.color, null, ArrowThickness);
            Handles.DrawAAPolyLine(ArrowThickness, toPoint,
                toPoint + toTransform.rotation * (-direction - side) * toSize * ArrowHeadSize);
            Handles.DrawAAPolyLine(ArrowThickness, toPoint,
                toPoint + toTransform.rotation * (-direction + side) * toSize * ArrowHeadSize);
        }

        private static Vector3 PointOnRectEdge(RectTransform rect, Vector2 direction)
        {
            if (rect == null)
                return Vector3.zero;

            if (direction != Vector2.zero)
                direction /= Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.y));

            return rect.rect.center + Vector2.Scale(rect.rect.size, direction * .5f);
        }
    }
}
