using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Mane.Unity.UI.Tests
{
    public class ColorSchemeTests
    {
        [Test]
        public void Default_HasOneWhiteColor()
        {
            ColorScheme scheme = ScriptableObject.CreateInstance<ColorScheme>();
            try
            {
                Assert.AreEqual(1, scheme.Length);
                Assert.AreEqual(Color.white, scheme[0]);
                Assert.AreEqual(Color.white, scheme[-1]);
                Assert.AreEqual(Color.white, scheme[2]);
            }
            finally
            {
                Object.DestroyImmediate(scheme);
            }
        }

        [Test]
        public void Indexer_UsesSerializedColors()
        {
            ColorScheme scheme = ScriptableObject.CreateInstance<ColorScheme>();
            try
            {
                SerializedObject so = new(scheme);
                SerializedProperty colors = so.FindProperty(ColorScheme.ColorsPropertyName);
                colors.arraySize = 2;
                colors.GetArrayElementAtIndex(0).colorValue = Color.red;
                colors.GetArrayElementAtIndex(1).colorValue = Color.blue;
                so.ApplyModifiedPropertiesWithoutUndo();

                Assert.AreEqual(2, scheme.Length);
                Assert.AreEqual(Color.red, scheme[0]);
                Assert.AreEqual(Color.blue, scheme[1]);
                Assert.AreEqual(Color.white, scheme[2]);
            }
            finally
            {
                Object.DestroyImmediate(scheme);
            }
        }
    }
}
