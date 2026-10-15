using Mane.Unity.Editor;
using UnityEditor;
using UnityEditor.UIElements;
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
        protected override void BuildInspector(VisualElement root)
        {
            SerializedProperty maxWidth = serializedObject.FindProperty("_maxWidth");
            root.Require<PropertyField>("compactWidthField")?.ShowWhen(() => maxWidth.floatValue > 0f, maxWidth);
        }
    }
}
