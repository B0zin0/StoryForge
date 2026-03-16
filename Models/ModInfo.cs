using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace StoryForge.Models
{
    public class ModInfo
    {
        public string Description { get; set; } = "No description";
        public string Version     { get; set; } = "1.0";
    }

    public class ModsMetaStore
    {
        // filename → metadata
        public Dictionary<string, ModInfo> Mods { get; set; } = new();

        private static readonly string FilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mods_meta.json");

        public static ModsMetaStore Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<ModsMetaStore>(json)
                           ?? new ModsMetaStore();
                }
            }
            catch { }
            return new ModsMetaStore();
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
    }
}
