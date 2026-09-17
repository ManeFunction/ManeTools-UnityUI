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

        private static GameObject CreateUiChild(Transform parent, string name)
        {
            GameObject go = new(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }
    }
}
