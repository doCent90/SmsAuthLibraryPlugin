using System;
using TMPro;
using UnityEngine;

namespace Agava.Wink
{
    [Serializable]
    internal class WindowScalerPresenter
    {
        [SerializeField] private RectTransform _target;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private float _yOffset;

        private Vector3 _defaultPosition;
        private bool _hasMoved = false;

        internal void Construct() => _defaultPosition = _target.position;

        internal void Update() 
        {
            if (_inputField.isFocused && _hasMoved == false)
                OnTargetWindowOpened();
        }

        internal void OnTargetWindowOpened()
        {
            Debug.LogWarning("KEY");
            _target.position += new Vector3(0, _yOffset, 0);
            _hasMoved = true;
            TouchScreenKeyboard.Open("", TouchScreenKeyboardType.PhonePad);
        }

        internal void OnTargetWindowClosed()
        {
            _target.position = _defaultPosition;
            _hasMoved = false;
        }
    }
}
