using Mane.Unity.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.UI.Editor
{
    /// <summary>
    /// Inspector for <see cref="ThreeStatesToggle"/>, including the shared <see cref="UIInteractionControl"/>.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ThreeStatesToggle), true)]
    public class ThreeStatesToggleEditor : ManeEditor
    {
        protected override void BuildInspector(VisualElement root)
        {
            UIInteractionControl interaction = root.Q<UIInteractionControl>("interaction");
            if (interaction == null)
            {
                Debug.LogError("ThreeStatesToggleEditor UXML is missing expected elements.");
                return;
            }

            interaction.Bind(serializedObject);
        }
    }
}
