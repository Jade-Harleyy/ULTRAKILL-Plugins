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

            LogError(path, error);
            return null;
        }

        public static void LogError(string path, Error error)
        {
            string message = "Failed to load sprite";
            message += error switch
            {
                Error.PathNullOrEmpty => " - No path provided.",
                Error.FileMissing => $" - File at \"{path}\" does not exist or is protected.",
                Error.FailedToLoad => $" - Failed to load texture from file at \"{path}\".",
                _ => "."
            };

            string trace = StackTraceUtility.ExtractStackTrace();
            trace = trace[trace.IndexOf('\n')..]; // remove trace extraction
            message += $"\nStack trace:{trace}";
            
            Debug.LogWarning(message);
        }
    }
}
