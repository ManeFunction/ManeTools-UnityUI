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
    public class MaxTMProSize : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private LayoutElement _layoutElement;

        [SerializeField] private int _maxWidth;
        [SerializeField] private int _maxHeight;

        private string _oldValue = string.Empty;


        /// <summary>
        /// Max preferred width in pixels. Zero is unlimited.
        /// </summary>
        public int MaxWidth
        {
            get => _maxWidth;
            set
            {
                _maxWidth = Mathf.Max(0, value);
                ReCalculateLayout();
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
                ReCalculateLayout();
            }
        }


#if UNITY_EDITOR
        protected void Reset()
        {
            _text = gameObject.GetOrAddComponent<TextMeshProUGUI>();
            _layoutElement = gameObject.GetOrAddComponent<LayoutElement>();

            ReCalculateLayout();
        }

        protected void OnValidate()
        {
            _maxWidth = Mathf.Max(0, _maxWidth);
            _maxHeight = Mathf.Max(0, _maxHeight);
            ReCalculateLayout();
        }
#endif

        protected void Update()
        {
            if (!_text || _text.text == _oldValue) return;

            _oldValue = _text.text;
            ReCalculateLayout();
        }

        private void ReCalculateLayout()
        {
            if (!_text || !_layoutElement)
                return;

            _layoutElement.preferredWidth = _maxWidth > 0 ? Mathf.Min(_text.preferredWidth, _maxWidth) : -1f;
            _layoutElement.preferredHeight = _maxHeight > 0 ? Mathf.Min(_text.preferredHeight, _maxHeight) : -1f;
        }
    }
}