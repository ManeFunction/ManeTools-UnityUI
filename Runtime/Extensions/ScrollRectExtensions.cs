using System.Collections;
using System.Runtime.CompilerServices;
using Mane.DotNet;
using UnityEngine;
using UnityEngine.UI;

namespace Mane.Unity.UI
{
    /// <summary>
    /// Helpers to move a <see cref="ScrollRect"/> so a child is in view, at once or animated.
    /// </summary>
    public static class ScrollRectExtensions
    {
        private const float MinDistance = .01f;

        private static readonly MinMaxFloat DefaultDurationLimits = new(.25f, .8f);

        private static readonly Vector3[] Corners = new Vector3[4];
        private static readonly ConditionalWeakTable<ScrollRect, ScrollRectAnimation> Running = new();

        /// <summary>
        /// Scrolls so <paramref name="item"/> is centered in the viewport, shifted by <paramref name="offset"/>.
        /// Unless the movement type is <see cref="ScrollRect.MovementType.Unrestricted"/>, the content stops
        /// at its edges, so items near the ends stay as close to the center as the content allows.
        /// Axes that are not enabled keep their current position. Stops a running <see cref="ScrollTo"/>.
        /// </summary>
        public static void SnapTo(this ScrollRect scroll, Transform item, Vector2 offset = default) =>
            Snap(scroll, item, offset, scroll.horizontal, scroll.vertical);

        /// <summary>
        /// Horizontal-only <see cref="SnapTo"/>. Does nothing when the scroll is not <see cref="ScrollRect.horizontal"/>.
        /// </summary>
        public static void SnapXTo(this ScrollRect scroll, Transform item, float offset = 0f) =>
            Snap(scroll, item, new Vector2(offset, 0f), scroll.horizontal, false);

        /// <summary>
        /// Vertical-only <see cref="SnapTo"/>. Does nothing when the scroll is not <see cref="ScrollRect.vertical"/>.
        /// </summary>
        public static void SnapYTo(this ScrollRect scroll, Transform item, float offset = 0f) =>
            Snap(scroll, item, new Vector2(0f, offset), false, scroll.vertical);

        /// <summary>
        /// Animated <see cref="SnapTo"/>: moves to the same position over time, on unscaled time like the
        /// <see cref="ScrollRect"/> itself. The duration is the distance divided by <paramref name="speed"/>,
        /// so far items take longer than near ones, kept within <paramref name="durationLimits"/>.
        /// </summary>
        /// <param name="scroll">The scroll to move.</param>
        /// <param name="item">The child to center.</param>
        /// <param name="speed">Average speed, in units per second of the content's parent. Not positive snaps at once.</param>
        /// <param name="easing">Easing of the movement, with time and value on 0–1 (see <c>Mane.Unity.Easing</c>).
        /// Null moves linearly. Overshooting curves stop at the content edges, like the target does.</param>
        /// <param name="offset">Shift from the centered position.</param>
        /// <param name="durationLimits">Shortest and longest duration in seconds, so near items do not jump
        /// and far ones do not crawl. Null uses 0.25–0.8 seconds.</param>
        /// <returns>The running coroutine, which can be yielded to wait for the end, or null when it snapped
        /// at once: outside Play mode, on an inactive object, or with nothing to move.</returns>
        /// <remarks>
        /// Another scroll or snap on the same <see cref="ScrollRect"/>, or anything else moving the content
        /// (such as a drag), stops it.
        /// </remarks>
        public static Coroutine ScrollTo(this ScrollRect scroll, Transform item, float speed,
            AnimationCurve easing = null, Vector2 offset = default, MinMaxFloat? durationLimits = null) =>
            Scroll(scroll, item, speed, easing, offset, durationLimits, scroll.horizontal, scroll.vertical);

        /// <summary>
        /// Horizontal-only <see cref="ScrollTo"/>. Does nothing when the scroll is not <see cref="ScrollRect.horizontal"/>.
        /// </summary>
        public static Coroutine ScrollXTo(this ScrollRect scroll, Transform item, float speed,
            AnimationCurve easing = null, float offset = 0f, MinMaxFloat? durationLimits = null) =>
            Scroll(scroll, item, speed, easing, new Vector2(offset, 0f), durationLimits, scroll.horizontal, false);

        /// <summary>
        /// Vertical-only <see cref="ScrollTo"/>. Does nothing when the scroll is not <see cref="ScrollRect.vertical"/>.
        /// </summary>
        public static Coroutine ScrollYTo(this ScrollRect scroll, Transform item, float speed,
            AnimationCurve easing = null, float offset = 0f, MinMaxFloat? durationLimits = null) =>
            Scroll(scroll, item, speed, easing, new Vector2(0f, offset), durationLimits, false, scroll.vertical);

        /// <summary>
        /// Stops a running <see cref="ScrollTo"/> where it is.
        /// </summary>
        public static void StopScrolling(this ScrollRect scroll)
        {
            if (scroll == null || !Running.TryGetValue(scroll, out ScrollRectAnimation animation))
                return;

            Running.Remove(scroll);
            if (animation.Coroutine != null)
                scroll.StopCoroutine(animation.Coroutine);
        }

        /// <summary>
        /// The animation of a <see cref="ScrollTo"/> call, or null when there is nothing to animate.
        /// </summary>
        internal static ScrollRectAnimation CreateAnimation(ScrollRect scroll, Transform item, float speed,
            AnimationCurve easing, Vector2 offset, MinMaxFloat? durationLimits, bool moveX, bool moveY)
        {
            if ((!moveX && !moveY) || speed <= 0f
                || !TryGetMove(scroll, item, offset, out Vector2 delta, out Vector2 min, out Vector2 max))
            {
                return null;
            }

            if (!moveX)
                delta.x = 0f;
            if (!moveY)
                delta.y = 0f;

            if (delta.sqrMagnitude < MinDistance * MinDistance)
                return null;

            float duration = (durationLimits ?? DefaultDurationLimits).Clamp(delta.magnitude / speed);
            Vector2 start = scroll.content.anchoredPosition;
            return new ScrollRectAnimation(scroll, start + delta, start + min, start + max,
                moveX, moveY, Mathf.Max(0f, duration), easing);
        }

        /// <summary>
        /// Writes the enabled axes of the content position and stops the inertia on them,
        /// which would otherwise carry the content away.
        /// </summary>
        internal static void MoveContent(ScrollRect scroll, Vector2 position, bool moveX, bool moveY)
        {
            Vector2 current = scroll.content.anchoredPosition;
            Vector2 velocity = scroll.velocity;
            if (moveX)
            {
                current.x = position.x;
                velocity.x = 0f;
            }

            if (moveY)
            {
                current.y = position.y;
                velocity.y = 0f;
            }

            scroll.content.anchoredPosition = current;
            scroll.velocity = velocity;
        }

        private static void Snap(ScrollRect scroll, Transform item, Vector2 offset, bool snapX, bool snapY)
        {
            if (!snapX && !snapY)
                return;

            scroll.StopScrolling();
            if (TryGetMove(scroll, item, offset, out Vector2 delta, out _, out _))
                MoveContent(scroll, scroll.content.anchoredPosition + delta, snapX, snapY);
        }

        private static Coroutine Scroll(ScrollRect scroll, Transform item, float speed,
            AnimationCurve easing, Vector2 offset, MinMaxFloat? durationLimits, bool moveX, bool moveY)
        {
            if (!moveX && !moveY)
                return null;

            // Coroutines need the player loop and an active object.
            if (speed <= 0f || !Application.isPlaying || !scroll.gameObject.activeInHierarchy)
            {
                Snap(scroll, item, offset, moveX, moveY);
                return null;
            }

            scroll.StopScrolling();
            ScrollRectAnimation animation =
                CreateAnimation(scroll, item, speed, easing, offset, durationLimits, moveX, moveY);
            if (animation == null)
                return null;

            animation.Coroutine = scroll.StartCoroutine(Run(animation));
            Running.Add(scroll, animation);
            return animation.Coroutine;
        }

        private static IEnumerator Run(ScrollRectAnimation animation)
        {
            do
            {
                yield return null;
            } while (animation.Step(Time.unscaledDeltaTime));

            if (Running.TryGetValue(animation.Scroll, out ScrollRectAnimation current) && current == animation)
                Running.Remove(animation.Scroll);
        }

        // The move that centers the item, and the moves that keep the view covered by the content.
        // Anchored position moves the content in its parent's space, so everything is measured there.
        // That keeps the result independent of the anchors and pivots of the content and the item.
        private static bool TryGetMove(ScrollRect scroll, Transform item, Vector2 offset,
            out Vector2 delta, out Vector2 min, out Vector2 max)
        {
            delta = min = max = default;
            RectTransform content = scroll.content;
            if (content == null || item == null)
                return false;

            Canvas.ForceUpdateCanvases();

            Transform space = content.parent;
            RectTransform viewport = scroll.viewport != null ? scroll.viewport : (RectTransform)scroll.transform;
            Rect view = GetRect(viewport, space);
            Vector2 itemCenter = space.InverseTransformPoint(GetWorldCenter(item));
            delta = view.center + offset - itemCenter;

            if (scroll.movementType == ScrollRect.MovementType.Unrestricted)
            {
                min = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
                max = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
                return true;
            }

            Rect bounds = GetRect(content, space);
            GetLimits(view.xMin, view.xMax, bounds.xMin, bounds.xMax, out min.x, out max.x);
            GetLimits(view.yMin, view.yMax, bounds.yMin, bounds.yMax, out min.y, out max.y);
            delta = Vector2.Max(min, Vector2.Min(max, delta));
            return true;
        }

        // Content smaller than the view does not move: its place is left to the ScrollRect.
        private static void GetLimits(float viewMin, float viewMax, float contentMin, float contentMax,
            out float min, out float max)
        {
            min = viewMax - contentMax;
            max = viewMin - contentMin;
            if (min > max)
                min = max = 0f;
        }

        private static Vector3 GetWorldCenter(Transform item) =>
            item is RectTransform rect ? rect.TransformPoint(rect.rect.center) : item.position;

        private static Rect GetRect(RectTransform rect, Transform space)
        {
            rect.GetWorldCorners(Corners);
            Vector2 min = space.InverseTransformPoint(Corners[0]);
            Vector2 max = min;
            for (int i = 1; i < Corners.Length; i++)
            {
                Vector2 corner = space.InverseTransformPoint(Corners[i]);
                min = Vector2.Min(min, corner);
                max = Vector2.Max(max, corner);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }
}
