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
        // Same value TMP uses internally for an unbounded axis.
        private const float UnlimitedHeight = 32767f;


        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private LayoutElement _layoutElement;

        [SerializeField] private float _maxWidth;
        [SerializeField] private float _maxHeight;
        [SerializeField] private bool _compactWidth;


        /// <summary>
        /// Max preferred width in pixels. Zero is unlimited.
        /// </summary>
        public float MaxWidth
        {
            get => _maxWidth;
            set
            {
                _maxWidth = Mathf.Max(0f, value);
                MarkLayoutDirty();
            }
        }

        /// <summary>
        /// Max preferred height in pixels. Zero is unlimited.
        /// </summary>
        public float MaxHeight
        {
            get => _maxHeight;
            set
            {
                _maxHeight = Mathf.Max(0f, value);
                MarkLayoutDirty();
            }
        }


        /// <summary>
        /// When the text wraps at <see cref="MaxWidth"/>, shrink the width to the widest line
        /// instead of keeping the full <see cref="MaxWidth"/>. Has no effect when <see cref="MaxWidth"/> is zero.
        /// </summary>
        public bool CompactWidth
        {
            get => _compactWidth;
            set
            {
                _compactWidth = value;
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
            _maxWidth = Mathf.Max(0f, _maxWidth);
            _maxHeight = Mathf.Max(0f, _maxHeight);
            MarkLayoutDirty();
        }
#endif

        protected void OnEnable() => MarkLayoutDirty();

        void ILayoutElement.CalculateLayoutInputHorizontal()
        {
            if (!_text || !_layoutElement)
                return;

            _layoutElement.preferredWidth = _maxWidth > 0 ? GetCappedWidth() : -1f;
        }

        void ILayoutElement.CalculateLayoutInputVertical()
        {
            if (!_text || !_layoutElement)
                return;

            // By now the horizontal pass has set the final width, so TMP wraps at the width it is going to have.
            _layoutElement.preferredHeight = _maxHeight > 0 ? Mathf.Min(_text.preferredHeight, _maxHeight) : -1f;
        }

        /// <summary>
        /// The text width if it fits in <see cref="MaxWidth"/>, otherwise <see cref="MaxWidth"/>, or the width
        /// of the widest line after wrapping at <see cref="MaxWidth"/> with <see cref="CompactWidth"/>,
        /// so a wrapped word does not leave an empty gap on the right.
        /// </summary>
        private float GetCappedWidth()
        {
            // Unwrapped width: TMP measures preferred width as a single line.
            float singleLine = _text.preferredWidth;
            if (singleLine <= _maxWidth)
                return singleLine;

            if (!_compactWidth)
                return _maxWidth;

            // The text area excludes the component margins, but the returned width includes them.
            Vector4 margin = _text.margin;
            float textArea = _maxWidth - Mathf.Max(0f, margin.x) - Mathf.Max(0f, margin.z);

            float wrapped = _text.GetPreferredValues(_text.text, textArea, UnlimitedHeight).x;
            return Mathf.Min(wrapped, _maxWidth);
        }

        private void MarkLayoutDirty()
        {
            if (transform is RectTransform rectTransform)
                LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
    }
}
