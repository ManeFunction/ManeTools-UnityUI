using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mane.Unity.UI
{
    /// <summary>
    /// Applies a <see cref="ColorScheme"/> to grouped <see cref="MaskableGraphic"/>s.
    /// Slot i maps to scheme color i.
    /// </summary>
    [AddComponentMenu("Mane Tools/UI/Color Scheme Controller")]
    public class ColorSchemeController : UIBehaviour
    {
        [SerializeField] protected GraphicCollection[] _graphic;
        [SerializeField] private ColorScheme _colorScheme;

        private Coroutine _animation;
        
        /// <summary>
        /// Active palette. Setting it stops a running <see cref="AnimateTo"/> and applies colors immediately.
        /// </summary>
        public ColorScheme ColorScheme
        {
            get => _colorScheme;
            set
            {
                StopAnimation();
                _colorScheme = value;
                Refresh();
            }
        }

        // The scheme is assigned when an animation starts, so re-applying it here also finishes
        // an animation that was cut short by deactivation.
        protected override void OnEnable()
        {
            base.OnEnable();
            Refresh();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // Unity has already stopped the coroutine.
            _animation = null;
        }

        /// <summary>
        /// Lerps graphic colors toward <paramref name="colorScheme"/> over <paramref name="duration"/> seconds.
        /// Snaps immediately when duration is not positive, there is no current scheme, or the controller
        /// is inactive. Does nothing when the target is null or the color counts differ.
        /// </summary>
        public void AnimateTo(ColorScheme colorScheme, float duration, AnimationCurve animationCurve = null)
        {
            if (duration <= 0f || _colorScheme == null || !isActiveAndEnabled)
            {
                ColorScheme = colorScheme;
                return;
            }

            if (colorScheme == null || _colorScheme.Length != colorScheme.Length)
                return;

            animationCurve ??= AnimationCurve.Linear(0f, 0f, 1f, 1f);

            StopAnimation();
            SetColorSchemeWithoutRefresh(colorScheme);
            _animation = StartCoroutine(AnimateCoroutine(colorScheme, duration, animationCurve));
        }

        private void StopAnimation()
        {
            if (_animation == null)
                return;

            StopCoroutine(_animation);
            _animation = null;
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

            _animation = null;
        }

        /// <summary>
        /// Color of the first graphic in slot <paramref name="i"/>, or white if none.
        /// </summary>
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

        /// <summary>
        /// Sets every graphic in slot <paramref name="i"/> to <paramref name="color"/>.
        /// </summary>
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
        
        /// <summary>
        /// Assigns the palette without applying colors. Used while animating.
        /// </summary>
        public void SetColorSchemeWithoutRefresh(ColorScheme colorScheme) => 
            _colorScheme = colorScheme;

        /// <summary>
        /// Applies the current scheme colors to all graphic slots.
        /// </summary>
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


        /// <summary>
        /// Graphics that share one scheme color.
        /// </summary>
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
        internal const string GraphicPropertyName = nameof(_graphic);
        internal const string ColorSchemePropertyName = nameof(_colorScheme);
#endif
    }
}
