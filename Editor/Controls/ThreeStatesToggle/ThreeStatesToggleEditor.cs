using Mane.Unity;
using Mane.Unity.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace Mane.Unity.UI.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ThreeStatesToggle), true)]
    public class ThreeStatesToggleEditor : ManeEditor
    {
        private VisualElement _targetGraphic;
        private VisualElement _colorWarning;
        private VisualElement _spriteWarning;
        private VisualElement _colors;
        private VisualElement _spriteState;
        private VisualElement _animationTriggers;

        protected override void BuildInspector(VisualElement root)
        {
            _targetGraphic = root.Q<PropertyField>("targetGraphic");
            _colors = root.Q<PropertyField>("colors");
            _spriteState = root.Q<PropertyField>("spriteState");
            _animationTriggers = root.Q<PropertyField>("animationTriggers");
            UINavigationControl navigation = root.Q<UINavigationControl>("navigation");

            if (_targetGraphic == null || _colors == null || _spriteState == null
                || _animationTriggers == null || navigation == null)
            {
                Debug.LogError("ThreeStatesToggleEditor UXML is missing expected elements.");
                return;
            }

            VisualElement transitionBlock = _targetGraphic.parent;
            int graphicIndex = transitionBlock.IndexOf(_targetGraphic);

            _colorWarning = InfoBoxDrawer.Create(
                "You must have a Graphic target in order to use a color transition.",
                InfoBoxType.Warning);
            _colorWarning.name = "colorGraphicWarning";
            _spriteWarning = InfoBoxDrawer.Create(
                "You must have an Image target in order to use a sprite swap transition.",
                InfoBoxType.Warning);
            _spriteWarning.name = "spriteGraphicWarning";
            transitionBlock.Insert(graphicIndex + 1, _colorWarning);
            transitionBlock.Insert(graphicIndex + 2, _spriteWarning);

            navigation.Bind(serializedObject);

            SerializedProperty transition = serializedObject.FindProperty("m_Transition");
            SerializedProperty targetGraphic = serializedObject.FindProperty("m_TargetGraphic");
            root.TrackPropertyValue(transition, _ => UpdateTransitionVisibility());
            root.TrackPropertyValue(targetGraphic, _ => UpdateTransitionVisibility());
            UpdateTransitionVisibility();
        }

        private void UpdateTransitionVisibility()
        {
            serializedObject.UpdateIfRequiredOrScript();
            Selectable.Transition transition =
                (Selectable.Transition)serializedObject.FindProperty("m_Transition").enumValueIndex;
            Graphic graphic = serializedObject.FindProperty("m_TargetGraphic").objectReferenceValue as Graphic;

            bool colorTint = transition == Selectable.Transition.ColorTint;
            bool spriteSwap = transition == Selectable.Transition.SpriteSwap;
            bool animation = transition == Selectable.Transition.Animation;

            SetVisible(_targetGraphic, colorTint || spriteSwap);
            SetVisible(_colors, colorTint);
            SetVisible(_spriteState, spriteSwap);
            SetVisible(_animationTriggers, animation);
            SetVisible(_colorWarning, colorTint && graphic == null);
            SetVisible(_spriteWarning, spriteSwap && graphic is not Image);
        }

        private static void SetVisible(VisualElement element, bool visible) =>
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
