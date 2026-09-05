using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Mane.Unity.UI
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Mane Tools/UI/RectTransformChanged Catcher")]
    public class RectTransformChangedCatcher : ManeUIBehaviour
    {
        public event Action<RectTransform> OnRectTransformDimensionsChanged;

        protected override void OnRectTransformDimensionsChange()
        {
            OnRectTransformDimensionsChanged?.Invoke(rectTransform);
        }
    }
}
