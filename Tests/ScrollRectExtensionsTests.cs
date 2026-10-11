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

        [Test]
        public void SnapXTo_SnapYTo_DisabledAxis_DoesNothing()
        {
            GameObject root = new("ScrollRoot");
            try
            {
                Canvas canvas = root.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                GameObject scrollGo = CreateUiChild(root.transform, "Scroll");
                ScrollRect scroll = scrollGo.AddComponent<ScrollRect>();

                GameObject contentGo = CreateUiChild(scrollGo.transform, "Content");
                RectTransform content = contentGo.GetComponent<RectTransform>();
                content.sizeDelta = new Vector2(400f, 400f);
                content.anchoredPosition = new Vector2(40f, -30f);
                scroll.content = content;

                GameObject itemGo = CreateUiChild(contentGo.transform, "Item");
                itemGo.GetComponent<RectTransform>().anchoredPosition = new Vector2(80f, -50f);

                Vector2 before = content.anchoredPosition;

                scroll.horizontal = false;
                scroll.vertical = true;
                scroll.SnapXTo(itemGo.transform, 10f);
                Assert.AreEqual(before, content.anchoredPosition);

                scroll.horizontal = true;
                scroll.vertical = false;
                scroll.SnapYTo(itemGo.transform, 10f);
                Assert.AreEqual(before, content.anchoredPosition);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        // 300x300 view, 1000-long content, 100x100 items with a centered pivot.
        [TestCase(600f, 450f, TestName = "SnapYTo_Vertical_CentersItem")]
        [TestCase(50f, 0f, TestName = "SnapYTo_Vertical_ClampsAtStart")]
        [TestCase(950f, 700f, TestName = "SnapYTo_Vertical_ClampsAtEnd")]
        public void SnapYTo_VerticalList(float itemFromTop, float expectedY)
        {
            GameObject root = new("ScrollRoot");
            try
            {
                ScrollRect scroll = CreateDefaultScrollView(root, horizontal: false);
                RectTransform content = scroll.content;
                content.anchorMin = new Vector2(0f, 1f);
                content.anchorMax = Vector2.one;
                content.pivot = new Vector2(0f, 1f);
                content.sizeDelta = new Vector2(0f, 1000f);
                content.anchoredPosition = Vector2.zero;

                RectTransform item = CreateItem(content, new Vector2(0.5f, 1f), new Vector2(0f, -itemFromTop));

                scroll.SnapYTo(item);
                Assert.AreEqual(expectedY, content.anchoredPosition.y, 0.001f);
                Assert.AreEqual(0f, content.anchoredPosition.x, 0.001f);

                content.anchoredPosition = Vector2.zero;
                scroll.SnapTo(item);
                AssertPosition(new Vector2(0f, expectedY), content);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        // Left-stretch content with a centered pivot: anchored position is not measured from the view's left edge.
        [TestCase(600f, -450f, TestName = "SnapXTo_Horizontal_CentersItem")]
        [TestCase(50f, 0f, TestName = "SnapXTo_Horizontal_ClampsAtStart")]
        [TestCase(950f, -700f, TestName = "SnapXTo_Horizontal_ClampsAtEnd")]
        public void SnapXTo_HorizontalList(float itemFromLeft, float expectedShift)
        {
            GameObject root = new("ScrollRoot");
            try
            {
                ScrollRect scroll = CreateDefaultScrollView(root, horizontal: true);
                RectTransform content = scroll.content;
                content.anchorMin = Vector2.zero;
                content.anchorMax = new Vector2(0f, 1f);
                content.pivot = new Vector2(0.5f, 0.5f);
                content.sizeDelta = new Vector2(1000f, 0f);
                content.anchoredPosition = new Vector2(500f, 0f);
                Vector2 start = content.anchoredPosition;

                RectTransform item = CreateItem(content, new Vector2(0f, 0.5f), new Vector2(itemFromLeft, 0f));

                scroll.SnapXTo(item);
                Assert.AreEqual(start.x + expectedShift, content.anchoredPosition.x, 0.001f);
                Assert.AreEqual(start.y, content.anchoredPosition.y, 0.001f);

                content.anchoredPosition = start;
                scroll.SnapTo(item);
                AssertPosition(start + new Vector2(expectedShift, 0f), content);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void SnapTo_Offset_ShiftsFromCenter()
        {
            GameObject root = new("ScrollRoot");
            try
            {
                ScrollRect scroll = CreateDefaultScrollView(root, horizontal: false);
                RectTransform content = scroll.content;
                content.anchorMin = new Vector2(0f, 1f);
                content.anchorMax = Vector2.one;
                content.pivot = new Vector2(0f, 1f);
                content.sizeDelta = new Vector2(0f, 1000f);

                RectTransform item = CreateItem(content, new Vector2(0.5f, 1f), new Vector2(0f, -600f));

                // A positive offset moves the item up from the center, so the content moves less.
                scroll.SnapYTo(item, 20f);
                Assert.AreEqual(470f, content.anchoredPosition.y, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void AssertPosition(Vector2 expected, RectTransform content)
        {
            Assert.AreEqual(expected.x, content.anchoredPosition.x, 0.001f);
            Assert.AreEqual(expected.y, content.anchoredPosition.y, 0.001f);
        }

        // Centered 300x300 ScrollRect with a stretched Viewport, like the "Scroll View" preset.
        private static ScrollRect CreateDefaultScrollView(GameObject root, bool horizontal)
        {
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            GameObject scrollGo = CreateUiChild(root.transform, "Scroll");
            scrollGo.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 300f);
            ScrollRect scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = horizontal;
            scroll.vertical = !horizontal;

            RectTransform viewport = CreateUiChild(scrollGo.transform, "Viewport").GetComponent<RectTransform>();
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.sizeDelta = Vector2.zero;
            scroll.viewport = viewport;

            scroll.content = CreateUiChild(viewport, "Content").GetComponent<RectTransform>();
            return scroll;
        }

        // A 100x100 item whose center is at position from the given content anchor.
        private static RectTransform CreateItem(RectTransform content, Vector2 anchor, Vector2 position)
        {
            RectTransform item = CreateUiChild(content, "Item").GetComponent<RectTransform>();
            item.anchorMin = item.anchorMax = anchor;
            item.sizeDelta = new Vector2(100f, 100f);
            item.anchoredPosition = position;
            return item;
        }

        private static GameObject CreateUiChild(Transform parent, string name)
        {
            GameObject go = new(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }
    }
}
