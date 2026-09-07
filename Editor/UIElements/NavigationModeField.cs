using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Mane.Unity.UI.Editor
{
    [UxmlElement]
    public partial class NavigationModeField : BaseField<Navigation.Mode>
    {
        private const Navigation.Mode Horizontal = Navigation.Mode.Horizontal;
        private const Navigation.Mode Vertical = Navigation.Mode.Vertical;
        private const Navigation.Mode Automatic = Navigation.Mode.Automatic;
        private const Navigation.Mode Explicit = Navigation.Mode.Explicit;
        private const Navigation.Mode Everything = Automatic | Explicit;

        private readonly VisualElement _input;
        private readonly TextElement _text;
        private VisualElement _overlay;
        private VisualElement _menu;

        public NavigationModeField() : this(null) { }

        public NavigationModeField(string label) : this(label, new VisualElement()) { }

        private NavigationModeField(string label, VisualElement input) : base(label, input)
        {
            _input = input;
            AddToClassList(ussClassName);
            AddToClassList("unity-popup-field");
            AddToClassList("unity-base-popup-field");
            _input.AddToClassList("unity-base-popup-field__input");
            _input.pickingMode = PickingMode.Position;

            _text = new TextElement();
            _text.AddToClassList("unity-base-popup-field__text");
            _text.pickingMode = PickingMode.Ignore;
            _text.style.color = EditorStyles.label.normal.textColor;
            _input.Add(_text);

            VisualElement arrow = new();
            arrow.AddToClassList("unity-base-popup-field__arrow");
            arrow.pickingMode = PickingMode.Ignore;
            _input.Add(arrow);

            _input.RegisterCallback<PointerDownEvent>(OnInputPointerDown);
            RegisterCallback<AttachToPanelEvent>(_ => RefreshLabel());
            RegisterCallback<DetachFromPanelEvent>(_ => HideMenu());
            RefreshLabel();
        }

        public override void SetValueWithoutNotify(Navigation.Mode newValue)
        {
            base.SetValueWithoutNotify(newValue);
            RefreshLabel();
            if (_menu != null)
                RebuildMenuItems();
        }

        protected override void UpdateMixedValueContent() => RefreshLabel();

        private void RefreshLabel()
        {
            if (_text == null)
                return;

            _text.style.color = EditorStyles.label.normal.textColor;
            _text.text = showMixedValue ? "\u2014" : Format(value);
        }

        private void OnInputPointerDown(PointerDownEvent evt)
        {
            evt.StopImmediatePropagation();
            if (_menu == null)
                ShowMenu();
            else
                HideMenu();
        }

        private void ShowMenu()
        {
            VisualElement host = panel?.visualTree;
            if (host == null)
                return;

            HideMenu();

            _overlay = new VisualElement { name = "mie-navigation-overlay", pickingMode = PickingMode.Ignore };
            _overlay.style.position = Position.Absolute;
            _overlay.style.left = 0;
            _overlay.style.top = 0;
            _overlay.style.right = 0;
            _overlay.style.bottom = 0;
            CopyAncestorStyleSheets(_overlay);

            VisualElement backdrop = new() { pickingMode = PickingMode.Position };
            backdrop.AddToClassList("mie-navigation-menu-backdrop");
            backdrop.RegisterCallback<PointerDownEvent>(evt =>
            {
                evt.StopImmediatePropagation();
                HideMenu();
            });

            _menu = new VisualElement();
            _menu.AddToClassList("unity-base-dropdown");
            _menu.AddToClassList("mie-navigation-menu");
            _menu.pickingMode = PickingMode.Position;
            RebuildMenuItems();

            Rect inputBound = _input.worldBound;
            Rect hostBound = host.worldBound;
            _menu.style.position = Position.Absolute;
            _menu.style.left = inputBound.xMin - hostBound.xMin;
            _menu.style.top = inputBound.yMax - hostBound.yMin;
            _menu.style.minWidth = inputBound.width;

            _overlay.Add(backdrop);
            _overlay.Add(_menu);
            host.Add(_overlay);
        }

        private void RebuildMenuItems()
        {
            if (_menu == null)
                return;

            _menu.Clear();
            AddItem("None", value == Navigation.Mode.None, () => value = Navigation.Mode.None);
            AddItem("Everything", value == Everything, () => value = Everything);
            AddItem("Horizontal", HasBits(Horizontal), () => ToggleBits(Horizontal));
            AddItem("Vertical", HasBits(Vertical), () => ToggleBits(Vertical));
            AddItem("Automatic", HasBits(Automatic), () => ToggleBits(Automatic));
            AddItem("Explicit", HasBits(Explicit), () => ToggleBits(Explicit));
        }

        private void AddItem(string itemLabel, bool isChecked, Action apply)
        {
            VisualElement item = new();
            item.AddToClassList("unity-base-dropdown__item");
            if (isChecked)
                item.AddToClassList("unity-base-dropdown__item--checked");

            VisualElement checkmark = new();
            checkmark.AddToClassList("unity-base-dropdown__checkmark");
            checkmark.pickingMode = PickingMode.Ignore;
            checkmark.style.unityBackgroundImageTintColor = EditorStyles.label.normal.textColor;
            item.Add(checkmark);

            Label text = new(itemLabel);
            text.AddToClassList("unity-base-dropdown__label");
            text.pickingMode = PickingMode.Ignore;
            text.style.color = EditorStyles.label.normal.textColor;
            item.Add(text);

            item.RegisterCallback<PointerDownEvent>(evt =>
            {
                evt.StopImmediatePropagation();
                apply();
            });

            _menu.Add(item);
        }

        private void HideMenu()
        {
            _overlay?.RemoveFromHierarchy();
            _overlay = null;
            _menu = null;
        }

        private void CopyAncestorStyleSheets(VisualElement target)
        {
            for (VisualElement element = this; element != null; element = element.parent)
            {
                int count = element.styleSheets.count;
                for (int i = 0; i < count; i++)
                {
                    StyleSheet sheet = element.styleSheets[i];
                    if (sheet != null && !target.styleSheets.Contains(sheet))
                        target.styleSheets.Add(sheet);
                }
            }
        }

        private void ToggleBits(Navigation.Mode bits) =>
            value = HasBits(bits) ? value & ~bits : value | bits;

        private bool HasBits(Navigation.Mode bits) => (value & bits) == bits;

        private static string Format(Navigation.Mode value)
        {
            if (value == Navigation.Mode.None)
                return "None";
            if (value == Everything)
                return "Everything";
            if (value == Automatic)
                return "Automatic";
            if (value == Explicit)
                return "Explicit";
            if (value == Horizontal)
                return "Horizontal";
            if (value == Vertical)
                return "Vertical";
            if (value == (Explicit | Horizontal))
                return "Horizontal, Explicit";
            if (value == (Explicit | Vertical))
                return "Vertical, Explicit";

            return value.ToString();
        }
    }
}
