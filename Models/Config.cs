using System;
using System.IO;
using System.Text.Json;

namespace StoryForge.Models
{
    public class Config
    {
        public string S1Path      { get; set; } = "";
        public string S2Path      { get; set; } = "";
        public bool   Music       { get; set; } = true;
        public double Volume      { get; set; } = 0.20;
        public int    WindowWidth { get; set; } = 1280;
        public int    WindowHeight{ get; set; } = 720;

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
            catch { }
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
