using Mane.Unity.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace Mane.Unity.UI.Editor
{
    /// <summary>
    /// Inspector block for a Selectable's <see cref="Navigation"/>: mode, wrap-around,
    /// explicit targets, and a visualize toggle.
    /// </summary>
    [UxmlElement]
    public sealed partial class UINavigationControl : VisualElement
    {
        /// <summary>
        /// USS class name for this control.
        /// </summary>
        public const string UssClassName = "mie-navigation-control";

        private const string DefaultBindingPath = "m_Navigation";

        private readonly PropertyField _modeField;
        private readonly PropertyField _wrapAround;
        private readonly PropertyField _selectOnUp;
        private readonly PropertyField _selectOnDown;
        private readonly PropertyField _selectOnLeft;
        private readonly PropertyField _selectOnRight;
        private readonly VisualElement _explicitNavigation;
        private readonly Button _visualizeButton;

        private SerializedObject _serializedObject;
        private SerializedProperty _modeProperty;
        private string _bindingPath = DefaultBindingPath;
        private bool _bound;

        /// <summary>
        /// Builds the navigation fields and visualize button.
        /// </summary>
        public UINavigationControl()
        {
            AddToClassList("mie-nested-block");
            AddToClassList(UssClassName);
            UIElementsTools.AddStyleSheet(this, typeof(UINavigationControl));

            // Navigation.Mode is [Flags]: the bound field is Unity's flags popup, as in the built-in Selectable inspector.
            _modeField = new PropertyField { label = "Navigation" };
            Add(_modeField);

            _wrapAround = new PropertyField { label = "Wrap Around" };
            Add(_wrapAround);

            _selectOnUp = new PropertyField { label = "Select On Up" };
            _selectOnDown = new PropertyField { label = "Select On Down" };
            _selectOnLeft = new PropertyField { label = "Select On Left" };
            _selectOnRight = new PropertyField { label = "Select On Right" };
            _explicitNavigation = new VisualElement();
            _explicitNavigation.Add(_selectOnUp);
            _explicitNavigation.Add(_selectOnDown);
            _explicitNavigation.Add(_selectOnLeft);
            _explicitNavigation.Add(_selectOnRight);
            Add(_explicitNavigation);

            _wrapAround.SetVisible(false);
            _explicitNavigation.SetVisible(false);

            VisualElement row = new();
            row.AddToClassList("mie-inline-button-row");
            row.AddToClassList("unity-base-field");

            Label spacer = new();
            spacer.AddToClassList("unity-base-field__label");
            row.Add(spacer);

            _visualizeButton = new Button { text = "Visualize", tooltip = "Show navigation flows between selectable UI elements." };
            _visualizeButton.AddToClassList("mie-button");
            _visualizeButton.AddToClassList("mie-inline-button");
            _visualizeButton.AddToClassList("unity-base-field__input");
            row.Add(_visualizeButton);
            Add(row);

            SyncVisualizeButton();
            _visualizeButton.clicked += () =>
            {
                SelectableNavigationVisualizer.Enabled = !SelectableNavigationVisualizer.Enabled;
                SyncVisualizeButton();
            };

            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                SelectableNavigationVisualizer.AddSubscriber();
                SelectableNavigationVisualizer.EnabledChanged += SyncVisualizeButton;
                SyncVisualizeButton();
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                SelectableNavigationVisualizer.EnabledChanged -= SyncVisualizeButton;
                SelectableNavigationVisualizer.RemoveSubscriber();
            });
        }

        /// <summary>
        /// Serialized path to the <see cref="Navigation"/> struct. Defaults to <c>m_Navigation</c>.
        /// </summary>
        [UxmlAttribute("binding-path")]
        public string BindingPath
        {
            get => _bindingPath;
            set => _bindingPath = string.IsNullOrEmpty(value) ? DefaultBindingPath : value;
        }

        /// <summary>
        /// Binds this control to <paramref name="serializedObject"/> at <see cref="BindingPath"/>.
        /// </summary>
        public void Bind(SerializedObject serializedObject)
        {
            if (serializedObject == null)
                return;

            _serializedObject = serializedObject;
            string path = BindingPath;
            _modeField.BindField(serializedObject, path + ".m_Mode");
            _wrapAround.BindField(serializedObject, path + ".m_WrapAround");
            _selectOnUp.BindField(serializedObject, path + ".m_SelectOnUp");
            _selectOnDown.BindField(serializedObject, path + ".m_SelectOnDown");
            _selectOnLeft.BindField(serializedObject, path + ".m_SelectOnLeft");
            _selectOnRight.BindField(serializedObject, path + ".m_SelectOnRight");

            _modeProperty = serializedObject.FindProperty(path + ".m_Mode");
            if (_modeProperty == null)
                return;

            UpdateVisibility();

            if (_bound)
                return;

            _bound = true;
            this.TrackPropertyValue(_modeProperty, _ => UpdateVisibility());
        }

        private void UpdateVisibility()
        {
            _serializedObject.UpdateIfRequiredOrScript();
            Navigation.Mode mode = (Navigation.Mode)_modeProperty.intValue;
            _wrapAround.SetVisible(mode is Navigation.Mode.Horizontal or Navigation.Mode.Vertical);
            _explicitNavigation.SetVisible(mode == Navigation.Mode.Explicit);
        }

        private void SyncVisualizeButton() =>
            _visualizeButton.EnableInClassList("mie-inline-button--on", SelectableNavigationVisualizer.Enabled);
    }
}
