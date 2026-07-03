using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using StoryForge.Models;

namespace StoryForge.Views
{
    public partial class HomePage : System.Windows.Controls.Page
    {
        private readonly Config _cfg;
        private int _focusedSeason = 1;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        private const int SW_MAXIMIZE = 3;
        private const int SW_RESTORE  = 9;

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
            if (!File.Exists(path))
            {
                Debug.WriteLine($"[StoryForge] Missing image asset: {path}");
                return;
            }

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource   = new Uri(path);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                img.Source = bmp;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StoryForge] Failed to load image '{path}': {ex.Message}");
            }
        }

        private void BgVideo_Ended(object s, RoutedEventArgs e)
        {
            BgVideo.Position = TimeSpan.Zero;
            BgVideo.Play();
        }

        private void Card_SizeChanged(object s, SizeChangedEventArgs e)
        {
            if (s is not FrameworkElement fe) return;
            var radius = (fe as Border)?.CornerRadius.TopLeft ?? 12;
            fe.Clip = new RectangleGeometry(new Rect(0, 0, e.NewSize.Width, e.NewSize.Height), radius, radius);
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

            if (_cfg.KillBackgroundApps)
                ProcessKiller.KillBackgroundApps();

            if (win != null)
                win.WindowState = WindowState.Minimized;

            var gameDir = Path.GetDirectoryName(path)!;

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName         = path,
                    WorkingDirectory = gameDir,
                    UseShellExecute  = true,
                    WindowStyle      = ProcessWindowStyle.Maximized
                },
                EnableRaisingEvents = true
            };

            proc.Exited += (_, _) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (win != null)
                        win.WindowState = WindowState.Normal;
                    win?.ResumeMusic();
                    win?.SetDiscordState("On the main menu");
                });
            };

            try
            {
                proc.Start();

                _ = Task.Run(async () =>
                {
                    for (int i = 0; i < 15; i++)
                    {
                        await Task.Delay(2000);
                        try
                        {
                            proc.Refresh();
                            if (proc.HasExited) break;

                            var hwnd = proc.MainWindowHandle;
                            if (hwnd != IntPtr.Zero)
                            {
                                ShowWindow(hwnd, SW_MAXIMIZE);
                                SetForegroundWindow(hwnd);
                            }
                        }
                        catch { }
                    }
                });
            }
            catch (Exception ex)
            {
                if (win != null)
                    win.WindowState = WindowState.Normal;
                win?.ResumeMusic();
                new ErrorDialog(
                    $"Failed to launch {seasonName}.\n\n{ex.Message}\n\nTry running StoryForge as Administrator.",
                    win!).ShowDialog();
            }
        }
    }
}
