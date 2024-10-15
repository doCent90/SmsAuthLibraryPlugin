﻿using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using AdsAppView.DTO;
using AdsAppView.Utility;
using Newtonsoft.Json;

namespace AdsAppView.Program
{
    public class PopupManager : MonoBehaviour
    {
        private const string ControllerName = "AdsApp";
        private const string SettingsRCName = "app-settings";
        private const string FilePathRCName = "file-path";
        private const string FtpCredsRCName = "ftp-creds";
        private const string CarouselPicture = "picrure";
        private const string Caching = "caching";
        private const int RetryCount = 3;
        private const int RetryDelayMlsec = 30000;

        [SerializeField] private MonoBehaviour _viewPresenterMonoBehaviour;

        public IViewPresenter ViewPresenter => _viewPresenterMonoBehaviour as IViewPresenter;

        private AppData _appData;
        private AppSettingsData _settingsData;
        private AdsFilePathsData _adsFilePathsData;

        private readonly List<PopupData> _popupDataList = new();
        private PopupData _popupData;

        private float _firstTimerSec = 60f;
        private float _regularTimerSec = 180f;
        private bool _caching = false;

        private void OnValidate()
        {
            if (_viewPresenterMonoBehaviour && !(_viewPresenterMonoBehaviour is IViewPresenter))
            {
                Debug.LogError(nameof(_viewPresenterMonoBehaviour) + " needs to implement " + nameof(IViewPresenter));
                _viewPresenterMonoBehaviour = null;
            }
        }

        public IEnumerator Construct(AppData appData)
        {
            DontDestroyOnLoad(gameObject);
            _appData = appData;

            if (Application.internetReachability == NetworkReachability.NotReachable)
                yield return new WaitWhile(() => Application.internetReachability == NetworkReachability.NotReachable);

            StartView();
        }

        private async void StartView()
        {
            Response appSettingsResponse = await AdsAppAPI.Instance.GetAppSettings(ControllerName, SettingsRCName, _appData);

            if (appSettingsResponse.statusCode == UnityWebRequest.Result.Success)
            {
                AppSettingsData data = JsonConvert.DeserializeObject<AppSettingsData>(appSettingsResponse.body);

                if (data != null)
                {
                    await SetCachingConfig();

                    _settingsData = data;
                    _firstTimerSec = data.first_timer;
                    _regularTimerSec = data.regular_timer;

                    _popupData = await GetPopupData();

                    if (_popupData != null)
                        StartCoroutine(ShowingAds());

                    if (_settingsData.carousel)
                        await FillPopupDataList();
                }
                else
                {
                    Debug.LogError("#PopupManager# App settings is null");
                }
            }
            else
            {
                Debug.LogError("#PopupManager# Fail to getting settings: " + appSettingsResponse.statusCode);
            }
        }

        private IEnumerator ShowingAds()
        {
            yield return new WaitForSecondsRealtime(_firstTimerSec);

            ViewPresenter.Show(_popupData);
            AnalyticsService.SendPopupView(_popupData.name);
            yield return new WaitWhile(() => ViewPresenter.Enable);

            if (_settingsData.carousel)
            {
                int index = 0;

                while (true)
                {
                    yield return new WaitForSecondsRealtime(_regularTimerSec);

                    ViewPresenter.Show(_popupDataList[index]);
                    yield return new WaitWhile(() => ViewPresenter.Enable);

                    index++;

                    if (index >= _popupDataList.Count)
                        index = 0;
                }
            }
            else
            {
                while (true)
                {
                    yield return new WaitForSecondsRealtime(_regularTimerSec);

                    ViewPresenter.Show(_popupData);
                    yield return new WaitWhile(() => ViewPresenter.Enable);
                }
            }
        }

        private async Task FillPopupDataList()
        {
            for (int i = 0; i < _settingsData.carousel_count; i++)
            {
                PopupData newSprite = null;

                for (int s = 0; s < RetryCount; s++)
                {
                    newSprite = await GetPopupData(index: i);

                    if (newSprite != null)
                        break;

                    await Task.Delay(RetryDelayMlsec);
                }

                newSprite ??= _popupData;
                _popupDataList.Add(newSprite);
            }
        }

        private async Task<PopupData> GetPopupData(int index = -1)
        {
            string appId = index == -1 ? _settingsData.ads_app_id : CarouselPicture + index;
            AppData newData = new AppData() { app_id = appId, store_id = _appData.store_id, platform = _appData.platform };

            Response filePathResponse = await AdsAppAPI.Instance.GetFilePath(ControllerName, FilePathRCName, newData);

            if (filePathResponse.statusCode == UnityWebRequest.Result.Success)
            {
                _adsFilePathsData = JsonConvert.DeserializeObject<AdsFilePathsData>(filePathResponse.body);

                if (_adsFilePathsData == null)
                    Debug.LogError("#PopupManager# Fail get file path data");

                Response ftpCredentialResponse = await AdsAppAPI.Instance.GetRemoteConfig(ControllerName, FtpCredsRCName);

                if (ftpCredentialResponse.statusCode == UnityWebRequest.Result.Success)
                {
                    FtpCreds creds = JsonConvert.DeserializeObject<FtpCreds>(ftpCredentialResponse.body);

                    if (creds == null)
                    {
                        Debug.LogError("#PopupManager# Fail get creds data");
                        return null;
                    }

                    string cacheTexturePath = FileUtils.ConstructFilePath(_adsFilePathsData.file_path, _adsFilePathsData.ads_app_id);

                    if ((_caching && FileUtils.TryLoadFile(cacheTexturePath, out byte[] bytes)) == false)
                    {
                        Response textureResponse = AdsAppAPI.Instance.GetBytesData(creds.host, _adsFilePathsData.file_path, creds.login, creds.password);

                        if (textureResponse.statusCode == UnityWebRequest.Result.Success)
                        {
                            bytes = textureResponse.bytes;

                            if (_caching)
                                FileUtils.TrySaveFile(cacheTexturePath, bytes);
                        }
                        else
                        {
                            Debug.LogError("#PopupManager# Fail to download texture: " + textureResponse.statusCode);
                            return null;
                        }
                    }

                    return new PopupData() { bytes = bytes, link = _adsFilePathsData.app_link, name = _adsFilePathsData.file_path };
                }
                else
                {
                    Debug.LogError("#PopupManager# Fail to getting ftp creds: " + ftpCredentialResponse.statusCode);
                    return null;
                }
            }
            else
            {
                Debug.LogError("#PopupManager# Fail to getting file path: " + filePathResponse.statusCode);
                return null;
            }
        }

        private async Task SetCachingConfig()
        {
            Response cachingResponse = await AdsAppAPI.Instance.GetRemoteConfig(Caching);

            if (cachingResponse.statusCode == UnityWebRequest.Result.Success)
            {
                string body = cachingResponse.body;

                if (bool.TryParse(body, out bool caching))
                {
                    _caching = caching;
#if UNITY_EDITOR
                    Debug.Log("#PopupManager# Caching set to: " + _caching);
#endif
                }
            }
            else
            {
                Debug.LogError("#PopupManager# Fail to Set Caching Config whith error: " + cachingResponse.statusCode);
            }
        }
    }
}
