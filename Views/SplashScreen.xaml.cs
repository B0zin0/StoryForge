using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace StoryForge.Views
{
    public partial class SplashWindow : Window
    {
        private readonly DispatcherTimer _timer = new();
        private int _step = 0;

        // Progress steps: (target 0-1, status message)
        private readonly (double progress, string message)[] _steps =
        {
            (0.25, "Initializing..."),
            (0.55, "Loading config..."),
            (0.80, "Loading mods..."),
            (1.00, "Ready!"),
        };

        public SplashWindow()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                // Pull version from assembly — single source of truth
                var ver = Assembly.GetExecutingAssembly().GetName().Version;
                VersionText.Text = ver != null ? $"v{ver.ToString(3)}" : "v1.2.0";

                StartProgress();
            };
        }

        private void StartProgress()
        {
            _timer.Interval = TimeSpan.FromMilliseconds(480);
            _timer.Tick    += Tick;
            _timer.Start();
        }

        private void Tick(object? sender, EventArgs e)
        {
            if (_step >= _steps.Length)
            {
                _timer.Stop();
                // Smooth fade out before closing
                var fadeOut = (Storyboard)Resources["FadeOut"];
                fadeOut.Begin(this);
                return;
            }

            var (progress, message) = _steps[_step++];
            StatusLabel.Text = message;
            AnimateProgress(progress);
        }

        private void AnimateProgress(double targetFraction)
        {
            // Get the actual rendered width of the track (parent Border)
            var trackWidth = ((System.Windows.Controls.Border)ProgressFill.Parent).ActualWidth;
            var targetWidth = trackWidth * targetFraction;

            var anim = new DoubleAnimation
            {
                To             = targetWidth,
                Duration       = TimeSpan.FromMilliseconds(420),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            ProgressFill.BeginAnimation(WidthProperty, anim);
        }

        // Called when FadeOut storyboard completes
        private void FadeOut_Completed(object? sender, EventArgs e)
        {
            Close();
        }
    }
}
