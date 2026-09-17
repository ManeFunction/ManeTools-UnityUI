using UnityEngine;

namespace Mane.Unity.UI
{
    /// <summary>
    /// Named palette of colors for <see cref="ColorSchemeController"/>.
    /// </summary>
    [ManeStyle]
    [CreateAssetMenu(fileName = "ColorScheme", menuName = "Mane Tools/Color Scheme")]
    public class ColorScheme : ScriptableObject
    {
        [SerializeField] private Color[] _colors = { Color.white };

        /// <summary>
        /// Number of colors in the palette.
        /// </summary>
        public int Length => _colors?.Length ?? 0;

        /// <summary>
        /// Color at <paramref name="index"/>, or white if the index is out of range.
        /// </summary>
        public Color this[int index] =>
            _colors == null || index < 0 || index >= _colors.Length ? Color.white : _colors[index];

#if UNITY_EDITOR
        public const string ColorsPropertyName = nameof(_colors);
#endif
    }
}