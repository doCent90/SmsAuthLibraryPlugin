using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using SmsAuthAPI.Program;
using SmsAuthAPI.DTO;

namespace SmsAuthAPI.Utility
{
    public static class SaveLoadCloudDataService
    {
        public static async void SaveData<T>(T data) where T : class
        {
            Tokens tokens = TokenLifeHelper.GetTokens();

            if (await TokenLifeHelper.IsTokensAlive(tokens) == false)
            {
                Debug.LogError("Tokens has expired");
                return;
            }

            var body = JsonConvert.SerializeObject(data);
            var response = await SmsAuthApi.SetSave(tokens.access, body);

            if (response.statusCode != (uint)YbdStatusCode.Success)
                Debug.LogError("CloudSave -> fail to save: " + response.statusCode + " Message: " + response.body);
        }

        public static async Task<T> LoadData<T>() where T : class
        {
            Tokens tokens = TokenLifeHelper.GetTokens();

            if (await TokenLifeHelper.IsTokensAlive(tokens) == false)
            {
                Debug.LogError("Tokens has expired");
                return null;
            }

            var response = await SmsAuthApi.GetSave(tokens.access);

            if (response.statusCode != (uint)YbdStatusCode.Success)
            {
                Debug.LogError("CloudSave -> fail to load: " + response.statusCode + " Message: " + response.body);
                return null;
            }
            else
            {
                string json;
                if (response.isBase64Encoded)
                {
                    byte[] bytes = Convert.FromBase64String(response.body);
                    json = Encoding.UTF8.GetString(bytes);
                }
                else
                {
                    json = response.body;
                }

                T data = JsonConvert.DeserializeObject<T>(json);
                return data;
            }
        }
    }
}
