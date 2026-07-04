using DiscordRPC;
using StoryForge.Models;
using StoryForge.Views;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WpfButton = System.Windows.Controls.Button;
using WpfPage = System.Windows.Controls.Page;

namespace StoryForge
{
    public partial class MainWindow : Window
    {
        public static Config AppConfig { get; private set; } = Config.Load();
        public static ModsMetaStore ModsMeta { get; private set; } = ModsMetaStore.Load();

        public static bool? IsUpToDate { get; private set; } = null;
        public static string? LatestVersionTag { get; private set; } = null;

        private readonly MediaPlayer _music = new();
        private static readonly HttpClient _http = new();

        private WpfButton? _activeNavBtn;

        public MainWindow()
        {
            InitializeComponent();

            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            VersionLabel.Text = ver != null ? $"v{ver.ToString(3)}" : "v1.2.0";

            Width = AppConfig.WindowWidth;
            Height = AppConfig.WindowHeight;

            if (AppConfig.StartMaximized)
                WindowState = WindowState.Maximized;

            StartMusic();
            RpcClient.Initialize("1522797378162393179");
            RpcClient.SetPresence("In Launcher", "how r u even seeing this");
            CheckForUpdate();

            Navigate(new HomePage(), BtnHome);
        }

        private void StartMusic()
        {
            if (!AppConfig.Music) return;
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "theme.mp3");
            if (!File.Exists(path)) return;
            _music.Open(new Uri(path));
            _music.Volume = AppConfig.Volume;
            _music.MediaEnded += (_, _) => { _music.Position = TimeSpan.Zero; _music.Play(); };
            _music.Play();
        }

        public void SetVolume(double v) { _music.Volume = v; AppConfig.Volume = v; }
        public void PauseMusic() => _music.Pause();
        public void ResumeMusic() { if (AppConfig.Music) _music.Play(); }

        private static Version? NormalizeVersion(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            var core = raw.Split('-', '+')[0].Trim();
            var parts = core.Split('.');

            var nums = new int[4];
            for (int i = 0; i < 4; i++)
                nums[i] = (i < parts.Length && int.TryParse(parts[i], out var n)) ? n : 0;

            return new Version(nums[0], nums[1], nums[2], nums[3]);
        }

        private async void CheckForUpdate()
        {
            try
            {
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("StoryForge/1.2");
                var json = await _http.GetStringAsync(
                    "https://api.github.com/repos/B0zin0/StoryForge/releases/latest");
                using var doc = JsonDocument.Parse(json);

                var latestStr = doc.RootElement.GetProperty("tag_name").GetString()?.TrimStart('v', 'V');
                var currentStr = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

                var latestVer = NormalizeVersion(latestStr);
                var currentVer = NormalizeVersion(currentStr);

                if (latestVer != null && currentVer != null)
                {
                    LatestVersionTag = latestStr;

                    if (latestVer > currentVer)
                    {
                        IsUpToDate = false;
                        Dispatcher.Invoke(() =>
                        {
                            UpdateBanner.Visibility = Visibility.Visible;
                            UpdateLabel.Text = $"v{latestStr} is available — click to download";
                        });
                    }
                    else
                    {
                        IsUpToDate = true;
                        Dispatcher.Invoke(() => UpdateBanner.Visibility = Visibility.Collapsed);
                    }
                }
            }
            catch
            {
                IsUpToDate = null;
            }
        }

        private void UpdateBanner_Click(object s, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
                "https://github.com/B0zin0/StoryForge/releases/latest")
            { UseShellExecute = true });
        }

        private void Discord_Click(object s, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
                "https://discord.gg/br5z5a3GS8")
            { UseShellExecute = true });
        }

        private void YouTube_Click(object s, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
                "https://www.youtube.com/@BOZINOSUP/featured")
            { UseShellExecute = true });
        }

        public void ApplyPreset(int w, int h)
        {
            WindowState = WindowState.Normal;
            Width = w; Height = h;
            AppConfig.WindowWidth = w; AppConfig.WindowHeight = h;
            AppConfig.StartMaximized = false;
            AppConfig.Save();
        }

        public void ApplyFullscreenPreset()
        {
            WindowState = WindowState.Maximized;
            AppConfig.StartMaximized = true;
            AppConfig.Save();
        }

        public void Navigate(WpfPage page, WpfButton? navBtn = null)
        {
            ContentFrame.Opacity = 0;
            ContentFrame.Navigate(page);
            RpcClient.SetPresence("In Launcher", $"Viewing {page.Title}");
            ((Storyboard)Resources["PageFadeIn"]).Begin(this);
            if (navBtn != null) SetActiveNav(navBtn);
        }

        private void SetActiveNav(WpfButton btn)
        {
            if (_activeNavBtn != null)
            {
                _activeNavBtn.Foreground = (Brush)Application.Current.Resources["MutedBrush"];
                _activeNavBtn.Background = Brushes.Transparent;
            }
            btn.Foreground = (Brush)Application.Current.Resources["GoldBrush"];
            btn.Background = (Brush)Application.Current.Resources["Surface2Brush"];
            _activeNavBtn = btn;
        }

        private void Nav_Home(object s, RoutedEventArgs e) => Navigate(new HomePage(), BtnHome);
        private void Nav_About(object s, RoutedEventArgs e) => Navigate(new AboutPage(), BtnAbout);
        private void Nav_Saves(object s, RoutedEventArgs e) => Navigate(new SavesPage(), BtnSaves);
        private void Nav_Mods(object s, RoutedEventArgs e) => Navigate(new ModsPage(), BtnMods);
        private void Nav_Settings(object s, RoutedEventArgs e) => Navigate(new SettingsPage(), BtnSettings);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.Key)
            {
                case Key.Escape: Navigate(new HomePage(), BtnHome); break;
                case Key.S when Keyboard.Modifiers == ModifierKeys.None: Navigate(new SettingsPage(), BtnSettings); break;
                case Key.M when Keyboard.Modifiers == ModifierKeys.None: Navigate(new ModsPage(), BtnMods); break;
            }
        }

        private void TopBar_MouseDown(object s, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void Minimize_Click(object s, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void Close_Click(object s, RoutedEventArgs e)
        {
            RpcClient.Dispose();
            Application.Current.Shutdown();
        }
    }
}