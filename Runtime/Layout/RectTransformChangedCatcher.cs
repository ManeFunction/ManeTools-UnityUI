using System;
using UnityEngine;

namespace Mane.Unity.UI
{
    /// <summary>
    /// Forwards RectTransform dimension changes as <see cref="OnRectTransformDimensionsChanged"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Mane Tools/UI/RectTransformChanged Catcher")]
    public class RectTransformChangedCatcher : ManeUIBehaviour
    {
        /// <summary>
        /// Raised when this RectTransform's dimensions change.
        /// </summary>
        public event Action<RectTransform> OnRectTransformDimensionsChanged;

        protected override void OnRectTransformDimensionsChange()
        {
            OnRectTransformDimensionsChanged?.Invoke(rectTransform);
        }
    }
}
