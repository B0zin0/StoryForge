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
                new("Version",     "1.0+"),
                new("Created by",  "B0zin0"),
                new("Built with",  "C# + WPF"),
                new("Supports",    "Season 1 & Season 2 + Mods"),
                new("Disclaimer",  "Not affiliated with Telltale Games"),
            };
        }

        private void Back_Click(object s, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.Navigate(new HomePage());
        }
    }
}
