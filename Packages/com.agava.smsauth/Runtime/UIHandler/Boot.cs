using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using SmsAuthAPI.Program;

namespace Agava.Wink
{
    public class Boot : MonoBehaviour
    {
        private const string FirstShownSignIn = nameof(FirstShownSignIn);
        private const float TimeOutTime = 60f;

        [SerializeField] private MonoBehaviour _winkSignInHandlerUIComponent;
        [SerializeField] private WindowPresenter _failWindow;
        [SerializeField] private StartLogoPresenter _startLogoPresenter;
        [SerializeField] private string _startSceneName;

        private Coroutine _signInProcess;
        private IWinkSignInHandlerUI _winkSignInHandlerUI;

        private void Awake()
        {
            if (string.IsNullOrEmpty(_startSceneName))
                throw new NullReferenceException("Start Name Scene is Empty on Boot!");

            if (_winkSignInHandlerUIComponent == null)
                throw new NullReferenceException("Wink SignIn HandlerUI Component is Empty On Boot!");

            DontDestroyOnLoad(this);
        }

        private IEnumerator Start()
        {
            _winkSignInHandlerUI = (IWinkSignInHandlerUI)_winkSignInHandlerUIComponent;

            _startLogoPresenter.ShowLogo();

            yield return new WaitForSecondsRealtime(3);
            yield return _startLogoPresenter.HidingLogo();

            yield return new WaitWhile(() => Application.internetReachability == NetworkReachability.NotReachable);
            _winkSignInHandlerUI.CloseAllWindows();

            _signInProcess = StartCoroutine(OnStarted());
            yield return _signInProcess;

            SceneManager.LoadScene(_startSceneName);
            gameObject.SetActive(false);
        }

        private IEnumerator OnStarted()
        {
            if (UnityEngine.PlayerPrefs.HasKey(FirstShownSignIn) == false)
            {
                yield return new WaitWhile(() => SmsAuthApi.Initialized == false);

                if (UnityEngine.PlayerPrefs.HasKey(SmsAuthAPI.DTO.TokenLifeHelper.Tokens) == false)
                {
                    _winkSignInHandlerUI.OpenSignWindow();
                    UnityEngine.PlayerPrefs.SetString(FirstShownSignIn, "true");

                    yield return new WaitUntil(() => (WinkAccessManager.Instance.HasAccess == true || _winkSignInHandlerUI.IsAnyWindowEnabled == false));

                    if (WinkAccessManager.Instance.HasAccess)
                        yield return CloudSavesLoading();
                    else
                        Debug.Log($"SignIn skiped");
                }
                else
                {
                    yield return new WaitUntil(() => WinkAccessManager.Instance.HasAccess == true);
                    yield return CloudSavesLoading();
                }
            }
            else
            {
                Debug.Log($"Wink Started");
            }

            _signInProcess = null;
        }

        private IEnumerator CloudSavesLoading()
        {
            Coroutine cancelation = null;
            cancelation = StartCoroutine(TimeOutWaiting());

            SmsAuthAPI.Utility.PlayerPrefs.Load();
            yield return new WaitWhile(() => SmsAuthAPI.Utility.PlayerPrefs.s_loaded == false);

            if (cancelation != null)
                StopCoroutine(cancelation);
        }

        private IEnumerator TimeOutWaiting()
        {
            yield return new WaitForSecondsRealtime(TimeOutTime);
            StopCoroutine(_signInProcess);
            _winkSignInHandlerUI.CloseAllWindows();
            _winkSignInHandlerUI.OpenWindow(_failWindow);
        }
    }
}