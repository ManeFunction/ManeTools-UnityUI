using UnityEngine;

namespace Mane.Unity.UI
{
    [ManeStyle]
    [CreateAssetMenu(fileName = "ColorScheme", menuName = "Mane Tools/Color Scheme")]
    public class ColorScheme : ScriptableObject
    {
        [SerializeField] private Color[] _colors = { Color.white };

        public int Length => _colors?.Length ?? 0;

        public Color this[int index] =>
            _colors == null || index < 0 || index >= _colors.Length ? Color.white : _colors[index];

#if UNITY_EDITOR
        public const string ColorsPropertyName = nameof(_colors);
#endif
    }
}