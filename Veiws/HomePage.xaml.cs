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

        public HomePage()
        {
            InitializeComponent();
            _cfg = MainWindow.AppConfig;
            Loaded += (_, _) =>
            {
                BgVideo.Play();
            };
        }

        private void BgVideo_Ended(object s, RoutedEventArgs e)
        {
            BgVideo.Position = TimeSpan.Zero;
            BgVideo.Play();
        }

        private void S1_Enter(object s, MouseEventArgs e) =>
            ((Storyboard)Resources["S1GlowOn"]).Begin(this);

        private void S1_Leave(object s, MouseEventArgs e) =>
            ((Storyboard)Resources["S1GlowOff"]).Begin(this);

        private void S1_Click(object s, MouseButtonEventArgs e) =>
            LaunchOrSettings(_cfg.S1Path);

        private void S2_Enter(object s, MouseEventArgs e) =>
            ((Storyboard)Resources["S2GlowOn"]).Begin(this);

        private void S2_Leave(object s, MouseEventArgs e) =>
            ((Storyboard)Resources["S2GlowOff"]).Begin(this);

        private void S2_Click(object s, MouseButtonEventArgs e) =>
            LaunchOrSettings(_cfg.S2Path);

        private void LaunchOrSettings(string path)
        {
            if (!File.Exists(path))
            {
                if (Window.GetWindow(this) is MainWindow mw)
                    mw.Navigate(new SettingsPage());
                return;
            }

            // Tell MainWindow to pause music
            if (Window.GetWindow(this) is MainWindow mainWin)
                mainWin.PauseMusic();

            var proc = new Process
            {
                StartInfo           = new ProcessStartInfo(path) { UseShellExecute = true },
                EnableRaisingEvents = true
            };

            // Resume music when the game process exits
            proc.Exited += (_, _) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (Window.GetWindow(this) is MainWindow mw)
                        mw.ResumeMusic();
                });
            };

            proc.Start();
        }
    }
}
