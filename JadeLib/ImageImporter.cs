using System.IO;
using UnityEngine;

namespace JadeLib
{
    public static class ImageImporter
    {
        public enum Error
        {
            NoError,
            PathNullOrEmpty,
            FileMissing,
            FailedToLoad
        }

        public static Sprite LoadFromFile(string path, out Error error)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                error = Error.PathNullOrEmpty;
            }
            else if (!File.Exists(path))
            {
                error = Error.FileMissing;
            }
            else
            {
                Texture2D texture = new(0, 0);
                if (!texture.LoadImage(File.ReadAllBytes(path)))
                {
                    error = Error.FailedToLoad;
                }
                else
                {
                    error = Error.NoError;
                    return Sprite.Create(texture, new(0f, 0f, texture.width, texture.height), new(0.5f, 0.5f));
                }
            }

            return null;
        }
    }
}
