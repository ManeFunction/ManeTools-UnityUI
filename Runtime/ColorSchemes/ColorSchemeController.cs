using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mane.Unity.UI
{
    [AddComponentMenu("Mane Tools/UI/Color Scheme Controller")]
    public class ColorSchemeController : UIBehaviour
    {
        [SerializeField] protected GraphicCollection[] _graphic;
        [SerializeField] private ColorScheme _colorScheme;
        
        public ColorScheme ColorScheme
        {
            get => _colorScheme;
            set
            {
                _colorScheme = value;
                Refresh();
            }
        }

        protected override void Awake() => Refresh();

        public void AnimateTo(ColorScheme colorScheme, float duration, AnimationCurve animationCurve = null)
        {
            if (duration <= 0f || _colorScheme == null)
            {
                ColorScheme = colorScheme;
                return;
            }

            if (colorScheme == null || _colorScheme.Length != colorScheme.Length)
                return;

            animationCurve ??= AnimationCurve.Linear(0f, 0f, 1f, 1f);

            SetColorSchemeWithoutRefresh(colorScheme);
            StopAllCoroutines();
            StartCoroutine(AnimateCoroutine(colorScheme, duration, animationCurve));
        }

        private IEnumerator AnimateCoroutine(ColorScheme targetScheme, float duration,
            AnimationCurve animationCurve)
        {
            float time = 0f;
            Color[] from = new Color[targetScheme.Length];
            for (int i = 0; i < targetScheme.Length; i++)
                from[i] = GetGraphicColor(i);

            while (time < duration)
            {
                float t = animationCurve.Evaluate(time / duration);
                for (int i = 0; i < targetScheme.Length; i++)
                    SetGraphicsColor(i, Color.Lerp(from[i], targetScheme[i], t));

                time += Time.deltaTime;
                yield return null;
            }

            for (int i = 0; i < targetScheme.Length; i++)
                SetGraphicsColor(i, targetScheme[i]);
        }

        public Color GetGraphicColor(int i)
        {
            if (_graphic == null || i < 0 || i >= _graphic.Length)
                return Color.white;

            GraphicCollection collection = _graphic[i];
            if (collection == null || collection.Length == 0)
                return Color.white;

            MaskableGraphic graphic = collection[0];
            return graphic ? graphic.color : Color.white;
        }

        public void SetGraphicsColor(int i, Color color)
        {
            if (_graphic == null || i < 0 || i >= _graphic.Length)
                return;

            GraphicCollection collection = _graphic[i];
            if (collection == null)
                return;

            for (int j = 0; j < collection.Length; j++)
            {
                MaskableGraphic graphic = collection[j];
                if (graphic)
                    graphic.color = color;
            }
        }
        
        public void SetColorSchemeWithoutRefresh(ColorScheme colorScheme) => 
            _colorScheme = colorScheme;

        public void Refresh()
        {
            if (_colorScheme == null || _graphic == null || _graphic.Length == 0)
                return;

            for (int i = 0; i < _graphic.Length; i++)
                RefreshColor(i);
        }

        private void RefreshColor(int i)
        {
            GraphicCollection collection = _graphic[i];
            if (collection == null)
                return;

            for (int j = 0; j < collection.Length; j++)
            {
                MaskableGraphic graphic = collection[j];
                if (graphic && i < _colorScheme.Length)
                {
#if UNITY_EDITOR
                    if (graphic.color != _colorScheme[i])
                        UnityEditor.EditorUtility.SetDirty(graphic);
#endif
                    graphic.color = _colorScheme[i];
                }
            }
        }


        [Serializable]
        public class GraphicCollection
        {
            [SerializeField] private MaskableGraphic[] _graphic;
            
            /// <summary>
            /// Graphic at <paramref name="index"/> in this slot.
            /// </summary>
            public MaskableGraphic this[int index] =>
                _graphic == null || index < 0 || index >= _graphic.Length ? null : _graphic[index];

            /// <summary>
            /// Number of graphics in this slot.
            /// </summary>
            public int Length => _graphic?.Length ?? 0;
        }


#if UNITY_EDITOR
        public const string GraphicPropertyName = nameof(_graphic);
        public const string ColorSchemePropertyName = nameof(_colorScheme);
#endif
    }
}
