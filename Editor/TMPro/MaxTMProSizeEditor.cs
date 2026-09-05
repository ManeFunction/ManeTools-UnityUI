using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Mane.Unity.Editor;

namespace Mane.Unity.UI.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MaxTMProSize), true)]
    public class MaxTMProSizeEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset xml;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();
            ManeEditorStyles.Apply(root);
            xml.CloneTree(root);
            return root;
        }
    }
}
