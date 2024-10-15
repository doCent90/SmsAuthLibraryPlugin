using System.IO;
using UnityEngine;

namespace AdsAppView.Utility
{
    public static class FileUtils
    {
        public static string ConstructFilePath(string filePath, string name)
        {
            string extension = Path.GetExtension(filePath);
            return Path.Combine(Application.persistentDataPath, name + extension);
        }

        public static bool TryLoadTexture(string cacheFilePath, out Texture2D texture)
        {
            texture = null;

            if (File.Exists(cacheFilePath))
            {
                byte[] rawData = File.ReadAllBytes(cacheFilePath);
                texture = new Texture2D(2, 2);
                texture.LoadImage(rawData);

#if UNITY_EDITOR
                Debug.Log($"#FileUtils# Cache texture loaded from path: {cacheFilePath}");
#endif
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log($"#FileUtils# Path {cacheFilePath} doesn't exist");
#endif
            }

            return texture != null;
        }

        public static void TrySaveTexture(string cacheFilePath, Texture2D texture)
        {
            try
            {
                File.WriteAllBytes(cacheFilePath, texture.EncodeToPNG());
#if UNITY_EDITOR
                Debug.Log($"#FileUtils# Cache texture saved to path: {cacheFilePath}");
#endif
            }
            catch (IOException exception)
            {
                Debug.LogError("#FileUtils# Fail to save cache texture: " + exception.Message);
            }
        }
    }
}
