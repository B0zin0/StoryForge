using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace StoryForge.Models
{
    public static class GameFinder
    {
        private static readonly string[] S1Executables =
            { "MinecraftStoryMode.exe", "MCSM.exe", "mc_story_mode.exe" };
        private static readonly string[] S2Executables =
            { "MinecraftStoryModeSeason2.exe", "MCSMS2.exe", "mc_story_mode_s2.exe" };

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

        private static readonly HashSet<string> SkipFolders = new(StringComparer.OrdinalIgnoreCase)
        {
            "Windows", "WinSxS", "System32", "SysWOW64",
            "$Recycle.Bin", "$RECYCLE.BIN", "System Volume Information",
            "ProgramData", "AppData", "Temp", "tmp",
            "Recovery", "PerfLogs", "MSOCache",
            ".git", "node_modules", "__pycache__",
        };

        public static (string s1, string s2) FindPaths()
        {
            var s1 = FindExe(S1Executables, "Season 1");
            var s2 = FindExe(S2Executables, "Season 2");
            return (s1, s2);
        }

        private static string FindExe(string[] exeNames, string season)
        {
            foreach (var basePath in CommonBasePaths)
                foreach (var exe in exeNames)
                {
                    var full = Path.Combine(basePath, exe);
                    if (File.Exists(full)) return full;
                }

            foreach (var lib in GetSteamLibraryPaths())
                foreach (var exe in exeNames)
                {
                    var direct = Path.Combine(lib, exe);
                    if (File.Exists(direct)) return direct;

                    var inCommon = Path.Combine(lib, "steamapps", "common",
                        season == "Season 1"
                            ? "Minecraft Story Mode"
                            : "Minecraft Story Mode Season 2",
                        exe);
                    if (File.Exists(inCommon)) return inCommon;
                }

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType != DriveType.Fixed) continue;
                try
                {
                    foreach (var exe in exeNames)
                    {
                        var result = SearchDrive(drive.RootDirectory, exe, maxDepth: 5);
                        if (!string.IsNullOrEmpty(result)) return result;
                    }
                }
                catch { }
            }

            return "";
        }

        private static string SearchDrive(DirectoryInfo dir, string exeName, int maxDepth)
        {
            if (maxDepth == 0) return "";

            if (SkipFolders.Contains(dir.Name)) return "";

            try
            {
                foreach (var file in dir.GetFiles(exeName, SearchOption.TopDirectoryOnly))
                    return file.FullName;

                foreach (var sub in dir.GetDirectories())
                {
                    var result = SearchDrive(sub, exeName, maxDepth - 1);
                    if (!string.IsNullOrEmpty(result)) return result;
                }
            }
            catch { /* Access denied or locked directory — silently skip */ }

            return "";
        }

        private static List<string> GetSteamLibraryPaths()
        {
            var paths = new List<string>();
            try
            {
                var steamKey  = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                var steamPath = steamKey?.GetValue("SteamPath") as string;

                if (!string.IsNullOrEmpty(steamPath))
                {
                    paths.Add(steamPath);

                    var vdf = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
                    if (File.Exists(vdf))
                    {
                        foreach (var line in File.ReadAllLines(vdf))
                        {
                            if (!line.Contains("\"path\"")) continue;
                            var parts = line.Split('"');
                            if (parts.Length >= 4)
                                paths.Add(parts[3].Replace("\\\\", "\\"));
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

            var gameModsDir = Path.Combine(Path.GetDirectoryName(gameExePath)!, "mods");
            Directory.CreateDirectory(gameModsDir);

            foreach (var mod in Directory.GetFiles(modsFolder))
            {
                try
                {
                    File.Copy(mod, Path.Combine(gameModsDir, Path.GetFileName(mod)),
                              overwrite: true);
                }
                catch { }
            }
        }
    }
}
