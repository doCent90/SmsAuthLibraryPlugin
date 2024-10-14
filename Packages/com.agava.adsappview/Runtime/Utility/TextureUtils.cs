using System.IO;
using UnityEngine;

namespace AdsAppView.Utility
{
    public static class TextureUtils
    {
        public static string ConstructCacheTexturePath(string filePath, string name)
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
                Debug.Log($"#TextureUtils# Cache texture loaded from path: {cacheFilePath}");
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
                Debug.Log($"#TextureUtils# Cache texture saved to path: {cacheFilePath}");
#endif
            }
            catch (IOException exception)
            {
                Debug.LogError("#TextureUtils# Fail to save cache texture: " + exception.Message);
            }
        }
    }
}
