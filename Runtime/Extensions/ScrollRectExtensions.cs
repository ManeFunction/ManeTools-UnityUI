using UnityEngine;
using UnityEngine.UI;

namespace Mane.Unity.UI
{
    /// <summary>
    /// Helpers to move a <see cref="ScrollRect"/> so a child is in view.
    /// </summary>
    public static class ScrollRectExtensions
    {
        // Based on this discussion, but improved:
        // https://stackoverflow.com/questions/30766020/how-to-scroll-to-a-specific-element-in-scrollrect-with-unity-ui
        /// <summary>
        /// Scrolls so <paramref name="item"/> sits at the content origin, plus <paramref name="offset"/>.
        /// Axes that are not enabled keep their current position.
        /// </summary>
        public static void SnapTo(this ScrollRect scroll, Transform item, Vector2 offset = default)
        {
            Canvas.ForceUpdateCanvases();

            Vector2 contentPos = scroll.transform.InverseTransformPoint(scroll.content.position);
            Vector2 childPos = scroll.transform.InverseTransformPoint(item.position);
            Vector2 endPos = contentPos - childPos;

            Vector2 current = scroll.content.anchoredPosition;
            Vector2 target = endPos + offset;
            if (!scroll.horizontal) target.x = current.x;
            if (!scroll.vertical) target.y = current.y;
            scroll.content.anchoredPosition = target;
        }

        // Optimized versions
        /// <summary>
        /// Horizontal-only <see cref="SnapTo"/>.
        /// </summary>
        public static void SnapXTo(this ScrollRect scroll, Transform item, float offset = 0f)
        {
            Canvas.ForceUpdateCanvases();

            Vector2 contentPos = scroll.transform.InverseTransformPoint(scroll.content.position);
            float childPos = scroll.transform.InverseTransformPoint(item.position).x;
            float x = contentPos.x - childPos;

            scroll.content.anchoredPosition = new Vector2(x + offset, scroll.content.anchoredPosition.y);
        }
        
        /// <summary>
        /// Vertical-only <see cref="SnapTo"/>.
        /// </summary>
        public static void SnapYTo(this ScrollRect scroll, Transform item, float offset = 0f)
        {
            Canvas.ForceUpdateCanvases();

            Vector2 contentPos = scroll.transform.InverseTransformPoint(scroll.content.position);
            float childPos = scroll.transform.InverseTransformPoint(item.position).y;
            float y = contentPos.y - childPos;

            scroll.content.anchoredPosition = new Vector2(scroll.content.anchoredPosition.x, y + offset);
        }
    }
}