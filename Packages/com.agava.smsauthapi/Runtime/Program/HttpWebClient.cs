using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmsAuthAPI.DTO;
using UnityEngine;
using UnityEngine.Networking;

namespace SmsAuthAPI.Program
{
    internal class HttpWebClient
    {
        private enum RequestType { POST, GET}

        private readonly string _connectId;
        private readonly HttpClient _client;

        public HttpWebClient(string connectId)
        {
            _connectId = connectId;
            Debug.Log("http id: " + _connectId);
            _client = new HttpClient();
        }

        public void Authorize(string authToken)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        }

        public async Task<Response> Post(Request request, string phone = null)
        {
            Debug.Log("request start");

            string body = JsonConvert.SerializeObject(request.body);
            string path = $"https://{_connectId}/api/{request.method}/{phone}";
            string type = RequestType.POST.ToString();

            var webRequest = new UnityWebRequest(path, type);
            Debug.Log($"request at | {path} |");

            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            webRequest.SetRequestHeader("Content-Type", "application/json");

            webRequest.SendWebRequest();

            while (webRequest.isDone == false)
                await Task.Yield();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Response fail: {webRequest.error}, {webRequest.result}");
                return null;
            }

            Debug.Log("response done: " + webRequest.result.ToString());
            return JsonConvert.DeserializeObject<Response>(webRequest.downloadHandler.text);
        }

        public async Task<Response> GetRemote(Request request, string key)
        {
            string path = $"https://{_connectId}/api/{request.method.ToLower()}/{key.ToLower()}";
            Debug.Log($"request at | {path} |");

            string type = RequestType.GET.ToString();
            var webRequest = new UnityWebRequest(path, type);

            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            try
            {
                webRequest.SendWebRequest();
            }
            catch (HttpRequestException exception)
            {
                throw new HttpRequestException($"Response fail: {webRequest.error}", exception);
            }

            while (webRequest.isDone == false)
                await Task.Yield();

            Debug.Log("response done: " + webRequest.result.ToString());

            return new Response(webRequest.result, webRequest.result.ToString(), webRequest.downloadHandler.text, false);
        }
    }
}
