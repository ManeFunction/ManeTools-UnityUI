using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Mane.Unity.UI.Tests
{
    public class ThreeStatesToggleTests
    {
        [Test]
        public void State_GetSet_InvokesChanged()
        {
            GameObject canvasGo = new("Canvas", typeof(Canvas), typeof(RectTransform));
            GameObject toggleGo = new("Toggle", typeof(RectTransform));
            try
            {
                toggleGo.transform.SetParent(canvasGo.transform, false);
                ThreeStatesToggle toggle = toggleGo.AddComponent<ThreeStatesToggle>();

                Assert.IsNull(toggle.State);

                bool? last = true;
                int count = 0;
                toggle.StateValueChanged += value =>
                {
                    last = value;
                    count++;
                };

                toggle.State = true;
                Assert.IsTrue(toggle.State);
                Assert.IsTrue(toggle.isOn);
                Assert.AreEqual(true, last);
                Assert.AreEqual(1, count);

                toggle.State = false;
                Assert.IsFalse(toggle.State);
                Assert.IsFalse(toggle.isOn);
                Assert.AreEqual(false, last);
                Assert.AreEqual(2, count);

                toggle.State = null;
                Assert.IsNull(toggle.State);
                Assert.IsFalse(toggle.isOn);
                Assert.IsNull(last);
                Assert.AreEqual(3, count);

                toggle.State = null;
                Assert.AreEqual(3, count);
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }

        [Test]
        public void StateValueChanged_Unsubscribe_StopsNotifications()
        {
            GameObject go = new("Toggle", typeof(RectTransform));
            try
            {
                ThreeStatesToggle toggle = go.AddComponent<ThreeStatesToggle>();
                int count = 0;
                void OnChanged(bool? _) => count++;

                toggle.StateValueChanged += OnChanged;
                toggle.StateValueChanged -= OnChanged;
                toggle.State = true;

                Assert.AreEqual(0, count);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Submit_CyclesStateLikeClick()
        {
            GameObject canvasGo = new("Canvas", typeof(Canvas), typeof(RectTransform));
            GameObject toggleGo = new("Toggle", typeof(RectTransform));
            try
            {
                toggleGo.transform.SetParent(canvasGo.transform, false);
                ThreeStatesToggle toggle = toggleGo.AddComponent<ThreeStatesToggle>();
                BaseEventData submit = new(null);

                toggle.OnSubmit(submit);
                Assert.IsTrue(toggle.State);
                Assert.IsTrue(toggle.isOn);

                toggle.OnSubmit(submit);
                Assert.IsFalse(toggle.State);

                toggle.OnSubmit(submit);
                Assert.IsNull(toggle.State);
                Assert.IsFalse(toggle.isOn);
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }

        [Test]
        public void ClickAndSubmit_NotInteractable_KeepState()
        {
            GameObject canvasGo = new("Canvas", typeof(Canvas), typeof(RectTransform));
            GameObject toggleGo = new("Toggle", typeof(RectTransform));
            try
            {
                toggleGo.transform.SetParent(canvasGo.transform, false);
                ThreeStatesToggle toggle = toggleGo.AddComponent<ThreeStatesToggle>();
                toggle.interactable = false;

                toggle.OnPointerClick(new PointerEventData(null) { button = PointerEventData.InputButton.Left });
                toggle.OnSubmit(new BaseEventData(null));

                Assert.IsNull(toggle.State);
                Assert.IsFalse(toggle.isOn);
            }
            finally
            {
                Object.DestroyImmediate(canvasGo);
            }
        }

        [Test]
        public void IsOn_Setter_MapsToTrueOrFalseState()
        {
            GameObject go = new("Toggle", typeof(RectTransform));
            try
            {
                ThreeStatesToggle toggle = go.AddComponent<ThreeStatesToggle>();

                toggle.isOn = true;
                Assert.IsTrue(toggle.State);

                toggle.isOn = false;
                Assert.IsFalse(toggle.State);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
