using UnityEngine;
using UnityEngine.EventSystems;

namespace Mane.Unity.UI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class ManeUIBehaviour : UIBehaviour
    {
        private RectTransform _rectTransform;

        public RectTransform rectTransform => _rectTransform ? _rectTransform : _rectTransform = transform as RectTransform;
    }
}