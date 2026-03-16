using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
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
                ((Storyboard)Resources["CardsIn"]).Begin(this);
            };
        }

        private void BgVideo_Ended(object s, RoutedEventArgs e)
        {
            BgVideo.Position = TimeSpan.Zero;
            BgVideo.Play();
        }

        // Season 1
        private void S1_Enter(object s, MouseEventArgs e)
        {
            _focusedSeason = 1;
            ((Storyboard)Resources["S1GlowOn"]).Begin(this);
        }
        private void S1_Leave(object s, MouseEventArgs e) =>
            ((Storyboard)Resources["S1GlowOff"]).Begin(this);
        private void S1_Click(object s, MouseButtonEventArgs e) =>
            LaunchOrError(_cfg.S1Path, "Season 1");

        // Season 2
        private void S2_Enter(object s, MouseEventArgs e)
        {
            _focusedSeason = 2;
            ((Storyboard)Resources["S2GlowOn"]).Begin(this);
        }
        private void S2_Leave(object s, MouseEventArgs e) =>
            ((Storyboard)Resources["S2GlowOff"]).Begin(this);
        private void S2_Click(object s, MouseButtonEventArgs e) =>
            LaunchOrError(_cfg.S2Path, "Season 2");

        // Enter key launches focused season
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Enter)
            {
                var path = _focusedSeason == 1 ? _cfg.S1Path : _cfg.S2Path;
                LaunchOrError(path, $"Season {_focusedSeason}");
            }
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
