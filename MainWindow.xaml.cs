using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DiscordRPC;
using StoryForge.Models;
using StoryForge.Views;

namespace StoryForge
{
    public partial class MainWindow : Window
    {
        public static Config        AppConfig { get; private set; } = Config.Load();
        public static ModsMetaStore ModsMeta  { get; private set; } = ModsMetaStore.Load();

        private readonly MediaPlayer   _music  = new();
        private          DiscordRpcClient? _discord;
        private static readonly HttpClient _http = new();

        public MainWindow()
        {
            InitializeComponent();

            // Apply saved window size
            Width  = AppConfig.WindowWidth;
            Height = AppConfig.WindowHeight;

            StartMusic();
            InitDiscord();
            CheckForUpdate();
            Navigate(new HomePage());
        }

        public static void ShowSplash()
        {
            var splash = new SplashWindow();
            splash.Show();
            // Auto-close after steps finish (4 steps × 450ms + buffer)
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(2200)
            };
            timer.Tick += (_, _) => { timer.Stop(); splash.Close(); };
            timer.Start();
        }

        private void StartMusic()
        {
            if (!AppConfig.Music) return;
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                    "Assets", "theme.mp3");
            if (!File.Exists(path)) return;

            _music.Open(new Uri(path));
            _music.Volume      = AppConfig.Volume;
            _music.MediaEnded += (_, _) => { _music.Position = TimeSpan.Zero; _music.Play(); };
            _music.Play();
        }

        public void SetVolume(double v)
        {
            _music.Volume      = v;
            AppConfig.Volume   = v;
        }

        public void PauseMusic() => _music.Pause();

        public void ResumeMusic()
        {
            if (AppConfig.Music) _music.Play();
        }

        private void InitDiscord()
        {
            try
            {
                _discord = new DiscordRpcClient("1483221024953602261"); // replace with your app id
                _discord.Initialize();
                _discord.SetPresence(new RichPresence
                {
                    Details = "StoryForge Launcher",
                    State   = "On the main menu",
                    Assets  = new Assets
                    {
                        LargeImageKey  = "storyforge",
                        LargeImageText = "StoryForge v1.0"
                    },
                    Timestamps = Timestamps.Now
                });
            }
            catch { /* Discord not running — silently skip */ }
        }

        public void SetDiscordState(string state)
        {
            try
            {
                _discord?.UpdateState(state);
            }
            catch { }
        }

        private async void CheckForUpdate()
        {
            try
            {
                _http.DefaultRequestHeaders.UserAgent
                     .ParseAdd("StoryForge/1.0");

                var url  = "https://api.github.com/repos/B0zin0/StoryForge/releases/latest";
                var json = await _http.GetStringAsync(url);
                using var doc = JsonDocument.Parse(json);

                var latest = doc.RootElement
                               .GetProperty("tag_name")
                               .GetString()
                               ?.TrimStart('v');

                var current = Assembly.GetExecutingAssembly()
                                      .GetName().Version?.ToString(3);

                if (latest != null && latest != current)
                {
                    UpdateBanner.Visibility = Visibility.Visible;
                    UpdateLabel.Text = $"Update available: v{latest} — click to download";
                }
            }
            catch { /* No internet or rate limited — silently skip */ }
        }

        private void UpdateBanner_Click(object s, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
                "https://github.com/B0zin0/StoryForge/releases/latest")
            { UseShellExecute = true });
        }

        public void ApplyPreset(int w, int h)
        {
            Width  = w;
            Height = h;
            AppConfig.WindowWidth  = w;
            AppConfig.WindowHeight = h;
            AppConfig.Save();
        }

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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
                Navigate(new HomePage());
            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.None)
                Navigate(new SettingsPage());
            else if (e.Key == Key.M && Keyboard.Modifiers == ModifierKeys.None)
                Navigate(new ModsPage());
        }

        private void TopBar_MouseDown(object s, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void Minimize_Click(object s, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void Close_Click(object s, RoutedEventArgs e)
        {
            _discord?.Dispose();
            Application.Current.Shutdown();
        }
    }
}