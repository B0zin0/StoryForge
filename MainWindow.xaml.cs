using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using StoryForge.Models;
using StoryForge.Views;

namespace StoryForge
{
    public partial class MainWindow : Window
    {
        public static Config        AppConfig { get; private set; } = Config.Load();
        public static ModsMetaStore ModsMeta  { get; private set; } = ModsMetaStore.Load();

        private readonly MediaPlayer _music = new();

        public MainWindow()
        {
            InitializeComponent();
            StartMusic();
            Navigate(new HomePage());
        }

        private void StartMusic()
        {
            if (!AppConfig.Music) return;

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                    "Assets", "theme.mp3");
            if (!File.Exists(path)) return;

            _music.Open(new Uri(path));
            _music.Volume           = 0.20;   
            _music.MediaEnded      += (_, _) =>
            {
                _music.Position = TimeSpan.Zero;
                _music.Play();
            };
            _music.Play();
        }

        public void PauseMusic()  => _music.Pause();
        public void ResumeMusic() => _music.Play();

        public void Navigate(System.Windows.Controls.Page page)
        {
            ContentFrame.Opacity = 0;
            ContentFrame.Navigate(page);
            var sb = (Storyboard)Resources["PageFadeIn"];
            sb.Begin(this);
        }

        private void Nav_About(object s, RoutedEventArgs e)    => Navigate(new AboutPage());
        private void Nav_Mods(object s, RoutedEventArgs e)     => Navigate(new ModsPage());
        private void Nav_Settings(object s, RoutedEventArgs e) => Navigate(new SettingsPage());

        private void TopBar_MouseDown(object s, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void Minimize_Click(object s, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void Close_Click(object s, RoutedEventArgs e) =>
            Application.Current.Shutdown();
    }
}
