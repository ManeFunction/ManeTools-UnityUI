using UnityEngine;
using UnityEngine.UI;

namespace Mane.Unity.UI
{
    /// <summary>
    /// Helpers to move a <see cref="ScrollRect"/> so a child is in view.
    /// </summary>
    public static class ScrollRectExtensions
    {
        private static readonly Vector3[] Corners = new Vector3[4];

        /// <summary>
        /// Scrolls so <paramref name="item"/> is centered in the viewport, shifted by <paramref name="offset"/>.
        /// Unless the movement type is <see cref="ScrollRect.MovementType.Unrestricted"/>, the content stops
        /// at its edges, so items near the ends stay as close to the center as the content allows.
        /// Axes that are not enabled keep their current position.
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

        private static void Snap(ScrollRect scroll, Transform item, Vector2 offset, bool snapX, bool snapY)
        {
            RectTransform content = scroll.content;
            if (content == null || item == null || (!snapX && !snapY))
                return;

            Canvas.ForceUpdateCanvases();

            // Anchored position moves the content in its parent's space, so everything is measured there.
            // That keeps the result independent of the anchors and pivots of the content and the item.
            Transform space = content.parent;
            RectTransform viewport = scroll.viewport != null ? scroll.viewport : (RectTransform)scroll.transform;
            Rect view = GetRect(viewport, space);
            Rect bounds = GetRect(content, space);
            Vector2 itemCenter = space.InverseTransformPoint(GetWorldCenter(item));

            Vector2 delta = view.center + offset - itemCenter;
            if (scroll.movementType != ScrollRect.MovementType.Unrestricted)
            {
                delta.x = ClampToBounds(delta.x, view.xMin, view.xMax, bounds.xMin, bounds.xMax);
                delta.y = ClampToBounds(delta.y, view.yMin, view.yMax, bounds.yMin, bounds.yMax);
            }

            Vector2 position = content.anchoredPosition;
            Vector2 velocity = scroll.velocity;
            if (snapX)
            {
                position.x += delta.x;
                velocity.x = 0f;
            }

            if (snapY)
            {
                position.y += delta.y;
                velocity.y = 0f;
            }

            content.anchoredPosition = position;
            // Inertia would carry the content away from the snapped position.
            scroll.velocity = velocity;
        }

        // Keeps the view covered by the content. Content smaller than the view is left to the ScrollRect.
        private static float ClampToBounds(float delta, float viewMin, float viewMax, float contentMin, float contentMax)
        {
            float min = viewMax - contentMax;
            float max = viewMin - contentMin;
            return min > max ? 0f : Mathf.Clamp(delta, min, max);
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
