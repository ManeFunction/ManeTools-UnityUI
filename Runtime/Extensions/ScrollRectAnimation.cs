using UnityEngine;
using UnityEngine.UI;

namespace Mane.Unity.UI
{
    /// <summary>
    /// One <see cref="ScrollRectExtensions.ScrollTo"/> movement, stepped by its coroutine.
    /// </summary>
    internal sealed class ScrollRectAnimation
    {
        // Larger than float noise, smaller than any drag.
        private const float MovedTolerance = .01f;

        private readonly Vector2 _start;
        private readonly Vector2 _target;
        private readonly Vector2 _min;
        private readonly Vector2 _max;
        private readonly bool _moveX;
        private readonly bool _moveY;
        private readonly AnimationCurve _easing;

        private float _elapsed;
        private Vector2 _last;

        public ScrollRectAnimation(ScrollRect scroll, Vector2 target, Vector2 min, Vector2 max,
            bool moveX, bool moveY, float duration, AnimationCurve easing)
        {
            Scroll = scroll;
            _start = _last = scroll.content.anchoredPosition;
            _target = target;
            _min = min;
            _max = max;
            _moveX = moveX;
            _moveY = moveY;
            Duration = duration;
            _easing = easing;
        }

        public ScrollRect Scroll { get; }

        /// <summary>
        /// Seconds from start to target.
        /// </summary>
        public float Duration { get; }

        /// <summary>
        /// The coroutine that steps this animation.
        /// </summary>
        public Coroutine Coroutine { get; set; }

        /// <summary>
        /// Advances by <paramref name="deltaTime"/> seconds. Returns false when the target is reached, or when
        /// something else moved the content since the last step (a drag, a snap); then it does not write.
        /// </summary>
        public bool Step(float deltaTime)
        {
            RectTransform content = Scroll != null ? Scroll.content : null;
            if (content == null || MovedElsewhere(content.anchoredPosition))
                return false;

            _elapsed += deltaTime;
            float t = Duration > 0f ? Mathf.Clamp01(_elapsed / Duration) : 1f;
            // The last step lands exactly on the target, whatever the last key of the curve is.
            float weight = t >= 1f ? 1f : _easing?.Evaluate(t) ?? t;

            // Overshooting curves may pass the target, but not the content edges, where the ScrollRect would pull back.
            Vector2 position = Vector2.LerpUnclamped(_start, _target, weight);
            position = Vector2.Max(_min, Vector2.Min(_max, position));

            ScrollRectExtensions.MoveContent(Scroll, position, _moveX, _moveY);
            _last = content.anchoredPosition;
            return t < 1f;
        }

        private bool MovedElsewhere(Vector2 position) =>
            (_moveX && Mathf.Abs(position.x - _last.x) > MovedTolerance)
            || (_moveY && Mathf.Abs(position.y - _last.y) > MovedTolerance);
    }
}
