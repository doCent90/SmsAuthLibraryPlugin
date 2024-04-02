using Newtonsoft.Json;
using UnityEngine;

namespace Utility
{
    internal static class SaveLoadLocalDataService
    {
        internal static void Save<T>(T obj, string saveName) where T : class
        {
            string data = JsonConvert.SerializeObject(obj);
            PlayerPrefs.SetString(saveName, data);
        }

        internal static T Load<T>(string saveName) where T : class
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

        internal static void Delete(string saveName)
        {
            PlayerPrefs.DeleteKey(saveName);
        }
    }
}
