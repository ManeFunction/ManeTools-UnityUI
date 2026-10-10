using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mane.Unity.UI
{
    /// <summary>
    /// Caps a <see cref="TextMeshProUGUI"/> preferred size. Acts as a layout element with
    /// layout priority 2, so the capped value overrides the text and a default <see cref="LayoutElement"/>.
    /// Zero means no cap on that axis.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI))]
    [AddComponentMenu("Mane Tools/UI/Max TMPro Size")]
    public class MaxTMProSize : MonoBehaviour, ILayoutElement
    {
        // Same value TMP uses internally for an unbounded axis.
        private const float UnlimitedHeight = 32767f;

        // Above TextMeshProUGUI (0) and a default LayoutElement (1).
        private const int LayoutPriority = 2;


        [SerializeField] private TextMeshProUGUI _text;

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


        // Capped values from the last layout pass. Negative values (no cap) are ignored by LayoutUtility,
        // so the text's own preferred size, or a LayoutElement's, applies.
        private float _preferredWidth = -1f;
        private float _preferredHeight = -1f;

        float ILayoutElement.minWidth => -1f;
        float ILayoutElement.preferredWidth => _preferredWidth;
        float ILayoutElement.flexibleWidth => -1f;
        float ILayoutElement.minHeight => -1f;
        float ILayoutElement.preferredHeight => _preferredHeight;
        float ILayoutElement.flexibleHeight => -1f;
        int ILayoutElement.layoutPriority => LayoutPriority;


#if UNITY_EDITOR
        protected void Reset()
        {
            _text = gameObject.GetOrAddComponent<TextMeshProUGUI>();

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

        // The layout skips disabled components, so the parent only has to recalculate without the caps.
        protected void OnDisable() => MarkLayoutDirty();

        void ILayoutElement.CalculateLayoutInputHorizontal() =>
            _preferredWidth = _text && _maxWidth > 0 ? GetCappedWidth() : -1f;

        // By now the horizontal pass has set the final width, so TMP wraps at the width it is going to have.
        void ILayoutElement.CalculateLayoutInputVertical() =>
            _preferredHeight = _text && _maxHeight > 0 ? Mathf.Min(_text.preferredHeight, _maxHeight) : -1f;

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
