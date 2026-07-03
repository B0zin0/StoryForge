using System;
using System.Windows;
using StoryForge.Views;

namespace StoryForge
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Build main window early (loads config, starts music, Discord RPC)
            // but don't show it yet — splash goes first
            var mainWindow = new MainWindow();

            var splash = new SplashWindow();
            splash.Show();

            // Splash handles its own fade-out internally after 4 steps × 480ms ≈ 2s
            // We wait the same duration then show the main window
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(2200)
            };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                mainWindow.Show();
            };
            timer.Start();
        }
    }
}
