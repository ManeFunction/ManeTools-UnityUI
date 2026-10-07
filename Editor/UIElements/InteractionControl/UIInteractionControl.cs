using Mane.Unity.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace Mane.Unity.UI.Editor
{
    /// <summary>
    /// Foldable "Interaction" inspector block for a <see cref="Selectable"/>: interactable flag,
    /// transition fields with their warnings, and a <see cref="UINavigationControl"/>.
    /// </summary>
    [UxmlElement]
    public sealed partial class UIInteractionControl : VisualElement
    {
        /// <summary>
        /// USS class name for this control.
        /// </summary>
        public const string UssClassName = "mie-interaction-control";

        private readonly FoldoutBlock _foldout;
        private readonly PropertyField _interactable;
        private readonly PropertyField _transition;
        private readonly PropertyField _targetGraphic;
        private readonly PropertyField _colors;
        private readonly PropertyField _spriteState;
        private readonly PropertyField _animationTriggers;
        private readonly VisualElement _colorWarning;
        private readonly VisualElement _spriteWarning;
        private readonly UINavigationControl _navigation;

        private SerializedObject _serializedObject;
        private SerializedProperty _transitionProperty;
        private SerializedProperty _targetGraphicProperty;
        private bool _bound;

        /// <summary>
        /// Builds the foldout with interactable, transition and navigation fields.
        /// </summary>
        public UIInteractionControl()
        {
            AddToClassList(UssClassName);

            _foldout = new FoldoutBlock { name = "interaction", text = "Interaction", CollapsedByDefault = true };
            Add(_foldout);

            _interactable = new PropertyField { label = "Interactable" };
            _foldout.Add(_interactable);

            VisualElement transitionBlock = new();
            transitionBlock.AddToClassList("mie-nested-block");
            _transition = new PropertyField { label = "Transition" };
            _targetGraphic = new PropertyField { label = "Target Graphic" };
            _colorWarning = InfoBoxDrawer.Create(
                "You must have a Graphic target in order to use a color transition.",
                InfoBoxType.Warning);
            _spriteWarning = InfoBoxDrawer.Create(
                "You must have an Image target in order to use a sprite swap transition.",
                InfoBoxType.Warning);
            _colors = new PropertyField { label = "Colors" };
            _spriteState = new PropertyField { label = "Sprite State" };
            _animationTriggers = new PropertyField { label = "Animation Triggers" };
            transitionBlock.Add(_transition);
            transitionBlock.Add(_targetGraphic);
            transitionBlock.Add(_colorWarning);
            transitionBlock.Add(_spriteWarning);
            transitionBlock.Add(_colors);
            transitionBlock.Add(_spriteState);
            transitionBlock.Add(_animationTriggers);
            _foldout.Add(transitionBlock);

            _navigation = new UINavigationControl();
            _foldout.Add(_navigation);

            SetVisible(_targetGraphic, false);
            SetVisible(_colors, false);
            SetVisible(_spriteState, false);
            SetVisible(_animationTriggers, false);
            SetVisible(_colorWarning, false);
            SetVisible(_spriteWarning, false);
        }

        /// <summary>
        /// Initial collapsed state when no stored value exists. Defaults to <c>true</c>.
        /// </summary>
        [UxmlAttribute("collapsed-by-default")]
        public bool CollapsedByDefault
        {
            get => _foldout.CollapsedByDefault;
            set => _foldout.CollapsedByDefault = value;
        }

        /// <summary>
        /// The inner navigation block, for further customization.
        /// </summary>
        public UINavigationControl Navigation => _navigation;

        /// <summary>
        /// Binds all fields to the standard <see cref="Selectable"/> serialized properties
        /// (<c>m_Interactable</c>, <c>m_Transition</c>, <c>m_Navigation</c>, ...).
        /// </summary>
        public void Bind(SerializedObject serializedObject)
        {
            if (serializedObject == null)
                return;

            _serializedObject = serializedObject;
            BindField(_interactable, serializedObject, "m_Interactable");
            BindField(_transition, serializedObject, "m_Transition");
            BindField(_targetGraphic, serializedObject, "m_TargetGraphic");
            BindField(_colors, serializedObject, "m_Colors");
            BindField(_spriteState, serializedObject, "m_SpriteState");
            BindField(_animationTriggers, serializedObject, "m_AnimationTriggers");
            _navigation.Bind(serializedObject);

            _transitionProperty = serializedObject.FindProperty("m_Transition");
            _targetGraphicProperty = serializedObject.FindProperty("m_TargetGraphic");
            if (_transitionProperty == null || _targetGraphicProperty == null)
                return;

            UpdateTransitionVisibility();

            if (_bound)
                return;

            _bound = true;
            this.TrackPropertyValue(_transitionProperty, _ => UpdateTransitionVisibility());
            this.TrackPropertyValue(_targetGraphicProperty, _ => UpdateTransitionVisibility());
        }

        private void UpdateTransitionVisibility()
        {
            _serializedObject.UpdateIfRequiredOrScript();
            Selectable.Transition transition = (Selectable.Transition)_transitionProperty.enumValueIndex;
            Graphic graphic = _targetGraphicProperty.objectReferenceValue as Graphic;

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

        private static void BindField(PropertyField field, SerializedObject serializedObject, string path)
        {
            SerializedProperty property = serializedObject.FindProperty(path);
            if (property == null)
            {
                Debug.LogError($"UIInteractionControl could not find '{path}'.");
                return;
            }

            field.BindProperty(property);
        }

        private static void SetVisible(VisualElement element, bool visible) =>
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
