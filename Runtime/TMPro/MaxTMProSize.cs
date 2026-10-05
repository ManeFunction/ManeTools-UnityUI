using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mane.Unity.UI
{
    /// <summary>
    /// Caps a <see cref="TextMeshProUGUI"/> preferred size via <see cref="LayoutElement"/>.
    /// Zero means no cap on that axis.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI))]
    [RequireComponent(typeof(LayoutElement))]
    [AddComponentMenu("Mane Tools/UI/Max TMPro Size")]
    public class MaxTMProSize : MonoBehaviour, ILayoutElement
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private LayoutElement _layoutElement;

        [SerializeField] private int _maxWidth;
        [SerializeField] private int _maxHeight;


        /// <summary>
        /// Max preferred width in pixels. Zero is unlimited.
        /// </summary>
        public int MaxWidth
        {
            get => _maxWidth;
            set
            {
                _maxWidth = Mathf.Max(0, value);
                MarkLayoutDirty();
            }
        }

        /// <summary>
        /// Max preferred height in pixels. Zero is unlimited.
        /// </summary>
        public int MaxHeight
        {
            get => _maxHeight;
            set
            {
                _maxHeight = Mathf.Max(0, value);
                MarkLayoutDirty();
            }
        }


        // This component only writes into the sibling LayoutElement, it has no layout opinion of its own.
        // Negative values are ignored by LayoutUtility.
        float ILayoutElement.minWidth => -1f;
        float ILayoutElement.preferredWidth => -1f;
        float ILayoutElement.flexibleWidth => -1f;
        float ILayoutElement.minHeight => -1f;
        float ILayoutElement.preferredHeight => -1f;
        float ILayoutElement.flexibleHeight => -1f;
        int ILayoutElement.layoutPriority => 0;


#if UNITY_EDITOR
        protected void Reset()
        {
            _text = gameObject.GetOrAddComponent<TextMeshProUGUI>();
            _layoutElement = gameObject.GetOrAddComponent<LayoutElement>();

            MarkLayoutDirty();
        }

        protected void OnValidate()
        {
            _maxWidth = Mathf.Max(0, _maxWidth);
            _maxHeight = Mathf.Max(0, _maxHeight);
            MarkLayoutDirty();
        }
#endif

        protected void OnEnable() => MarkLayoutDirty();

        void ILayoutElement.CalculateLayoutInputHorizontal()
        {
            if (!_text || !_layoutElement)
                return;

            _layoutElement.preferredWidth = _maxWidth > 0 ? Mathf.Min(_text.preferredWidth, _maxWidth) : -1f;
        }

        void ILayoutElement.CalculateLayoutInputVertical()
        {
            if (!_text || !_layoutElement)
                return;

            // By now the horizontal pass has set the final width, so TMP wraps at the width it is going to have.
            _layoutElement.preferredHeight = _maxHeight > 0 ? Mathf.Min(_text.preferredHeight, _maxHeight) : -1f;
        }

        private void MarkLayoutDirty()
        {
            if (transform is RectTransform rectTransform)
                LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
    }
}
