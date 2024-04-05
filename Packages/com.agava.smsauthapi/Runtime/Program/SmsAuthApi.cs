using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmsAuthAPI.DTO;

namespace SmsAuthAPI.Program
{
    public static class SmsAuthApi
    {
        private static YandexFunction _function;

        public static void Initialize(string functionId)
        {
            if(string.IsNullOrEmpty(functionId))
                throw new InvalidOperationException(nameof(SmsAuthApi) + " fuction id not entered");

            if (Initialized)
                throw new InvalidOperationException(nameof(SmsAuthApi) + " has already been initialized");

            _function = new YandexFunction(functionId);
        }

        public static bool Initialized => _function != null;

        public async static Task<Response> Login(LoginData loginData)
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "LOGIN",
                body = JsonConvert.SerializeObject(loginData),
            };

            return await _function.Post(request);
        }

        public async static Task<Response> Regist(string phoneNumber)
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "REGISTRATION",
                body = phoneNumber,
            };

            return await _function.Post(request);
        }

        public async static Task<Response> Refresh(string refreshToken)
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "REFRESH",
                body = refreshToken,
            };

            return await _function.Post(request);
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

            return await _function.Post(request);
        }

        public async static Task<Response> SampleAuth(string accessToken)
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "SAMPLE_AUTH",
                access_token = accessToken,
            };

            return await _function.Post(request);
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

            return await _function.Post(request);
        }

        public async static Task<Response> GetSave(string accessToken)
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "GET_CLOUD_SAVES",
                access_token = accessToken,
            };

            return await _function.Post(request);
        }

        public async static Task<Response> GetDevices(string accessToken)
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "GET_DEVICES",
                access_token = accessToken,
            };

            return await _function.Post(request);
        }

        public async static Task<Response> GetDemoTimer()
        {
            EnsureInitialize();

            var request = new Request()
            {
                method = "GET_DEMO_TIMER",
            };

            return await _function.Post(request);
        }

        private static void EnsureInitialize()
        {
            if (Initialized == false)
                throw new InvalidOperationException(nameof(SmsAuthApi) + " is not initialized");
        }
    }
}
