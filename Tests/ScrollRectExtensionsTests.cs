using Mane.DotNet;
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

        private const float FrameTime = 1f / 60f;

        private static readonly AnimationCurve[] Curves =
        {
            Easing.Linear, Easing.QuadIn, Easing.CubicOut, Easing.SinInOut, Easing.ExpoInOut,
            Easing.CircOut, Easing.BackOut, Easing.BounceOut, Easing.ElasticOut, Easing.Spring
        };

        private static readonly AnimationCurve[] SmoothCurves =
        {
            Easing.Linear, Easing.QuadIn, Easing.QuadOut, Easing.CubicInOut, Easing.SinOut, Easing.ExpoIn
        };

        // Item 600 from the top: the content moves 450, item 375: 225. Default limits are 0.25–0.8 seconds.
        [TestCase(600f, 900f, .5f, TestName = "ScrollYTo_Duration_DistanceBySpeed")]
        [TestCase(375f, 900f, .25f, TestName = "ScrollYTo_Duration_DistanceBySpeed_AtMin")]
        [TestCase(600f, 150f, .8f, TestName = "ScrollYTo_Duration_TooSlow_ClampedToMax")]
        [TestCase(375f, 9000f, .25f, TestName = "ScrollYTo_Duration_TooFast_ClampedToMin")]
        public void ScrollYTo_Duration_DefaultLimits(float itemFromTop, float speed, float expected)
        {
            GameObject root = new("ScrollRoot");
            try
            {
                ScrollRect scroll = CreateVerticalList(root);
                RectTransform item = CreateItem(scroll.content, new Vector2(0.5f, 1f), new Vector2(0f, -itemFromTop));

                ScrollRectAnimation animation = CreateAnimationY(scroll, item, speed, Easing.QuadOut);
                Assert.AreEqual(expected, animation.Duration, 0.0001f);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        // 450 at 900 per second is 0.5 seconds before the limits.
        [TestCase(0f, 10f, .5f, TestName = "ScrollYTo_Duration_WideLimits_Unchanged")]
        [TestCase(1f, 2f, 1f, TestName = "ScrollYTo_Duration_CustomMin")]
        [TestCase(.1f, .2f, .2f, TestName = "ScrollYTo_Duration_CustomMax")]
        [TestCase(.3f, .1f, .3f, TestName = "ScrollYTo_Duration_ReversedLimits")]
        public void ScrollYTo_Duration_CustomLimits(float a, float b, float expected)
        {
            GameObject root = new("ScrollRoot");
            try
            {
                ScrollRect scroll = CreateVerticalList(root);
                RectTransform item = CreateItem(scroll.content, new Vector2(0.5f, 1f), new Vector2(0f, -600f));

                ScrollRectAnimation animation = ScrollRectExtensions.CreateAnimation(
                    scroll, item, 900f, Easing.QuadOut, Vector2.zero, new MinMaxFloat(a, b), false, true);
                Assert.AreEqual(expected, animation.Duration, 0.0001f);

                RunToEnd(animation);
                Assert.AreEqual(450f, scroll.content.anchoredPosition.y, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [TestCaseSource(nameof(Curves))]
        public void ScrollYTo_EndsOnSnapPosition(AnimationCurve easing)
        {
            GameObject root = new("ScrollRoot");
            try
            {
                ScrollRect scroll = CreateVerticalList(root);
                RectTransform item = CreateItem(scroll.content, new Vector2(0.5f, 1f), new Vector2(0f, -600f));

                ScrollRectAnimation animation = CreateAnimationY(scroll, item, 900f, easing);
                int frames = RunToEnd(animation);

                Assert.AreEqual(450f, scroll.content.anchoredPosition.y, 0.001f);
                Assert.AreEqual(0f, scroll.content.anchoredPosition.x, 0.001f);
                Assert.AreEqual(Mathf.CeilToInt(animation.Duration / FrameTime), frames, 1);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [TestCaseSource(nameof(SmoothCurves))]
        public void ScrollYTo_FollowsEasing(AnimationCurve easing)
        {
            GameObject root = new("ScrollRoot");
            try
            {
                ScrollRect scroll = CreateVerticalList(root);
                RectTransform item = CreateItem(scroll.content, new Vector2(0.5f, 1f), new Vector2(0f, -600f));

                // 450 at 900 per second: 0.5 seconds.
                ScrollRectAnimation animation = CreateAnimationY(scroll, item, 900f, easing);
                for (float time = .1f; time < .45f; time += .1f)
                {
                    Assert.IsTrue(animation.Step(.1f));
                    Assert.AreEqual(450f * easing.Evaluate(time / .5f), scroll.content.anchoredPosition.y, 0.01f);
                }
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ScrollYTo_Overshoot_PassesTargetInsideContent()
        {
            GameObject root = new("ScrollRoot");
            try
            {
                ScrollRect scroll = CreateVerticalList(root);
                RectTransform item = CreateItem(scroll.content, new Vector2(0.5f, 1f), new Vector2(0f, -600f));

                float furthest = RunToEnd(CreateAnimationY(scroll, item, 900f, Easing.BackOut), scroll);

                Assert.Greater(furthest, 451f);
                Assert.AreEqual(450f, scroll.content.anchoredPosition.y, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [TestCase(950f, 700f)]
        [TestCase(50f, 0f)]
        public void ScrollYTo_Overshoot_StopsAtContentEdge(float itemFromTop, float edge)
        {
            GameObject root = new("ScrollRoot");
            try
            {
                ScrollRect scroll = CreateVerticalList(root);
                RectTransform item = CreateItem(scroll.content, new Vector2(0.5f, 1f), new Vector2(0f, -itemFromTop));
                foreach (AnimationCurve easing in new[] { Easing.BackOut, Easing.ElasticOut, Easing.Spring })
                {
                    // Start mid-way, so the move toward the edge can overshoot it.
                    scroll.content.anchoredPosition = new Vector2(0f, 350f);
                    ScrollRectAnimation animation = CreateAnimationY(scroll, item, 900f, easing);
                    while (animation.Step(FrameTime))
                    {
                        Assert.That(scroll.content.anchoredPosition.y, Is.InRange(-0.001f, 700.001f));
                    }

                    Assert.AreEqual(edge, scroll.content.anchoredPosition.y, 0.001f);
                }
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ScrollXTo_HorizontalList_EndsCentered()
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

                RectTransform item = CreateItem(content, new Vector2(0f, 0.5f), new Vector2(600f, 0f));

                ScrollRectAnimation animation =
                    ScrollRectExtensions.CreateAnimation(scroll, item, 900f, Easing.QuadInOut, Vector2.zero, null, true, false);
                Assert.AreEqual(.5f, animation.Duration, 0.0001f);
                RunToEnd(animation);

                AssertPosition(new Vector2(50f, 0f), content);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ScrollYTo_MovedElsewhere_Stops()
        {
            GameObject root = new("ScrollRoot");
            try
            {
                ScrollRect scroll = CreateVerticalList(root);
                RectTransform item = CreateItem(scroll.content, new Vector2(0.5f, 1f), new Vector2(0f, -600f));

                ScrollRectAnimation animation = CreateAnimationY(scroll, item, 900f, Easing.SinInOut);
                Assert.IsTrue(animation.Step(FrameTime));

                // A drag moves the content between steps.
                scroll.content.anchoredPosition = new Vector2(0f, 120f);
                Assert.IsFalse(animation.Step(FrameTime));
                Assert.AreEqual(120f, scroll.content.anchoredPosition.y, 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ScrollTo_NothingToAnimate_ReturnsNoAnimation()
        {
            GameObject root = new("ScrollRoot");
            try
            {
                ScrollRect scroll = CreateVerticalList(root);
                RectTransform item = CreateItem(scroll.content, new Vector2(0.5f, 1f), new Vector2(0f, -600f));

                // No room on X, no speed, and already centered.
                Assert.IsNull(ScrollRectExtensions.CreateAnimation(scroll, item, 900f, Easing.Linear, Vector2.zero, null, true, false));
                Assert.IsNull(CreateAnimationY(scroll, item, 0f, Easing.Linear));
                scroll.SnapYTo(item);
                Assert.IsNull(CreateAnimationY(scroll, item, 900f, Easing.Linear));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ScrollTo_OutsidePlayMode_Snaps()
        {
            GameObject root = new("ScrollRoot");
            try
            {
                ScrollRect scroll = CreateVerticalList(root);
                RectTransform item = CreateItem(scroll.content, new Vector2(0.5f, 1f), new Vector2(0f, -600f));

                Assert.IsNull(scroll.ScrollTo(item, 900f, Easing.CubicOut));
                AssertPosition(new Vector2(0f, 450f), scroll.content);

                scroll.content.anchoredPosition = Vector2.zero;
                Assert.IsNull(scroll.ScrollXTo(item, 900f, Easing.CubicOut));
                AssertPosition(Vector2.zero, scroll.content);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static ScrollRectAnimation CreateAnimationY(ScrollRect scroll, Transform item, float speed, AnimationCurve easing) =>
            ScrollRectExtensions.CreateAnimation(scroll, item, speed, easing, Vector2.zero, null, false, true);

        private static int RunToEnd(ScrollRectAnimation animation)
        {
            int frames = 1;
            while (animation.Step(FrameTime))
                frames++;

            return frames;
        }

        // Returns the furthest Y the content reached.
        private static float RunToEnd(ScrollRectAnimation animation, ScrollRect scroll)
        {
            float furthest = scroll.content.anchoredPosition.y;
            while (animation.Step(FrameTime))
                furthest = Mathf.Max(furthest, scroll.content.anchoredPosition.y);

            return Mathf.Max(furthest, scroll.content.anchoredPosition.y);
        }

        // Vertical "Scroll View" with a 1000-high top-stretch Content.
        private static ScrollRect CreateVerticalList(GameObject root)
        {
            ScrollRect scroll = CreateDefaultScrollView(root, horizontal: false);
            RectTransform content = scroll.content;
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(0f, 1f);
            content.sizeDelta = new Vector2(0f, 1000f);
            content.anchoredPosition = Vector2.zero;
            return scroll;
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
