using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DungeonCrawler.Core.Services
{
    public static class GameSerializer
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true,
            ReferenceHandler = ReferenceHandler.Preserve,
            Converters = { new JsonStringEnumConverter() }
        };

        public static void SerializeToFile(string filePath, object obj)
        {
            try
            {
                var json = JsonSerializer.Serialize(obj, _options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка сериализации: {ex.Message}", ex);
            }
        }

        public static T DeserializeFromFile<T>(string filePath) where T : class
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;
                var json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<T>(json, _options);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка десериализации: {ex.Message}", ex);
            }
        }
    }
}