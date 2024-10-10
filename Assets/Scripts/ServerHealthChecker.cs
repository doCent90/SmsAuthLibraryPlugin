using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UnityEngine.Networking.UnityWebRequest;

namespace Agava.ServerCheck
{
    public class ServerHealthChecker : MonoBehaviour
    {
        private const string RootApi = "/api";
        private const string AppJson = "application/json";
        private const string ContentType = "Content-Type";
        protected const int TimeOut = 59;
        protected enum RequestType { POST, GET, PUT }

        [SerializeField] private string _serverName;
        [SerializeField] private string _methodName = "accounttest/wink/healthcheck";
        [SerializeField] private Image _back;
        [SerializeField] private Text _text;

        private IEnumerator Start()
        {
            var cooldown = new WaitForSecondsRealtime(TimeOut);

            while (true)
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    _text.text = "No internet";
                    yield return new WaitWhile(() => Application.internetReachability == NetworkReachability.NotReachable);
                }

                _text.text = "Connection...";

                Task<Response> task = GetHealthData(_methodName);
                yield return new WaitUntil(() => task.IsCompleted);

                Result status = task.Result.statusCode;

                if (status == Result.Success)
                {
                    _back.color = Color.green;
                    _text.text = $"{task.Result.reasonPhrase} || {task.Result.body}";
                }
                else
                {
                    _back.color = Color.red;
                    _text.text = $"ERROR {task.Result.reasonPhrase} || {task.Result.body}";
                }

                yield return cooldown;
            }
        }

        private async Task<Response> GetHealthData(string apiName)
        {
            string path = $"{GetHttpPath(apiName)}";

            using (UnityWebRequest webRequest = CreateWebRequest(path, RequestType.GET))
            {
                webRequest.SendWebRequest();

                await WaitProccessing(webRequest);
                TryShowRequestInfo(webRequest, apiName);

                var body = JsonConvert.DeserializeObject<string>(webRequest.downloadHandler.text);
                return new Response(webRequest.result, webRequest.result.ToString(), body, false);
            }
        }

        private string GetHttpPath(string apiMethodName)
        {
            string path = $"{_serverName}/{apiMethodName.ToLower()}";
            string httpRequets = $"https://{path}";
            Debug.Log(httpRequets);
            return httpRequets;
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
