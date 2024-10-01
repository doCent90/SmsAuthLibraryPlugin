using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using AdsAppView.DTO;
using Newtonsoft.Json;
using System.Collections;
using System.IO;

namespace AdsAppView.Utility
{
    public class ViewPresenter : MonoBehaviour
    {
#if UNITY_STANDALONE
        private const string Platform = "standalone";
#elif UNITY_ANDROID
        private const string Platform = "android";
#elif UNITY_IOS
        private const string Platform = "ios";
#endif
        private const string ControllerName = "AdsApp";
        private const string SettingsRCName = "app-settings";
        private const string FilePathRCName = "file-path";
        private const string FtpCredsRCName = "ftp-creds";

        [Tooltip("Store name")]
        [SerializeField] private string _storeName;
        [Tooltip("Server name remote data")]
        [SerializeField] private string _serverPath;
#if UNITY_EDITOR
        [SerializeField] private Button _testGetTextureBtn;
        [SerializeField] private Image _testImage;
#endif
        private AdsAppAPI _api;

        private AppData _appData;
        private AppSettingsData _settingsData;
        private AdsFilePathsData _adsFilePathsData;
        private Sprite _sprite;

        private float _firstTimerSec = 60f;
        private float _regularTimerSec = 180f;
        private bool _carouselEnable = false;

        public string AppId => Application.identifier;

        private IEnumerator Start()
        {
            _appData = new() { app_id = AppId, store_id = _storeName, platform = Platform };
            _api = new(_serverPath, AppId);

            if (Application.internetReachability == NetworkReachability.NotReachable)
                yield return new WaitWhile(() => Application.internetReachability == NetworkReachability.NotReachable);

            StartView();
#if UNITY_EDITOR
            Debug.Log(JsonConvert.SerializeObject(_appData));
            _testGetTextureBtn.onClick.AddListener(GetTexture);
#endif
        }

        private async void StartView()
        {
            Response appSettingsResponse = await _api.GetRemoteConfig(ControllerName, SettingsRCName, _appData);

            if (appSettingsResponse.statusCode == UnityWebRequest.Result.Success)
            {
                AppSettingsData data = JsonConvert.DeserializeObject<AppSettingsData>(appSettingsResponse.body);

                if (data == null)
                {
                    Debug.LogError("App settings is null");
                }
                else
                {
                    _settingsData = data;
                    _firstTimerSec = data.first_timer;
                    _regularTimerSec = data.regular_timer;
                    _carouselEnable = data.carousel;

                    _sprite = await GetSprite();

                    if (_sprite != null)
                    {
                        StartCoroutine(ShowingAds());
                    }
                }
            }
            else
            {
                Debug.LogError("Fail to getting settings: " + appSettingsResponse.statusCode);
            }
        }

        private IEnumerator ShowingAds()
        {
            yield return null;

            _testImage.sprite = _sprite; //test
        }

        private async Task<Sprite> GetSprite()
        {
            AppData newData = new AppData() { app_id = _settingsData.ads_app_id, store_id = _storeName, platform = Platform };
            Response filePathResponse = await _api.GetRemoteConfig(ControllerName, FilePathRCName, newData);

            if (filePathResponse.statusCode == UnityWebRequest.Result.Success)
            {
                _adsFilePathsData = JsonConvert.DeserializeObject<AdsFilePathsData>(filePathResponse.body);

                if (_adsFilePathsData == null)
                    Debug.LogError("Fail get file path data");

                Response ftpCredentialResponse = await _api.GetRemoteConfig(ControllerName, FtpCredsRCName);

                if (ftpCredentialResponse.statusCode == UnityWebRequest.Result.Success)
                {
                    FtpCreds creds = JsonConvert.DeserializeObject<FtpCreds>(ftpCredentialResponse.body);

                    if (creds == null)
                        Debug.LogError("Fail get creds data");

                    Response textureResponse = _api.GetTextureData(creds.host, _adsFilePathsData.file_path, creds.login, creds.password);

                    if (textureResponse.statusCode == UnityWebRequest.Result.Success)
                    {
                        Texture2D texture = textureResponse.texture;
                        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    }
                    else
                    {
                        Debug.LogError("Fail to download texture: " + textureResponse.statusCode);
                        return null;
                    }
                }
                else
                {
                    Debug.LogError("Fail to getting ftp creds: " + ftpCredentialResponse.statusCode);
                    return null;
                }
            }
            else
            {
                Debug.LogError("Fail to getting file path: " + filePathResponse.statusCode);
                return null;
            }
        }

        #region TEST
#if UNITY_EDITOR
        private async void GetTexture()
        {
            var sprite = await GetSprite();
            _testImage.sprite = sprite;
        }
#endif
        #endregion
    }
}
