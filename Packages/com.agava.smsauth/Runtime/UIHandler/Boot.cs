using System;
using System.Collections;
using UnityEngine;
using SmsAuthAPI.Program;

namespace Agava.Wink
{
    public class Boot : MonoBehaviour
    {
        private const string FirstShownSignIn = nameof(FirstShownSignIn);
        private const float TimeOutTime = 60f;

        [SerializeField] private MonoBehaviour _winkAccessManagerComponent;
        [SerializeField] private MonoBehaviour _winkSignInHandlerUIComponent;
        [SerializeField] private StartLogoPresenter _startLogoPresenter;
        [SerializeField] private SceneLoader _sceneLoader;
        [SerializeField] private float _logoDuration = 3f;

        private Coroutine _signInProcess;
        private IWinkAccessManager _winkSignInHandler;
        private IWinkSignInHandlerUI _winkSignInHandlerUI;

        private void Awake()
        {
            if (_winkSignInHandlerUIComponent == null || _winkAccessManagerComponent == null)
                throw new NullReferenceException("Some Wink Component is Missing On Boot!");

            _winkSignInHandler = (IWinkAccessManager)_winkAccessManagerComponent;
            _winkSignInHandlerUI = (IWinkSignInHandlerUI)_winkSignInHandlerUIComponent;

            _winkSignInHandler.Successfully += OnSuccessfully;

            DontDestroyOnLoad(this);
        }

        private IEnumerator Start()
        {
            _startLogoPresenter.ShowLogo();

            yield return new WaitForSecondsRealtime(_logoDuration);
            yield return _startLogoPresenter.HidingLogo();

            yield return new WaitWhile(() => Application.internetReachability == NetworkReachability.NotReachable);
            _winkSignInHandlerUI.CloseAllWindows();

            _signInProcess = StartCoroutine(OnStarted());
            yield return _signInProcess;

            _sceneLoader.LoadGameScene();
            _startLogoPresenter.CloseBootView();
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

        private void OnSuccessfully()
        {
            StartCoroutine(CloudSavesLoading());
            _sceneLoader.LoadGameScene();
        }

        private IEnumerator CloudSavesLoading()
        {
            Coroutine cancelation = null;
            cancelation = StartCoroutine(TimeOutWaiting());

            SmsAuthAPI.Utility.PlayerPrefs.Load();
            yield return new WaitWhile(() => SmsAuthAPI.Utility.PlayerPrefs.s_Loaded == false);

            if (cancelation != null)
                StopCoroutine(cancelation);
        }

        private IEnumerator TimeOutWaiting()
        {
            yield return new WaitForSecondsRealtime(TimeOutTime);
            StopCoroutine(_signInProcess);
            _winkSignInHandlerUI.CloseAllWindows();
            _winkSignInHandlerUI.OpenWindow(WindowType.Fail);
        }
    }
}