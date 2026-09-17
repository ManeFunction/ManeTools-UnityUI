using NUnit.Framework;
using UnityEngine;

namespace Mane.Unity.UI.Tests
{
    public class ColorSchemeControllerTests
    {
        [Test]
        public void Refresh_NullGraphicList_DoesNotThrow()
        {
            GameObject go = new("ColorSchemeController");
            ColorScheme scheme = ScriptableObject.CreateInstance<ColorScheme>();
            try
            {
                ColorSchemeController controller = go.AddComponent<ColorSchemeController>();

                Assert.DoesNotThrow(controller.Refresh);
                Assert.DoesNotThrow(() => controller.ColorScheme = scheme);
                Assert.DoesNotThrow(() => controller.SetGraphicsColor(0, Color.red));
                Assert.AreEqual(Color.white, controller.GetGraphicColor(0));
                Assert.DoesNotThrow(() => controller.AnimateTo(scheme, 0f));
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(scheme);
            }
        }

        [Test]
        public void SetColorSchemeWithoutRefresh_DoesNotRequireGraphics()
        {
            GameObject go = new("ColorSchemeController");
            ColorScheme scheme = ScriptableObject.CreateInstance<ColorScheme>();
            try
            {
                ColorSchemeController controller = go.AddComponent<ColorSchemeController>();
                controller.SetColorSchemeWithoutRefresh(scheme);

                Assert.AreEqual(scheme, controller.ColorScheme);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(scheme);
            }
        }
    }
}
