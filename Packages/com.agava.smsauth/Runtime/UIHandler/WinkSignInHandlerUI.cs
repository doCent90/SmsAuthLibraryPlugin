﻿using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Agava.Wink
{
    public class WinkSignInHandlerUI : MonoBehaviour
    {
        [SerializeField] private WinkAccessManager _winkAccessManager;
        [Header("UI Windows")]
        [SerializeField] private NotifyWindowPresenter _signInWindow;
        [SerializeField] private NotifyWindowPresenter _failWindow;
        [SerializeField] private NotifyWindowPresenter _wrongNumberWindow;
        [SerializeField] private NotifyWindowPresenter _proccesOnWindow;
        [SerializeField] private NotifyWindowPresenter _successfullyWindow;
        [SerializeField] private RedirectWindowPresenter _redirectToWebsiteWindow;
        [SerializeField] private InputWindowPresenter _enterCodeWindow;
        [SerializeField] private List<WindowPresenter> _windows;
        [Header("UI Input")]
        [SerializeField] private TMP_InputField _codeInputField;
        [SerializeField] private TMP_InputField _numbersInputField;
        [Header("UI Buttons")]
        [SerializeField] private Button _signInButton;
        [SerializeField] private Button _openSignInButton;
        [SerializeField] private Button _testSignInButton;
        [Header("Phone Number Check Settings")]
        [SerializeField] private int _numberCount = 10;
        [SerializeField] private int _codeCount = 3;

        private void OnDestroy()
        {
            _signInButton.onClick.RemoveAllListeners();
            _winkAccessManager.OnRefreshFail -= OpenSignWindow;
        }

        private void Start()
        {
            _signInButton.onClick.AddListener(OnSignInClicked);
#if UNITY_EDITOR || TEST
            _testSignInButton.onClick.AddListener(OnTestSignInClicked);
#else
            _testSignInButton.gameObject.SetActive(false);
#endif
            _openSignInButton.onClick.AddListener(OpenSignWindow);
            _windows.ForEach(window => window.Disable());

            if (PlayerPrefs.HasKey(_winkAccessManager.AccesToken) == false)
            {
                _openSignInButton.gameObject.SetActive(true);
            }
            else
            {
#if UNITY_EDITOR || TEST
                _testSignInButton.gameObject.SetActive(false);
#endif
                _openSignInButton.gameObject.SetActive(false);
            }

            _winkAccessManager.OnRefreshFail += OpenSignWindow;
        }

#if UNITY_EDITOR || TEST
        private void OnTestSignInClicked()
        {
            _winkAccessManager.TestEnableSubsription();
            _testSignInButton.gameObject.SetActive(false);
        }
#endif

        private void OpenSignWindow() => _signInWindow.Enable();

        private void OnSignInClicked()
        {
            var number = GetNumber();

            if (string.IsNullOrEmpty(number))
                return;

            _proccesOnWindow.Enable();

            _winkAccessManager.Regist(phoneNumber: number,
            otpCodeRequest: (hasOtpCode) => 
            {
                if (hasOtpCode)
                {
                    _proccesOnWindow.Disable();
                    _enterCodeWindow.Enable(onInputDone: (code) =>
                    {
                        _proccesOnWindow.Enable();
                        _winkAccessManager.SendOtpCode(code);
                    });
                }
                else
                {
                    _proccesOnWindow.Disable();
                    _failWindow.Enable();
                }
            }, 
            winkSubscriptionAccessRequest: (hasAccess) => 
            {
                if (hasAccess)
                {
                    OnSignInDone();
                }
                else
                {
                    _failWindow.Enable();
                    _proccesOnWindow.Disable();
                    _redirectToWebsiteWindow.Enable();
                }
            });
        }

        private void OnSignInDone()
        {
            _successfullyWindow.Enable();
            _signInWindow.Disable();
            _proccesOnWindow.Disable();
            _openSignInButton.gameObject.SetActive(false);
        }

        private string GetNumber()
        {
            bool isCorrectCode = uint.TryParse(_codeInputField.text, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out uint resultCode);
            bool isCorrectNumber = ulong.TryParse(_numbersInputField.text, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out ulong resultNumber);

            int countCode = resultCode.ToString().Length;
            int countNumber = resultNumber.ToString().Length;

            if (isCorrectNumber == false || isCorrectCode == false
                || string.IsNullOrEmpty(_numbersInputField.text)
                || (countNumber < _numberCount || countNumber > _numberCount)
                || (countCode > _codeCount || countCode <= 0 || resultCode == 0))
            {
                _wrongNumberWindow.Enable();
                return null;
            }

            string number = $"+{resultCode}{resultNumber}";
            return number;
        }
    }
}
