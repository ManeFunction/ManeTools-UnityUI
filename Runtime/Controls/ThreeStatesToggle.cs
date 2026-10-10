using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mane.Unity.UI
{
    /// <summary>
    /// Toggle with on, off, and undefined states. Click cycles on → off → undefined.
    /// <see cref="Toggle.graphic"/> is the on visual; <see cref="offGraphic"/> and
    /// <see cref="undefinedGraphic"/> cover the other two.
    /// </summary>
    [AddComponentMenu("Mane Tools/UI/Three States Toggle")]
    public class ThreeStatesToggle : Toggle
    {
        /// <summary>
        /// Graphic shown while the toggle is off.
        /// </summary>
        public Graphic offGraphic;

        /// <summary>
        /// Graphic shown while the toggle is undefined.
        /// </summary>
        public Graphic undefinedGraphic;
        
        [SerializeField] private ToggleState _state = ToggleState.Undefined;

        /// <summary>
        /// Invoked when <see cref="State"/> changes. Argument is true, false, or null.
        /// </summary>
        public ThreeStatesToggleEvent onStateValueChanged = new();
        
        /// <summary>
        /// Same as <see cref="onStateValueChanged"/>, as a C# event.
        /// </summary>
        public event UnityAction<bool?> StateValueChanged
        {
            add => onStateValueChanged.AddListener(value);
            remove => onStateValueChanged.RemoveListener(value);
        }

        /// <summary>
        /// Binary on/off. Setting it also updates <see cref="State"/> to true or false.
        /// </summary>
        public new bool isOn
        {
            get => base.isOn;
            set
            {
                base.isOn = value; 
                UpdateStateFromIsOn();
            }
        }

        private const string ToggleGroupNotSupportedMessage = "Toggle group is not supported with ThreeStatesToggle";
        /// <summary>
        /// Not supported. Three-state toggles cannot belong to a <see cref="ToggleGroup"/>.
        /// </summary>
        [Obsolete(ToggleGroupNotSupportedMessage, true)]
        public new ToggleGroup group
        {
            get => throw new NotSupportedException(ToggleGroupNotSupportedMessage);
            set => throw new NotSupportedException(ToggleGroupNotSupportedMessage);
        }

        /// <summary>
        /// Three-state value: true, false, or null (undefined).
        /// </summary>
        public bool? State
        {
            get => StateToBool(_state);
            set
            {
                ToggleState state = BoolToState(value);
                if (_state == state) return;
                
                _state = state;
                UpdateIsOnFromState();
                PlayUndefinedEffect(transition == Transition.None);
                onStateValueChanged.Invoke(value);
            }
        }

        private ToggleState BoolToState(bool? value) => value switch
        {
            true => ToggleState.On,
            false => ToggleState.Off,
            null => ToggleState.Undefined,
        };

        private bool? StateToBool(ToggleState state) => state switch
        {
            ToggleState.On => true,
            ToggleState.Off => false,
            _ => null,
        };

        protected override void Start()
        {
            base.Start();
            
            UpdateIsOnFromState();
            PlayUndefinedEffect(true);
        }
        
        private void PlayUndefinedEffect(bool instant)
        {
            ProcessGraphic(undefinedGraphic, ToggleState.Undefined);
            ProcessGraphic(offGraphic, ToggleState.Off);
            
            return;
            
            
            void ProcessGraphic(Graphic g, ToggleState state)
            {
                if (g == null) return;
                
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    g.canvasRenderer.SetAlpha(_state == state ? 1f : 0f);
                else
#endif
                    g.CrossFadeAlpha(_state == state ? 1f : 0f, instant ? 0f : 0.1f, true);
            }
        }
        
        protected override void OnDidApplyAnimationProperties()
        {
            // Check if state has been changed by the animation.
            // Unfortunately there is no way to check if we don't have a graphic.
            if (undefinedGraphic != null)
            {
                bool isUndefined = !Mathf.Approximately(undefinedGraphic.canvasRenderer.GetColor().a, 0f);
                if (isUndefined && _state != ToggleState.Undefined)
                    _state = ToggleState.Undefined;
            }

            base.OnDidApplyAnimationProperties();
        }

        private void UpdateStateFromIsOn() => 
            State = StateToBool(base.isOn ? ToggleState.On : ToggleState.Off);

        private void UpdateIsOnFromState()
        {
            switch (_state)
            {
                case ToggleState.On:
                    base.isOn = true;
                    break;
                case ToggleState.Off:
                case ToggleState.Undefined:
                    base.isOn = false;
                    break;
            }
        }

        /// <summary>
        /// Cycles state on left click: on → off → undefined → on.
        /// </summary>
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            switch (State)
            {
                case true:  State = false; break;
                case false: State = null;  break;
                case null:  State = true;  break;
            }
        }
        
        
        /// <summary>
        /// UnityEvent raised with the new three-state value.
        /// </summary>
        [Serializable]
        public class ThreeStatesToggleEvent : UnityEvent<bool?> { }

        private enum ToggleState
        {
            Undefined,
            On,
            Off,
        }
    }
}