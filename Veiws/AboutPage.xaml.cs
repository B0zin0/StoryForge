using System.Collections.Generic;
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
            Loaded += (_, _) => Populate();
        }

        private void Populate()
        {
            InfoItems.ItemsSource = new List<InfoRow>
            {
                new("Version",      "1.1"),
                new("Made by",      "B0zin0"),
                new("Contributors", "xd89271"),
                new("Built with",   "C# + WPF (.NET 8)"),
                new("Supports",     "MCSM Season 1 & Season 2 + Mods"),
                new("Studio",       "StoryForge Studio — Open Beta April 2026"),
                new("Website",      "B0zin0.github.io"),
                new("Disclaimer",   "Not affiliated with Telltale Games or Mojang"),
                new("Warning",      "Always back up your saves before importing"),
            };
        }

        private void Back_Click(object s, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.Navigate(new HomePage());
        }
    }
}
