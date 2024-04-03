using Newtonsoft.Json;
using UnityEngine;

namespace SmsAuthAPI.Utility
{
    public static class SaveLoadLocalDataService
    {
        public static void Save<T>(T obj, string saveName) where T : class
        {
            string data = JsonConvert.SerializeObject(obj);
            PlayerPrefs.SetString(saveName, data);
        }

        public static T Load<T>(string saveName) where T : class
        {
            if (PlayerPrefs.HasKey(saveName) == false)
            {
                Debug.LogError("Saves not exhist");
                return null;
            }

            var loadData = PlayerPrefs.GetString(saveName);
            T data = JsonConvert.DeserializeObject<T>(loadData);

            return data;
        }

        public static void Delete(string saveName)
        {
            PlayerPrefs.DeleteKey(saveName);
        }
    }
}
