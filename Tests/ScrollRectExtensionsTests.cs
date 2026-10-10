using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Mane.Unity.UI.Tests
{
    public class ScrollRectExtensionsTests
    {
        [Test]
        public void SnapXTo_KeepsY_SnapYTo_KeepsX()
        {
            GameObject root = new("ScrollRoot");
            try
            {
                Canvas canvas = root.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                GameObject scrollGo = CreateUiChild(root.transform, "Scroll");
                ScrollRect scroll = scrollGo.AddComponent<ScrollRect>();
                scroll.horizontal = true;
                scroll.vertical = true;

                GameObject contentGo = CreateUiChild(scrollGo.transform, "Content");
                RectTransform content = contentGo.GetComponent<RectTransform>();
                content.sizeDelta = new Vector2(400f, 400f);
                content.anchoredPosition = new Vector2(40f, -30f);
                scroll.content = content;

                GameObject itemGo = CreateUiChild(contentGo.transform, "Item");
                itemGo.GetComponent<RectTransform>().anchoredPosition = new Vector2(80f, -50f);

                Vector2 before = content.anchoredPosition;

                Assert.DoesNotThrow(() => scroll.SnapTo(itemGo.transform));

                content.anchoredPosition = before;
                scroll.SnapXTo(itemGo.transform, 10f);
                Assert.AreEqual(before.y, content.anchoredPosition.y);

                content.anchoredPosition = before;
                scroll.SnapYTo(itemGo.transform, 10f);
                Assert.AreEqual(before.x, content.anchoredPosition.x);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        // Default "Scroll View" layout: centered pivot, stretched Viewport, top-stretch Content with pivot (0, 1).
        // Here the Content's anchored position and its position in ScrollRect space differ.
        [Test]
        public void Snap_DefaultScrollViewLayout_KeepsAxisThatIsNotSnapped()
        {
            GameObject root = new("ScrollRoot");
            try
            {
                Canvas canvas = root.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                GameObject scrollGo = CreateUiChild(root.transform, "Scroll");
                scrollGo.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 300f);
                ScrollRect scroll = scrollGo.AddComponent<ScrollRect>();
                scroll.horizontal = false;
                scroll.vertical = true;

                GameObject viewportGo = CreateUiChild(scrollGo.transform, "Viewport");
                RectTransform viewport = viewportGo.GetComponent<RectTransform>();
                viewport.anchorMin = Vector2.zero;
                viewport.anchorMax = Vector2.one;
                viewport.sizeDelta = Vector2.zero;
                scroll.viewport = viewport;

                GameObject contentGo = CreateUiChild(viewportGo.transform, "Content");
                RectTransform content = contentGo.GetComponent<RectTransform>();
                content.anchorMin = new Vector2(0f, 1f);
                content.anchorMax = Vector2.one;
                content.pivot = new Vector2(0f, 1f);
                content.sizeDelta = new Vector2(0f, 1000f);
                content.anchoredPosition = Vector2.zero;
                scroll.content = content;

                GameObject itemGo = CreateUiChild(contentGo.transform, "Item");
                RectTransform item = itemGo.GetComponent<RectTransform>();
                item.anchorMin = item.anchorMax = new Vector2(0f, 1f);
                item.anchoredPosition = new Vector2(50f, -600f);

                scroll.SnapYTo(itemGo.transform);
                Assert.AreEqual(0f, content.anchoredPosition.x, 0.001f);

                content.anchoredPosition = Vector2.zero;
                scroll.SnapTo(itemGo.transform);
                Assert.AreEqual(0f, content.anchoredPosition.x, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateUiChild(Transform parent, string name)
        {
            GameObject go = new(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }
    }
}
