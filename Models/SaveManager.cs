using System;
using System.IO;
using System.Linq;

namespace StoryForge.Models
{
    public static class SaveManager
    {
        private static readonly string[] S1SavePaths =
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Telltale Games", "Minecraft Story Mode"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Telltale Games", "Minecraft Story Mode"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Telltale Games", "Minecraft Story Mode"),
        };

        private static readonly string[] S2SavePaths =
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Telltale Games", "Minecraft Story Mode - Season Two"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Telltale Games", "Minecraft Story Mode - Season Two"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Telltale Games", "Minecraft Story Mode - Season Two"),
        };

        public static string? FindSaveFolder(int season)
        {
            var paths = season == 1 ? S1SavePaths : S2SavePaths;
            foreach (var path in paths)
                if (Directory.Exists(path)) return path;
            return null;
        }

        public static bool ExportSave(int season, string destinationZip)
        {
            var folder = FindSaveFolder(season);
            if (folder == null || !Directory.Exists(folder)) return false;

            try
            {
                if (File.Exists(destinationZip)) File.Delete(destinationZip);
                System.IO.Compression.ZipFile.CreateFromDirectory(folder, destinationZip);
                return true;
            }
            catch { return false; }
        }

        public static bool ImportSave(int season, string sourceZip)
        {
            var folder = FindSaveFolder(season);
            if (folder == null)
            {
                var paths = season == 1 ? S1SavePaths : S2SavePaths;
                folder = paths[0];
            }

            try
            {
                if (Directory.Exists(folder))
                {
                    var backup = folder + "_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    Directory.Move(folder, backup);
                }

                Directory.CreateDirectory(folder);
                System.IO.Compression.ZipFile.ExtractToDirectory(sourceZip, folder);
                return true;
            }
            catch { return false; }
        }

        public static string[] GetSaveFiles(int season)
        {
            var folder = FindSaveFolder(season);
            if (folder == null || !Directory.Exists(folder)) return Array.Empty<string>();
            return Directory.GetFiles(folder, "*", SearchOption.AllDirectories)
                            .OrderByDescending(File.GetLastWriteTime)
                            .ToArray();
        }
    }
}
