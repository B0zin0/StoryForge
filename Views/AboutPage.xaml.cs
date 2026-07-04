using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace StoryForge.Views
{
    public record InfoRow(string Label, string Value);

    public partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();
            this.Title = "About";
            Loaded += (_, _) => Populate();
        }

        private void Populate()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            var verStr = ver != null ? $"v{ver.ToString(3)}" : "v1.2.0";
            VersionBadge.Text = verStr;

            string updateStatus = MainWindow.IsUpToDate switch
            {
                true  => "You're on the latest version ✓",
                false => $"Update available — v{MainWindow.LatestVersionTag}",
                null  => "Checking..."
            };

            InfoItems.ItemsSource = new List<InfoRow>
            {
                new("Version",      verStr),
                new("Updates",      updateStatus),
                new("Made by",      "Bozino"),
                new("Contributors", "xd89271 - Social Media Manager"),
                new("Built with",   "C# + WPF (.NET 8)"),
                new("Supports",     "MCSM Season 1 & Season 2 + Mods"),
                new("Studio",       "StoryForge Studio — Open Beta Now!"),
                new("GitHub",       "github.com/B0zin0/StoryForge"),
                new("Website",      "B0zin0.github.io"),
                new("Saves",        "Always back up before importing"),
            };
        }
    }
}
