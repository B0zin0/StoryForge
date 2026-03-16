using Microsoft.Win32;
using System.Windows;
using StoryForge.Models;

namespace StoryForge.Views
{
    public partial class SettingsPage : System.Windows.Controls.Page
    {
        private readonly Config _cfg;

        public SettingsPage()
        {
            InitializeComponent();
            _cfg = MainWindow.AppConfig;
            Loaded += (_, _) => Populate();
        }

        private void Populate()
        {
            S1PathBox.Text     = _cfg.S1Path;
            S2PathBox.Text     = _cfg.S2Path;
            MusicCheck.IsChecked = _cfg.Music;
        }

        private void Browse_S1(object s, RoutedEventArgs e) => BrowseExe(S1PathBox);
        private void Browse_S2(object s, RoutedEventArgs e) => BrowseExe(S2PathBox);

        private static void BrowseExe(System.Windows.Controls.TextBox box)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Executable|*.exe",
                Title  = "Select MCSM executable"
            };
            if (dlg.ShowDialog() == true)
                box.Text = dlg.FileName;
        }

        private void Save_Click(object s, RoutedEventArgs e)
        {
            _cfg.S1Path = S1PathBox.Text.Trim();
            _cfg.S2Path = S2PathBox.Text.Trim();
            _cfg.Music  = MusicCheck.IsChecked ?? true;
            _cfg.Save();
            SavedLabel.Text = "SAVED!";
        }

        private void Back_Click(object s, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.Navigate(new HomePage());
        }
    }
}
