using UnityEngine;
using UnityEngine.EventSystems;

namespace Mane.Unity.UI
{
    /// <summary>
    /// <see cref="UIBehaviour"/> with a cached <see cref="RectTransform"/>.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class ManeUIBehaviour : UIBehaviour
    {
        private RectTransform _rectTransform;

        /// <summary>
        /// This object's <see cref="RectTransform"/>.
        /// </summary>
        public RectTransform rectTransform => _rectTransform ? _rectTransform : _rectTransform = transform as RectTransform;
    }
}