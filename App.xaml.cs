using System.Windows;
using StoryForge.Views;

namespace StoryForge
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = new MainWindow();

            // Show splash on top
            var splash = new SplashWindow();
            splash.Show();

            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = System.TimeSpan.FromMilliseconds(2200)
            };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                splash.Close();
                mainWindow.Show();
            };
            timer.Start();
        }
    }
}
