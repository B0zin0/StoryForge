using System;
using System.Windows;
using System.Windows.Threading;

namespace StoryForge.Views
{
    public partial class SplashWindow : Window
    {
        private readonly DispatcherTimer _timer = new();
        private int _step = 0;

        private readonly (double progress, string message)[] _steps =
        {
            (0.25, "Initializing..."),
            (0.50, "Loading config..."),
            (0.80, "Loading mods..."),
            (1.00, "Ready!"),
        };

        public SplashWindow()
        {
            InitializeComponent();
            Loaded += (_, _) => StartProgress();
        }

        private void StartProgress()
        {
            _timer.Interval = TimeSpan.FromMilliseconds(450);
            _timer.Tick    += Tick;
            _timer.Start();
        }

        private void Tick(object? s, EventArgs e)
        {
            if (_step >= _steps.Length)
            {
                _timer.Stop();
                return;
            }

            var (progress, message) = _steps[_step++];
            ProgressBar.Width = 400 * progress;
            StatusLabel.Text  = message;
        }
    }
}
