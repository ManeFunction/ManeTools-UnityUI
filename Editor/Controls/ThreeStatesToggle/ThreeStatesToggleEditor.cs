using Mane.Unity.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
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
        private VisualElement _wrapAround;
        private VisualElement _explicitNavigation;
        private Button _visualizeButton;

        protected override void BuildInspector(VisualElement root)
        {
            _targetGraphic = root.Q<PropertyField>("targetGraphic");
            _colorWarning = root.Q<VisualElement>("colorGraphicWarning");
            _spriteWarning = root.Q<VisualElement>("spriteGraphicWarning");
            _colors = root.Q<PropertyField>("colors");
            _spriteState = root.Q<PropertyField>("spriteState");
            _animationTriggers = root.Q<PropertyField>("animationTriggers");
            _wrapAround = root.Q<PropertyField>("wrapAround");
            _explicitNavigation = root.Q<VisualElement>("explicitNavigation");
            _visualizeButton = root.Q<Button>("visualizeNavigation");
            NavigationModeField navigationMode = root.Q<NavigationModeField>("navigationMode");

            if (_targetGraphic == null || _colorWarning == null || _spriteWarning == null
                || _colors == null || _spriteState == null || _animationTriggers == null
                || _wrapAround == null || _explicitNavigation == null
                || _visualizeButton == null || navigationMode == null)
            {
                Debug.LogError("ThreeStatesToggleEditor UXML is missing expected elements.");
                return;
            }

            SerializedProperty transition = serializedObject.FindProperty("m_Transition");
            SerializedProperty targetGraphic = serializedObject.FindProperty("m_TargetGraphic");
            SerializedProperty navigationModeProperty = serializedObject.FindProperty("m_Navigation.m_Mode");

            SyncNavigationMode(navigationMode, navigationModeProperty);
            navigationMode.RegisterValueChangedCallback(evt =>
            {
                navigationModeProperty.intValue = (int)evt.newValue;
                serializedObject.ApplyModifiedProperties();
                UpdateNavigationVisibility();
            });

            root.TrackPropertyValue(transition, _ => UpdateTransitionVisibility());
            root.TrackPropertyValue(targetGraphic, _ => UpdateTransitionVisibility());
            root.TrackPropertyValue(navigationModeProperty, _ =>
            {
                SyncNavigationMode(navigationMode, navigationModeProperty);
                UpdateNavigationVisibility();
            });
            UpdateTransitionVisibility();
            UpdateNavigationVisibility();

            SyncVisualizeButton();
            _visualizeButton.clicked += () =>
            {
                SelectableNavigationVisualizer.Enabled = !SelectableNavigationVisualizer.Enabled;
                SyncVisualizeButton();
            };

            root.RegisterCallback<AttachToPanelEvent>(_ => SelectableNavigationVisualizer.AddSubscriber());
            root.RegisterCallback<DetachFromPanelEvent>(_ => SelectableNavigationVisualizer.RemoveSubscriber());
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

        private void SyncNavigationMode(NavigationModeField field, SerializedProperty property)
        {
            serializedObject.UpdateIfRequiredOrScript();
            if (property.hasMultipleDifferentValues)
            {
                field.showMixedValue = true;
                return;
            }

            field.showMixedValue = false;
            field.SetValueWithoutNotify((Navigation.Mode)property.intValue);
        }

        private void UpdateNavigationVisibility()
        {
            serializedObject.UpdateIfRequiredOrScript();
            Navigation.Mode mode =
                (Navigation.Mode)serializedObject.FindProperty("m_Navigation.m_Mode").intValue;
            SetVisible(_wrapAround, mode is Navigation.Mode.Horizontal or Navigation.Mode.Vertical);
            SetVisible(_explicitNavigation, mode == Navigation.Mode.Explicit);
        }

        private void SyncVisualizeButton() =>
            _visualizeButton.EnableInClassList("mie-inline-button--on", SelectableNavigationVisualizer.Enabled);

        private static void SetVisible(VisualElement element, bool visible) =>
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
