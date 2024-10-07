using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using AdsAppView.DTO;
using AdsAppView.Utility;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace AdsAppView.Program
{
    [Serializable]
    public class AssetsBundlesLoader
    {
#if UNITY_STANDALONE
        private const string Platform = "standalone";
#elif UNITY_ANDROID
        private const string Platform = "Android";
#elif UNITY_IOS
        private const string Platform = "Ios";
#endif

        class AssetPath
        {
            public string[] m_InternalIds;
        }

        private const string ControllerName = "AdsApp";
        private const string FtpCredsRCName = "ftp-creds";

        [SerializeField] private string _catalogPath;
        [SerializeField] private string _assetName;

        private AssetBundle _assetBundle;
        private string _nameBundle;
        private string _filePath;

        public void Unload()
        {
            _assetBundle.UnloadAsync(false);

            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }

        public async Task<GameObject> GetPopupObject()
        {
            Response ftpCredentialResponse = await AdsAppAPI.Instance.GetRemoteConfig(ControllerName, FtpCredsRCName);

            if (ftpCredentialResponse.statusCode == UnityWebRequest.Result.Success)
            {
                FtpCreds creds = JsonConvert.DeserializeObject<FtpCreds>(ftpCredentialResponse.body);

                if (creds == null)
                {
                    Debug.LogError("Fail get creds data");
                    return null;
                }

                string pathFile = DownloadConfigFile($"{_catalogPath}/{Platform}/catalog_{Application.version}.json", creds.login, creds.password);

                AssetPath path = JsonConvert.DeserializeObject<AssetPath>(pathFile);
                List<string> list = path.m_InternalIds.ToList();

                string assetPath = list.FirstOrDefault(s => s.StartsWith("http"));
                assetPath = assetPath.Replace("http", "ftp");
                Debug.Log(assetPath);

                _assetBundle = await DownloadAssetBundleFile(assetPath, savePath: Application.persistentDataPath, creds.login, creds.password);

                if (_assetBundle == null)
                {
                    Debug.LogError("Fail load bundle: " + _assetName);
                    return null;
                }

                GameObject target = _assetBundle.LoadAsset<GameObject>(_assetName);

                if (target == null)
                    Debug.LogError("Fail load obj from asset bundle: " + _assetName);

                return target;
            }
            else
            {
                return null;
            }
        }

        private string DownloadConfigFile(string ftpUrl, string userName, string password)
        {
            if (Uri.TryCreate(ftpUrl, UriKind.Absolute, out Uri uri) == false)
                throw new NullReferenceException("Cant create uri: " + ftpUrl);

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);

            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = true;

            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                request.Credentials = new NetworkCredential(userName, password);

            request.Method = WebRequestMethods.Ftp.DownloadFile;

            using (Stream input = request.GetResponse().GetResponseStream())
            {
                byte[] buffer = new byte[16 * 1024];
                using (MemoryStream ms = new())
                {
                    int read;

                    while (input.CanRead && (read = input.Read(buffer, 0, buffer.Length)) > 0)
                        ms.Write(buffer, 0, read);

                    return Encoding.Default.GetString(ms.ToArray());
                }
            }
        }

        private async Task<AssetBundle> DownloadAssetBundleFile(string ftpUrl, string savePath, string userName, string password)
        {
            if (Uri.TryCreate(ftpUrl, UriKind.Absolute, out Uri uri) == false)
                throw new NullReferenceException("Cant create uri: " + ftpUrl);

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);

            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = true;

            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                request.Credentials = new NetworkCredential(userName, password);

            request.Method = WebRequestMethods.Ftp.DownloadFile;

            ftpUrl = ftpUrl.Replace("ftp://ftp-p.ctcmedia.ru/mediartk/AssetsBundles/", "");
            ftpUrl = ftpUrl.Replace("/", "");
            ftpUrl = ftpUrl.Replace(Platform, "");
            Debug.Log("Asset bundle name to load: " + ftpUrl);

            string fileName = ftpUrl;
            _nameBundle = fileName;

            try
            {
                float timeout;
#if UNITY_EDITOR || TEST
                timeout = 30f;
#else
                timeout = 120f;
#endif
                AssetBundle assetBundle = null;
                assetBundle = await DownloadAndSave(request.GetResponse(), savePath, fileName);

                while (assetBundle == null && timeout > 0)
                {
                    await Task.Yield();
                    timeout -= Time.deltaTime;
                    Debug.Log(timeout + " " + assetBundle);
                }

                return assetBundle;
            }
            catch
            {
                Debug.LogError("Fail to download asset bundle #DownloadAssetBundleFile()#: " + fileName);
                return null;
            }
        }

        private async Task<AssetBundle> DownloadAndSave(WebResponse request, string savePath, string name)
        {
            savePath = savePath + "/Assets";
            Stream reader = request.GetResponseStream();

            if (Directory.Exists(savePath) == false)
            {
                Directory.CreateDirectory(savePath);
                Debug.Log($"Created folder: " + savePath);
            }
            else
            {
                Debug.Log($"Folder exist: " + savePath);
            }

            string path = string.IsNullOrEmpty(name) ? savePath : savePath + "/" + name;
            _filePath = path;
            FileStream fileStream = new(path, FileMode.Create);

            int bytesRead = 0;
            byte[] buffer = new byte[2048];

            while (true)
            {
                bytesRead = reader.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                    break;

                fileStream.Write(buffer, 0, bytesRead);
            }

            fileStream.Close();

            Debug.Log($"Try load resource {name} from: " + savePath);

            UnityWebRequest requestLocalLoad = UnityWebRequestAssetBundle.GetAssetBundle(path);
            requestLocalLoad.SendWebRequest();

            float timeout;
#if UNITY_EDITOR || TEST
            timeout = 30f;
#else
            timeout = 120f;
#endif
            Debug.Log($"Try load bundle from downloaded asset");
            while (requestLocalLoad.isDone == false && timeout > 0)
            {
                await Task.Yield();
                timeout -= Time.deltaTime;
                Debug.Log(timeout + " " + requestLocalLoad.isDone);
            }

            AssetBundle assetBundle = null;
            assetBundle = DownloadHandlerAssetBundle.GetContent(requestLocalLoad);
#if UNITY_EDITOR || TEST
            timeout = 30f;
#else
            timeout = 120f;
#endif
            Debug.Log($"Try get bundle from request");
            while (assetBundle == null && timeout > 0)
            {
                await Task.Yield();
                timeout -= Time.deltaTime;
                Debug.Log(timeout + " " + assetBundle);
            }

            Debug.Log($"Loaded bundle web {assetBundle.GetAllAssetNames()[0]}");
            return assetBundle;
        }
    }
}
