using System.IO;
using System.Text.Json;

namespace StoryForge.Models
{
    public class Config
    {
        public string S1Path { get; set; } = "";
        public string S2Path { get; set; } = "";
        public bool   Music  { get; set; } = true;

        // ── File path next to the .exe ──────────────────────────────────────
        private static readonly string FilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        public static Config Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<Config>(json) ?? new Config();
                }
            }
            catch { /* first run or corrupt file — return defaults */ }
            return new Config();
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(this,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
    }
}
