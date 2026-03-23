using System;
using System.Diagnostics;
using System.Linq;

namespace StoryForge.Models
{
    public static class ProcessKiller
    {
        private static readonly string[] TargetsToKill =
        {
            "spotify", "discord", "slack", "teams", "zoom",
            "chrome", "firefox", "msedge", "opera",
            "onedrive", "dropbox", "googledrivefs",
            "steam", "epicgameslauncher", "origin",
            "obs64", "obs32", "streamlabs"
        };

        public static int KillBackgroundApps()
        {
            int killed = 0;
            foreach (var name in TargetsToKill)
            {
                try
                {
                    var procs = Process.GetProcessesByName(name);
                    foreach (var proc in procs)
                    {
                        proc.Kill();
                        killed++;
                    }
                }
                catch { }
            }
            return killed;
        }

        public static string[] GetRunningTargets()
        {
            return TargetsToKill
                .Where(name => Process.GetProcessesByName(name).Length > 0)
                .ToArray();
        }
    }
}
