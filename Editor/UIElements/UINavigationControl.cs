using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace Mane.Unity.UI.Editor
{
    [UxmlElement]
    public partial class UINavigationControl : VisualElement
    {
        public const string UssClassName = "mie-navigation-control";

        private const string SheetFileName = "UINavigationControl.uss";
        private const string DefaultBindingPath = "m_Navigation";

        private static StyleSheet _sheet;

        private readonly NavigationModeField _modeField;
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

        public UINavigationControl()
        {
            AddToClassList("mie-nested-block");
            AddToClassList(UssClassName);
            ApplySheet();

            _modeField = new NavigationModeField("Navigation");
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

            SetVisible(_wrapAround, false);
            SetVisible(_explicitNavigation, false);

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

        [UxmlAttribute("binding-path")]
        public string BindingPath
        {
            get => _bindingPath;
            set => _bindingPath = string.IsNullOrEmpty(value) ? DefaultBindingPath : value;
        }

        public void Bind(SerializedObject serializedObject)
        {
            if (serializedObject == null)
                return;

            _serializedObject = serializedObject;
            string path = BindingPath;
            BindField(_wrapAround, serializedObject, path + ".m_WrapAround");
            BindField(_selectOnUp, serializedObject, path + ".m_SelectOnUp");
            BindField(_selectOnDown, serializedObject, path + ".m_SelectOnDown");
            BindField(_selectOnLeft, serializedObject, path + ".m_SelectOnLeft");
            BindField(_selectOnRight, serializedObject, path + ".m_SelectOnRight");

            _modeProperty = serializedObject.FindProperty(path + ".m_Mode");
            if (_modeProperty == null)
            {
                Debug.LogError($"UINavigationControl could not find '{path}.m_Mode'.");
                return;
            }

            SyncMode();
            UpdateVisibility();

            if (_bound)
                return;

            _bound = true;
            _modeField.RegisterValueChangedCallback(evt =>
            {
                _modeProperty.intValue = (int)evt.newValue;
                _serializedObject.ApplyModifiedProperties();
                UpdateVisibility();
            });
            this.TrackPropertyValue(_modeProperty, _ =>
            {
                SyncMode();
                UpdateVisibility();
            });
        }

        private void SyncMode()
        {
            _serializedObject.UpdateIfRequiredOrScript();
            if (_modeProperty.hasMultipleDifferentValues)
            {
                _modeField.showMixedValue = true;
                return;
            }

            _modeField.showMixedValue = false;
            _modeField.SetValueWithoutNotify((Navigation.Mode)_modeProperty.intValue);
        }

        private void UpdateVisibility()
        {
            _serializedObject.UpdateIfRequiredOrScript();
            Navigation.Mode mode = (Navigation.Mode)_modeProperty.intValue;
            SetVisible(_wrapAround, mode is Navigation.Mode.Horizontal or Navigation.Mode.Vertical);
            SetVisible(_explicitNavigation, mode == Navigation.Mode.Explicit);
        }

        private void SyncVisualizeButton() =>
            _visualizeButton.EnableInClassList("mie-inline-button--on", SelectableNavigationVisualizer.Enabled);

        private void ApplySheet()
        {
            StyleSheet sheet = Sheet;
            if (sheet == null)
            {
                Debug.LogError("UINavigationControl.uss was not found next to UINavigationControl.");
                return;
            }

            if (!styleSheets.Contains(sheet))
                styleSheets.Add(sheet);
        }

        private static StyleSheet Sheet
        {
            get
            {
                if (_sheet != null)
                    return _sheet;

                string[] guids = AssetDatabase.FindAssets($"t:MonoScript {nameof(UINavigationControl)}");
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    if (script == null || script.GetClass() != typeof(UINavigationControl))
                        continue;

                    string folder = Path.GetDirectoryName(path);
                    if (string.IsNullOrEmpty(folder))
                        break;

                    _sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                        Path.Combine(folder, SheetFileName).Replace('\\', '/'));
                    break;
                }

                return _sheet;
            }
        }

        private static void BindField(PropertyField field, SerializedObject serializedObject, string path)
        {
            SerializedProperty property = serializedObject.FindProperty(path);
            if (property == null)
            {
                Debug.LogError($"UINavigationControl could not find '{path}'.");
                return;
            }

            field.BindProperty(property);
        }

        private static void SetVisible(VisualElement element, bool visible) =>
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
