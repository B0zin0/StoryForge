using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace StoryForge.Models
{
    public static class GameFinder
    {
        private static readonly string[] S1Executables = { "MinecraftStoryMode.exe", "MCSM.exe", "mc_story_mode.exe" };
        private static readonly string[] S2Executables = { "MinecraftStoryModeSeason2.exe", "MCSMS2.exe", "mc_story_mode_s2.exe" };

        private static readonly string[] CommonBasePaths =
        {
            @"C:\Program Files\Minecraft Story Mode",
            @"C:\Program Files (x86)\Minecraft Story Mode",
            @"C:\Program Files\Telltale Games\Minecraft Story Mode",
            @"C:\Program Files (x86)\Telltale Games\Minecraft Story Mode",
            @"C:\Program Files\Minecraft Story Mode - Season Two",
            @"C:\Program Files (x86)\Minecraft Story Mode - Season Two",
            @"C:\Program Files\Telltale Games\Minecraft Story Mode - Season Two",
            @"C:\Program Files (x86)\Telltale Games\Minecraft Story Mode - Season Two",
        };

        public static (string s1, string s2) FindPaths()
        {
            var s1 = FindExe(S1Executables, "Season 1");
            var s2 = FindExe(S2Executables, "Season 2");
            return (s1, s2);
        }

        private static string FindExe(string[] exeNames, string season)
        {
            // Check common install paths first
            foreach (var basePath in CommonBasePaths)
                foreach (var exe in exeNames)
                {
                    var full = Path.Combine(basePath, exe);
                    if (File.Exists(full)) return full;
                }

            // Check Steam library paths from registry
            var steamPaths = GetSteamLibraryPaths();
            foreach (var lib in steamPaths)
                foreach (var exe in exeNames)
                {
                    var full = Path.Combine(lib, exe);
                    if (File.Exists(full)) return full;

                    var withFolder = Path.Combine(lib, "steamapps", "common",
                        season == "Season 1" ? "Minecraft Story Mode" : "Minecraft Story Mode Season 2", exe);
                    if (File.Exists(withFolder)) return withFolder;
                }

            // Search all drives as a last resort
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType != DriveType.Fixed) continue;
                try
                {
                    foreach (var exe in exeNames)
                    {
                        var results = SearchDrive(drive.RootDirectory, exe, 4);
                        if (!string.IsNullOrEmpty(results)) return results;
                    }
                }
                catch { }
            }

            return "";
        }

        private static string SearchDrive(DirectoryInfo dir, string exeName, int depth)
        {
            if (depth == 0) return "";
            try
            {
                foreach (var file in dir.GetFiles(exeName))
                    return file.FullName;

                foreach (var sub in dir.GetDirectories())
                {
                    var result = SearchDrive(sub, exeName, depth - 1);
                    if (!string.IsNullOrEmpty(result)) return result;
                }
            }
            catch { }
            return "";
        }

        private static List<string> GetSteamLibraryPaths()
        {
            var paths = new List<string>();
            try
            {
                var steamKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                var steamPath = steamKey?.GetValue("SteamPath") as string;
                if (!string.IsNullOrEmpty(steamPath))
                {
                    paths.Add(steamPath);
                    var libraryFolders = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
                    if (File.Exists(libraryFolders))
                    {
                        var lines = File.ReadAllLines(libraryFolders);
                        foreach (var line in lines)
                        {
                            if (line.Contains("\"path\""))
                            {
                                var parts = line.Split('"');
                                if (parts.Length >= 4)
                                    paths.Add(parts[3].Replace("\\\\", "\\"));
                            }
                        }
                    }
                }
            }
            catch { }
            return paths;
        }

        public static void AutoAttachMods(string gameExePath, string modsFolder)
        {
            if (string.IsNullOrEmpty(gameExePath) || !File.Exists(gameExePath)) return;
            if (!Directory.Exists(modsFolder)) return;

            var gameDir = Path.GetDirectoryName(gameExePath)!;
            var gameModsDir = Path.Combine(gameDir, "mods");
            Directory.CreateDirectory(gameModsDir);

            foreach (var mod in Directory.GetFiles(modsFolder))
            {
                var dest = Path.Combine(gameModsDir, Path.GetFileName(mod));
                try { File.Copy(mod, dest, overwrite: true); }
                catch { }
            }
        }
    }
}
