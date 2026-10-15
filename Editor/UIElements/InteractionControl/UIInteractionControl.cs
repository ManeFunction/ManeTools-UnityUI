using Mane.Unity.Editor;
using UnityEditor;
using UnityEditor.UIElements;
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

            _targetGraphic.SetVisible(false);
            _colors.SetVisible(false);
            _spriteState.SetVisible(false);
            _animationTriggers.SetVisible(false);
            _colorWarning.SetVisible(false);
            _spriteWarning.SetVisible(false);
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
        /// The transition fields and warnings follow the first object it is bound to.
        /// </summary>
        public void Bind(SerializedObject serializedObject)
        {
            if (serializedObject == null)
                return;

            _interactable.BindField(serializedObject, "m_Interactable");
            _transition.BindField(serializedObject, "m_Transition");
            _targetGraphic.BindField(serializedObject, "m_TargetGraphic");
            _colors.BindField(serializedObject, "m_Colors");
            _spriteState.BindField(serializedObject, "m_SpriteState");
            _animationTriggers.BindField(serializedObject, "m_AnimationTriggers");
            _navigation.Bind(serializedObject);

            SerializedProperty transition = serializedObject.FindProperty("m_Transition");
            SerializedProperty targetGraphic = serializedObject.FindProperty("m_TargetGraphic");
            if (_bound || transition == null || targetGraphic == null)
                return;

            _bound = true;
            _targetGraphic.ShowWhen(() => Is(Selectable.Transition.ColorTint) || Is(Selectable.Transition.SpriteSwap),
                transition);
            _colors.ShowWhen(() => Is(Selectable.Transition.ColorTint), transition);
            _spriteState.ShowWhen(() => Is(Selectable.Transition.SpriteSwap), transition);
            _animationTriggers.ShowWhen(() => Is(Selectable.Transition.Animation), transition);
            _colorWarning.ShowWhen(() => Is(Selectable.Transition.ColorTint)
                                         && targetGraphic.objectReferenceValue is not Graphic,
                transition, targetGraphic);
            _spriteWarning.ShowWhen(() => Is(Selectable.Transition.SpriteSwap)
                                          && targetGraphic.objectReferenceValue is not Image,
                transition, targetGraphic);

            bool Is(Selectable.Transition value) => (Selectable.Transition)transition.enumValueIndex == value;
        }
    }
}
