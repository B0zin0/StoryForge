using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using StoryForge.Models;

namespace StoryForge.Views
{
    public partial class SettingsPage : Page
    {
        private readonly Config _cfg;
        private bool _loaded = false;

        public SettingsPage()
        {
            InitializeComponent();
            _cfg = MainWindow.AppConfig;
            Loaded += (_, _) => Populate();
        }

        private void Populate()
        {
            S1PathBox.Text       = _cfg.S1Path;
            S2PathBox.Text       = _cfg.S2Path;
            MusicCheck.IsChecked = _cfg.Music;
            VolumeSlider.Value   = _cfg.Volume;
            VolumeLabel.Text     = $"{(int)(_cfg.Volume * 100)}%";
            _loaded = true;
        }

        private void AutoDetect_Click(object s, RoutedEventArgs e)
        {
            DetectLabel.Text = "Searching... this may take a moment.";

            var (s1, s2) = GameFinder.FindPaths();

            if (!string.IsNullOrEmpty(s1)) S1PathBox.Text = s1;
            if (!string.IsNullOrEmpty(s2)) S2PathBox.Text = s2;

            if (!string.IsNullOrEmpty(s1) && !string.IsNullOrEmpty(s2))
                DetectLabel.Text = "Found both seasons automatically!";
            else if (!string.IsNullOrEmpty(s1))
                DetectLabel.Text = "Found Season 1. Season 2 not found — set it manually.";
            else if (!string.IsNullOrEmpty(s2))
                DetectLabel.Text = "Found Season 2. Season 1 not found — set it manually.";
            else
                DetectLabel.Text = "Could not find MCSM automatically. Please set paths manually.";
        }

        private void Browse_S1(object s, RoutedEventArgs e) => BrowseExe(S1PathBox);
        private void Browse_S2(object s, RoutedEventArgs e) => BrowseExe(S2PathBox);

        private static void BrowseExe(TextBox box)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Executable|*.exe",
                Title  = "Select MCSM executable"
            };
            if (dlg.ShowDialog() == true) box.Text = dlg.FileName;
        }

        private void Volume_Changed(object s, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_loaded) return;
            VolumeLabel.Text = $"{(int)(VolumeSlider.Value * 100)}%";
            if (Window.GetWindow(this) is MainWindow mw)
                mw.SetVolume(VolumeSlider.Value);
        }

        private void Music_Changed(object s, RoutedEventArgs e)
        {
            if (!_loaded) return;
            _cfg.Music = MusicCheck.IsChecked ?? true;
        }

        private void Preset_900(object s, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw) mw.ApplyPreset(900, 600);
        }
        private void Preset_1280(object s, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw) mw.ApplyPreset(1280, 720);
        }
        private void Preset_1600(object s, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw) mw.ApplyPreset(1600, 900);
        }
        private void Preset_Full(object s, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.WindowState = System.Windows.WindowState.Maximized;
        }

        private void Save_Click(object s, RoutedEventArgs e)
        {
            _cfg.S1Path = S1PathBox.Text.Trim();
            _cfg.S2Path = S2PathBox.Text.Trim();
            _cfg.Music  = MusicCheck.IsChecked ?? true;
            _cfg.Volume = VolumeSlider.Value;
            _cfg.Save();

            GameFinder.AutoAttachMods(_cfg.S1Path, System.IO.Path.Combine(
                System.AppDomain.CurrentDomain.BaseDirectory, "mods"));
            GameFinder.AutoAttachMods(_cfg.S2Path, System.IO.Path.Combine(
                System.AppDomain.CurrentDomain.BaseDirectory, "mods"));

            SavedLabel.Text = "SAVED!";
        }

        private void Back_Click(object s, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.Navigate(new HomePage());
        }
    }
}
