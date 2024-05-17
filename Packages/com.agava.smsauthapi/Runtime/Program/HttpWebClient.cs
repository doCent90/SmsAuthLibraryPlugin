using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using SmsAuthAPI.DTO;

namespace SmsAuthAPI.Program
{
    internal class HttpWebClient
    {
        private const string RootApi = "api";
        private const string AppJson = "application/json";
        private const string ContentType = "Content-Type";

        private enum RequestType { POST, GET, PUT}

        private readonly string _connectId;
        private readonly HttpClient _client;

        public HttpWebClient(string connectId)
        {
            _connectId = connectId;
            _client = new HttpClient();
            Debug.Log("http id: " + _connectId);
        }

        public void Authorize(string authToken) => _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

        public async Task<Response> Login(Request request)
        {
            string path = $"{GetHttpType()}{_connectId}/{RootApi}/{request.method.ToLower()}";
            string type = RequestType.POST.ToString();

            var webRequest = new UnityWebRequest(path, type);

            webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(request.body));
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader(ContentType, AppJson);
            webRequest.SendWebRequest();

            while (webRequest.isDone == false)
                await Task.Yield();

            OnError(webRequest, request.method);
            return new Response(webRequest.result, webRequest.result.ToString(), webRequest.downloadHandler.text, false);
        }

        public async Task<Response> Regist(string methodName, string phone)
        {
            string path = $"{GetHttpType()}{_connectId}/{RootApi}/{methodName.ToLower()}/{phone}";
            string type = RequestType.POST.ToString();

            var webRequest = new UnityWebRequest(path, type);

            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader(ContentType, AppJson);
            webRequest.SendWebRequest();

            while (webRequest.isDone == false)
                await Task.Yield();

            OnError(webRequest, methodName);
            return new Response(webRequest.result, webRequest.result.ToString(), webRequest.downloadHandler.text, false);
        }

        public async Task<Response> Refresh(Request request)
        {
            string path = $"{GetHttpType()}{_connectId}/{RootApi}/{request.method.ToLower()}";
            string type = RequestType.POST.ToString();

            var webRequest = new UnityWebRequest(path, type);

            webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes($"\"{request.body}\""));
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader(ContentType, AppJson);
            webRequest.SendWebRequest();

            while (webRequest.isDone == false)
                await Task.Yield();

            OnError(webRequest, request.method);
            return new Response(webRequest.result, webRequest.result.ToString(), webRequest.downloadHandler.text, false);
        }

        public async Task<Response> Unlink(Request request)
        {
            Debug.Log("Body: " + request.body);

            string path = $"{GetHttpType()}{_connectId}/{RootApi}/{request.method.ToLower()}/{request.body}";
            string type = RequestType.POST.ToString();

            var webRequest = new UnityWebRequest(path, type);

            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader(ContentType, AppJson);
            webRequest.SetRequestHeader("Authorization", $"Bearer {request.access_token}");
            webRequest.SendWebRequest();

            while (webRequest.isDone == false)
                await Task.Yield();

            OnError(webRequest, request.method);
            return new Response(webRequest.result, webRequest.result.ToString(), webRequest.downloadHandler.text, false);
        }

        public async Task<Response> GetDevices(Request request)
        {
            string path = $"{GetHttpType()}{_connectId}/{RootApi}/{request.method.ToLower()}";
            string type = RequestType.GET.ToString();

            var webRequest = new UnityWebRequest(path, type);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader(ContentType, AppJson);
            webRequest.SetRequestHeader("Authorization", $"Bearer {request.access_token}");
            webRequest.SendWebRequest();

            while (webRequest.isDone == false)
                await Task.Yield();

            OnError(webRequest, request.method);
            return new Response(webRequest.result, webRequest.result.ToString(), webRequest.downloadHandler.text, false);
        }

        public async Task<Response> GetRemote(string methodName, string key)
        {
            string path = $"{GetHttpType()}{_connectId}/{RootApi}/{methodName.ToLower()}/{key.ToLower()}";
            string type = RequestType.GET.ToString();

            var webRequest = new UnityWebRequest(path, type);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader(ContentType, AppJson);
            webRequest.SendWebRequest();

            while (webRequest.isDone == false)
                await Task.Yield();

            OnError(webRequest, methodName);
            var body = JsonConvert.DeserializeObject<string>(webRequest.downloadHandler.text);
            return new Response(webRequest.result, webRequest.result.ToString(), body, false);
        }

        public async Task<Response> SetCloudData(Request request, string key)
        {
            Debug.Log("Body: " + request.body);

            string path = $"{GetHttpType()}{_connectId}/{RootApi}/{request.method.ToLower()}/{key.ToLower()}";
            string type = RequestType.PUT.ToString();

            var webRequest = new UnityWebRequest(path, type);
            webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(request.body));
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader(ContentType, AppJson);
            webRequest.SetRequestHeader("Authorization", $"Bearer {request.access_token}");
            webRequest.SendWebRequest();

            while (webRequest.isDone == false)
                await Task.Yield();

            OnError(webRequest, request.method);
            return new Response(webRequest.result, webRequest.result.ToString(), webRequest.downloadHandler.text, false);
        }

        public async Task<Response> GetCloudData(Request request)
        {
            Debug.Log("Body: " + request.body);
            await Task.Yield();

            string path = $"{GetHttpType()}{_connectId}/{RootApi}/{request.method.ToLower()}/{request.body.ToLower()}";
            string type = RequestType.GET.ToString();

            var webRequest = new UnityWebRequest(path, type);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader(ContentType, AppJson);
            webRequest.SetRequestHeader("Authorization", $"Bearer {request.access_token}");
            webRequest.SendWebRequest();

            Debug.Log("Header target: " + webRequest.GetRequestHeader("Authorization"));

            while (webRequest.isDone == false && webRequest.downloadProgress != 1)
                await Task.Yield();

            OnError(webRequest, request.method);
            var body = JsonConvert.DeserializeObject<string>(webRequest.downloadHandler.text);
            Debug.Log("Body recive: " + body);
            return new Response(webRequest.result, webRequest.result.ToString(), body, false);
        }

        private static string GetHttpType()
        {
#if UNITY_EDITOR && TEST
            return "http://";
#else
            return "https://";
#endif
        }

        private static void OnError(UnityWebRequest webRequest, string method)
        {
            Debug.Log($"response {method} done: " + webRequest.result.ToString());
            Debug.Log($"response {method} result: " + webRequest.downloadHandler.text);

            if (webRequest.result != UnityWebRequest.Result.Success)
                Debug.LogError($"Response  {method} fail: {webRequest.error}, {webRequest.result}");
        }
    }
}
