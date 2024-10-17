using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UnityEngine.Networking.UnityWebRequest;

namespace Agava.ServerCheck
{
    [Serializable]
    public class ServersData
    {
        public string serverName;
        public string methodName;
    }

    public class ServerHealthChecker : MonoBehaviour
    {
        private const string AppJson = "application/json";
        private const string ContentType = "Content-Type";
        protected const int TimeOut = 59;
        protected enum RequestType { POST, GET, PUT }

        [SerializeField] private ServersData[] _serverDatas;
        [SerializeField] private ServerView _template;
        [SerializeField] private RectTransform _viewContainer;
        [SerializeField] private Text _text;

        private List<ServerView> _serverViewes = new();

        private IEnumerator Start()
        {
            var cooldown = new WaitForSecondsRealtime(TimeOut);
            var waitSec = new WaitForSecondsRealtime(1f);

            for (int i = 0; i < _serverDatas.Length; i++)
            {
                var view = Instantiate(_template, _viewContainer);
                _serverViewes.Add(view);
            }

            while (true)
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    _text.text = "No internet";
                    yield return new WaitWhile(() => Application.internetReachability == NetworkReachability.NotReachable);
                }

                _text.text = "Connection...";
                yield return waitSec;
                bool hasError = false;

                for (int i = 0; i < _serverDatas.Length; i++)
                {
                    ServersData data = _serverDatas[i];
                    Task<Response> task = GetHealthData(data.serverName, data.methodName);
                    yield return new WaitUntil(() => task.IsCompleted);

                    Result status = task.Result.statusCode;

                    if (status == Result.Success)
                    {
                        var textStatusInfo = string.IsNullOrEmpty(task.Result.body) ? "Ok" : task.Result.body;
                        var text = $"{task.Result.reasonPhrase} || {textStatusInfo}";
                        _serverViewes[i].SetData(data.serverName, text, Color.green);
                    }
                    else
                    {
                        hasError = true;
                        var textStatusInfo = string.IsNullOrEmpty(task.Result.body) ? "Server dead" : task.Result.body;
                        var text = $"ERROR {task.Result.reasonPhrase} || {textStatusInfo}";
                        _serverViewes[i].SetData(data.serverName, text, Color.red);
                    }

                    if (hasError)
                        _text.text = "ERROR";
                    else
                        _text.text = "SUCCESS";

                    yield return waitSec;
                }

                yield return cooldown;
            }
        }

        private async Task<Response> GetHealthData(string serverName, string apiName)
        {
            string path = $"{GetHttpPath(serverName, apiName)}";

            using (UnityWebRequest webRequest = CreateWebRequest(path, RequestType.GET))
            {
                webRequest.SendWebRequest();

                await WaitProccessing(webRequest);
                TryShowRequestInfo(webRequest, apiName);

                var body = webRequest.downloadHandler.text;
                return new Response(webRequest.result, webRequest.result.ToString(), body, false);
            }
        }

        private string GetHttpPath(string serverName, string apiMethodName)
        {
            string path = $"{serverName}/{apiMethodName.ToLower()}";
            Debug.Log(path);
            return path;
        }

        private UnityWebRequest CreateWebRequest(string path, RequestType type)
        {
            var httpRequest = new UnityWebRequest(path, type.ToString());
            httpRequest.downloadHandler = new DownloadHandlerBuffer();

            httpRequest.SetRequestHeader(ContentType, AppJson);

            return httpRequest;
        }

        private async Task WaitProccessing(UnityWebRequest webRequest, Action<float> progress = null)
        {
            while (webRequest.result == UnityWebRequest.Result.InProgress)
            {
                progress?.Invoke(webRequest.downloadProgress);
                await Task.Yield();
            }
        }

        private void TryShowRequestInfo(UnityWebRequest webRequest, string method)
        {
            Debug.Log($"#WebClient# response {method} to {webRequest.url} done {webRequest.result}. Result: {webRequest.downloadHandler.text}");

            if (webRequest.result != UnityWebRequest.Result.Success)
                Debug.LogError($"#WebClient# Response {method} fail: {webRequest.error}, {webRequest.result}");
        }

        public class Request
        {
            public string api_name { get; set; }
            public string body { get; set; }
            public string access_token { get; set; }
            public string refresh_token { get; set; }
            public string login { get; set; }
            public string password { get; set; }
        }

        class Response
        {
            public Response(Result statusCode, string reasonPhrase, string body, bool isBase64Encoded)
            {
                this.statusCode = statusCode;
                this.body = body;
                this.reasonPhrase = reasonPhrase;
                this.isBase64Encoded = isBase64Encoded;
            }

            public Result statusCode { get; private set; }
            public string reasonPhrase { get; private set; }
            public string body { get; private set; }
            public bool isBase64Encoded { get; private set; }
        }
    }
}
