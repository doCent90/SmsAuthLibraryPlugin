﻿using System;
using System.Collections;
using UnityEngine;
using SmsAuthAPI.Program;
using UnityEngine.Scripting;
using SmsAuthAPI.DTO;

namespace Agava.Wink
{
    /// <summary>
    ///     Starting auth services and cloud saves.
    /// </summary>
    [DefaultExecutionOrder(-123), Preserve]
    public class Boot : MonoBehaviour, IBoot
    {
        private const string FirsttimeStartApp = nameof(FirsttimeStartApp);
        private const float TimeOutTime = 60f;

        [SerializeField] private int _bundlIdVersion = 1;
        [SerializeField] private WinkAccessManager _winkAccessManager;
        [SerializeField] private WinkSignInHandlerUI _winkSignInHandlerUI;
        [SerializeField] private StartLogoPresenter _startLogoPresenter;
        [SerializeField] private SceneLoader _sceneLoader;
        [SerializeField] private LoadingProgressBar _loadingProgressBar;
        [SerializeField] private bool _restartAfterAuth = true;

        private Coroutine _signInProcess;
        private PreloadService _preloadService;
        private bool _bootStarted = false;

        public static Boot Instance { get; private set; }

        public event Action Restarted;

        private void OnDestroy()
        {
            _winkAccessManager.AuthorizationSuccessfully -= OnSuccessfully;
            _winkSignInHandlerUI?.Dispose();
        }

        private IEnumerator Start()
        {
            Debug.Log("Boot: Start plugin initialize");
            DontDestroyOnLoad(this);

            if (_winkSignInHandlerUI == null || _winkAccessManager == null)
                throw new NullReferenceException("Boot: Some Auth Component is Missing On Boot!");

            if (Instance == null)
                Instance = this;

            Debug.Log("Boot: Try Preload Service initialize");
            _preloadService = new(_winkSignInHandlerUI, _bundlIdVersion);
            Debug.Log("Boot: Try Wink Access Manager initialize");
            _winkAccessManager.Initialize();
            Debug.Log("Boot: Try Wink Access Manager initialized");
            _winkAccessManager.AuthorizationSuccessfully += OnSuccessfully;
            _startLogoPresenter.Construct();
            yield return _preloadService.Preparing();
            Debug.Log("Boot: Preload Service initialized");

            if (_preloadService.IsPluginAwailable)
            {
                yield return _winkSignInHandlerUI.Initialize();
                SmsAuthApi.DownloadCloudSavesProgress += OnDownloadCloudSavesProgress;

                _startLogoPresenter.ShowLogo();

                yield return _winkAccessManager.Construct();
                _winkSignInHandlerUI.StartSevice(_winkAccessManager);
                _winkSignInHandlerUI.Construct();
                _winkAccessManager.TryQuickAccess();

                yield return new WaitForSecondsRealtime(_startLogoPresenter.LogoDuration);
                yield return _startLogoPresenter.HidingLogo();

                _signInProcess = StartCoroutine(OnStarted());
                yield return _signInProcess;

                _bootStarted = true;

                _startLogoPresenter.CloseBootView();
                var loadingScene = _sceneLoader.LoadGameScene();
                SmsAuthApi.DownloadCloudSavesProgress -= OnDownloadCloudSavesProgress;
                _loadingProgressBar.Enable();

                yield return new WaitUntil(() => { _loadingProgressBar.SetProgress(loadingScene.progress, 0.5f, 1.0f); return loadingScene.isDone; });

                _loadingProgressBar.Disable();
                AnalyticsWinkService.SendStartApp(appId: Application.identifier);
            }
            else
            {
                yield return _winkSignInHandlerUI.Initialize();
                _loadingProgressBar.Disable();
                _startLogoPresenter.CloseBootView();
                _sceneLoader.LoadGameScene();
            }
        }

        private void OnDownloadCloudSavesProgress(float progress)
            => _loadingProgressBar.SetProgress(progress, 0.0f, 0.5f);

        private IEnumerator OnStarted()
        {
            yield return new WaitWhile(() => SmsAuthApi.Initialized == false);

            if (UnityEngine.PlayerPrefs.HasKey(FirsttimeStartApp) == false)
            {
                _winkSignInHandlerUI.OpenSignWindow();
                UnityEngine.PlayerPrefs.SetString(FirsttimeStartApp, "true");

                yield return new WaitUntil(() => (WinkAccessManager.Instance.HasAccess == true || _winkSignInHandlerUI.IsAnyWindowEnabled == false));

                if (WinkAccessManager.Instance.HasAccess)
                {
                    yield return CloudSavesLoading();
#if UNITY_EDITOR || TEST
                    Debug.Log($"Boot: App First Started. SignIn successfully");
#endif
                }
                else
                {
                    OnSkiped();
                }
            }
            else
            {
                if (UnityEngine.PlayerPrefs.HasKey(SmsAuthAPI.DTO.TokenLifeHelper.Tokens))
                {
                    yield return new WaitUntil(() => WinkAccessManager.Instance.Authenficated == true);

                    if (WinkAccessManager.Instance.HasAccess)
                        yield return CloudSavesLoading();
                    else
                        OnSkiped();
                }
                else
                {
                    OnSkiped();
                }
#if UNITY_EDITOR || TEST
                Debug.Log($"Boot: App Started. Authenficated: {WinkAccessManager.Instance.Authenficated}");
                Debug.Log($"Boot: App Started. Authorized: {WinkAccessManager.Instance.HasAccess}");
#endif
            }

            _signInProcess = null;
        }

        private void OnSuccessfully()
        {
            if (_bootStarted == false)
                return;

#if UNITY_EDITOR || TEST
            Debug.Log($"Boot: Access Successfully");
#endif
            StartCoroutine(Loading());
            IEnumerator Loading()
            {
                yield return CloudSavesLoading();
                Restarted?.Invoke();

                if (_restartAfterAuth)
                    _sceneLoader.LoadGameScene();
            }
        }

        private IEnumerator CloudSavesLoading()
        {
#if UNITY_EDITOR || TEST
            Debug.Log($"Boot: Try load cloud saves");
#endif

            Coroutine cancelation = null;
            cancelation = StartCoroutine(TimeOutWaiting());

            var task = SmsAuthAPI.Utility.PlayerPrefs.Load();
            yield return new WaitUntil(() => task.IsCompleted);

            if (cancelation != null)
                StopCoroutine(cancelation);
        }

        private IEnumerator TimeOutWaiting()
        {
            yield return new WaitForSecondsRealtime(TimeOutTime);

            StopCoroutine(_signInProcess);
            _winkSignInHandlerUI.CloseAllWindows();
            _winkSignInHandlerUI.OpenWindow(WindowType.Fail);

#if UNITY_EDITOR || TEST
            Debug.Log($"Boot: Time Out!");
#endif
        }

        private void OnSkiped()
        {
#if UNITY_EDITOR || TEST
            Debug.Log($"Boot: SignIn skiped");
#endif
        }
    }
}
