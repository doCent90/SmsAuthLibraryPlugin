using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmsAuthAPI.DTO;

namespace SmsAuthAPI.Program
{
    public static class SmsAuthApi
    {
        private static HttpWebClient _httpClient;

        public static bool Initialized => _httpClient != null;

        public static void Initialize(string connectId)
        {
            if(string.IsNullOrEmpty(connectId))
                throw new InvalidOperationException(nameof(SmsAuthApi) + " fuction id not entered");

            if (Initialized)
                throw new InvalidOperationException(nameof(SmsAuthApi) + " has already been initialized");

            _httpClient = new HttpWebClient(connectId);
        }

        public async static Task<Response> Login(LoginData loginData)
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "LOGIN",
                body = JsonConvert.SerializeObject(loginData),
            };

            return await _httpClient.Post(request, loginData.phone);
        }

        public async static Task<Response> Regist(string phoneNumber)
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "registration",
                body = phoneNumber,
            };

            return await _httpClient.Post(request, phoneNumber);
        }

        public async static Task<Response> Refresh(string refreshToken)
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "REFRESH",
                body = refreshToken,
            };

            return await _httpClient.Post(request);
        }

        public async static Task<Response> Unlink(string accessToken, string deviceId)
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "UNLINK",
                body = deviceId,
                access_token = accessToken,
            };

            return await _httpClient.Post(request);
        }

        public async static Task<Response> SampleAuth(string accessToken)
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "SAMPLE_AUTH",
                access_token = accessToken,
            };

            return await _httpClient.Post(request);
        }

        public async static Task<Response> SetSave(string accessToken, string body)
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "SET_CLOUD_SAVES",
                body = body,
                access_token = accessToken,
            };

            return await _httpClient.Post(request);
        }

        public async static Task<Response> GetSave(string accessToken)
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "GET_CLOUD_SAVES",
                access_token = accessToken,
            };

            return await _httpClient.Post(request);
        }

        public async static Task<Response> GetDevices(string accessToken)
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "GET_DEVICES",
                access_token = accessToken,
            };

            return await _httpClient.Post(request);
        }

        public async static Task<Response> GetRemoteConfig(string remoteName)
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "remoteconfig",
                body = remoteName,
            };

            return await _httpClient.GetRemote(request, remoteName);
        }

        private static void EnsureInitialize()
        {
            if (Initialized == false)
                throw new InvalidOperationException(nameof(SmsAuthApi) + " is not initialized");
        }
    }
}
