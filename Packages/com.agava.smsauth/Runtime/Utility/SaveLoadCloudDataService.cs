using Agava.Wink;
using Newtonsoft.Json;
using SmsAuthLibrary.DTO;
using SmsAuthLibrary.Program;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Utility
{
    public static class SaveLoadCloudDataService
    {
        public static async void SaveData<T>(T data) where T : class
        {
            Tokens tokens = GetTokens();

            if (await IsTokenAlive(tokens) == false)
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
            Tokens tokens = GetTokens();

            if (await IsTokenAlive(tokens) == false)
            {
                Debug.LogError("Tokens has expired");
                return null;
            }

            var response = await SmsAuthApi.GetSave(tokens.access);

            if (response.statusCode != (uint)YbdStatusCode.Success)
            {
                Debug.LogError("CloudSave -> fail to load: " +  response.statusCode + " Message: " + response.body);
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

        private static async Task<bool> IsTokenAlive(Tokens tokens)
        {
            if (TokenLifeHelper.IsTokenAlive(tokens.access))
            {
                return true;
            }
            else if (TokenLifeHelper.IsTokenAlive(tokens.refresh))
            {
                var currentToken = await TokenLifeHelper.GetRefreshedToken(tokens.refresh);

                if (string.IsNullOrEmpty(currentToken))
                    return false;
                else
                    return true;
            }
            else
            {
                SaveLoadLocalDataService.Delete(WinkAccessManager.Tokens);
                return false;
            }
        }

        private static Tokens GetTokens() => SaveLoadLocalDataService.Load<Tokens>(WinkAccessManager.Tokens);
    }
}
