using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
            tree.CloneTree(root);
            return root;
        }
    }
}
