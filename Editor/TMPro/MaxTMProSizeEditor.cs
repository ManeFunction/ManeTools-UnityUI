using Mane.Unity.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.UI.Editor
{
    /// <summary>
    /// Inspector for <see cref="MaxTMProSize"/>. Layout comes from the assigned UXML.
    /// Options that depend on <c>Max Width</c> are hidden while it is unlimited.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MaxTMProSize), true)]
    public class MaxTMProSizeEditor : ManeEditor
    {
        private VisualElement _compactWidth;

        protected override void BuildInspector(VisualElement root)
        {
            _compactWidth = root.Q<PropertyField>("compactWidthField");
            if (_compactWidth == null)
            {
                Debug.LogError("MaxTMProSizeEditor UXML is missing expected elements.");
                return;
            }

            SerializedProperty maxWidth = serializedObject.FindProperty("_maxWidth");
            root.TrackPropertyValue(maxWidth, _ => UpdateVisibility());
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            serializedObject.UpdateIfRequiredOrScript();
            SetVisible(_compactWidth, serializedObject.FindProperty("_maxWidth").intValue > 0);
        }

        private static void SetVisible(VisualElement element, bool visible) =>
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
