using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using StoryForge.Models;

namespace StoryForge.Views
{
    public partial class HomePage : System.Windows.Controls.Page
    {
        private readonly Config _cfg;
        private int _focusedSeason = 1;

        public HomePage()
        {
            InitializeComponent();
            _cfg = MainWindow.AppConfig;
            Loaded += (_, _) =>
            {
                BgVideo.Play();
                LoadSeasonImages();
                ((Storyboard)Resources["CardsIn"]).Begin(this);
            };
        }

        private void LoadSeasonImages()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            LoadImage(Path.Combine(baseDir, "Assets", "season1.png"), S1Image);
            LoadImage(Path.Combine(baseDir, "Assets", "season2.png"), S2Image);
        }

        private static void LoadImage(string path, System.Windows.Controls.Image img)
        {
            if (!File.Exists(path)) return;
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource   = new Uri(path);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            img.Source = bmp;
        }

        private void BgVideo_Ended(object s, RoutedEventArgs e)
        {
            BgVideo.Position = TimeSpan.Zero;
            BgVideo.Play();
        }

        private void S1_Enter(object s, System.Windows.Input.MouseEventArgs e)
        {
            _focusedSeason = 1;
            ((Storyboard)Resources["S1GlowOn"]).Begin(this);
        }
        private void S1_Leave(object s, System.Windows.Input.MouseEventArgs e) =>
            ((Storyboard)Resources["S1GlowOff"]).Begin(this);
        private void S1_Click(object s, MouseButtonEventArgs e) =>
            LaunchOrError(_cfg.S1Path, "Season 1");

        private void S2_Enter(object s, System.Windows.Input.MouseEventArgs e)
        {
            _focusedSeason = 2;
            ((Storyboard)Resources["S2GlowOn"]).Begin(this);
        }
        private void S2_Leave(object s, System.Windows.Input.MouseEventArgs e) =>
            ((Storyboard)Resources["S2GlowOff"]).Begin(this);
        private void S2_Click(object s, MouseButtonEventArgs e) =>
            LaunchOrError(_cfg.S2Path, "Season 2");

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Enter)
                LaunchOrError(_focusedSeason == 1 ? _cfg.S1Path : _cfg.S2Path,
                              $"Season {_focusedSeason}");
        }

        private void LaunchOrError(string path, string seasonName)
        {
            var win = Window.GetWindow(this) as MainWindow;

            if (!File.Exists(path))
            {
                new ErrorDialog(
                    $"Could not find the {seasonName} executable.\n\nPlease go to Settings and set the correct path.",
                    win!).ShowDialog();
                return;
            }

            win?.PauseMusic();
            win?.SetDiscordState($"Playing {seasonName}");

            var proc = new Process
            {
                StartInfo           = new ProcessStartInfo(path) { UseShellExecute = true },
                EnableRaisingEvents = true
            };
            proc.Exited += (_, _) =>
            {
                Dispatcher.Invoke(() =>
                {
                    win?.ResumeMusic();
                    win?.SetDiscordState("On the main menu");
                });
            };
            proc.Start();
        }
    }
}
