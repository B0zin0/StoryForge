using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using StoryForge.Models;

namespace StoryForge.Views
{
    public partial class SavesPage : Page
    {
        private DispatcherTimer? _statusTimer;

        public SavesPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await RefreshInfo();
        }

        private async Task RefreshInfo()
        {
            S1SavePath.Text  = "Searching...";
            S2SavePath.Text  = "Searching...";
            S1FileCount.Text = "…";
            S2FileCount.Text = "…";

            var (s1Folder, s1Files, s2Folder, s2Files) = await Task.Run(() =>
            {
                var f1    = SaveManager.FindSaveFolder(1);
                var files1 = SaveManager.GetSaveFiles(1);
                var f2    = SaveManager.FindSaveFolder(2);
                var files2 = SaveManager.GetSaveFiles(2);
                return (f1, files1, f2, files2);
            });

            // Season 1
            if (s1Folder != null)
            {
                S1SavePath.Text  = s1Folder;
                S1FileCount.Text = $"{s1Files.Length} file{(s1Files.Length != 1 ? "s" : "")}";
                S1Files.ItemsSource = s1Files
                    .Take(8) // show up to 8 most recent
                    .Select(f => Path.GetFileName(f))
                    .ToList();
            }
            else
            {
                S1SavePath.Text  = "Not found — launch Season 1 at least once.";
                S1FileCount.Text = "0 files";
                S1Files.ItemsSource = new List<string> { "(no save files found)" };
            }

            // Season 2
            if (s2Folder != null)
            {
                S2SavePath.Text  = s2Folder;
                S2FileCount.Text = $"{s2Files.Length} file{(s2Files.Length != 1 ? "s" : "")}";
                S2Files.ItemsSource = s2Files
                    .Take(8)
                    .Select(f => Path.GetFileName(f))
                    .ToList();
            }
            else
            {
                S2SavePath.Text  = "Not found — launch Season 2 at least once.";
                S2FileCount.Text = "0 files";
                S2Files.ItemsSource = new List<string> { "(no save files found)" };
            }
        }

        // ── Export ────────────────────────────────────────────────────────
        private void ExportS1_Click(object s, RoutedEventArgs e) => ExportSave(1);
        private void ExportS2_Click(object s, RoutedEventArgs e) => ExportSave(2);

        private void ExportSave(int season)
        {
            var dlg = new SaveFileDialog
            {
                Filter   = "StoryForge Save Backup|*.sfsave",
                FileName = $"MCSM_Season{season}_{DateTime.Now:yyyyMMdd_HHmm}"
            };
            if (dlg.ShowDialog() != true) return;

            var ok = SaveManager.ExportSave(season, dlg.FileName);
            ShowStatus(
                ok ? $"✓  Season {season} save exported successfully!"
                   : $"✗  Couldn't find Season {season} saves — play the game first.",
                ok);
        }

        // ── Import ────────────────────────────────────────────────────────
        private void ImportS1_Click(object s, RoutedEventArgs e) => _ = ImportSave(1);
        private void ImportS2_Click(object s, RoutedEventArgs e) => _ = ImportSave(2);

        private async Task ImportSave(int season)
        {
            var result = MessageBox.Show(
                $"This will replace your current Season {season} save.\n\n" +
                $"StoryForge will automatically back up your existing save before importing.\n\n" +
                $"Continue?",
                "StoryForge — Import Save",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            var dlg = new OpenFileDialog
            {
                Filter = "StoryForge Save Backup|*.sfsave|All files|*.*",
                Title  = $"Select Season {season} save backup"
            };
            if (dlg.ShowDialog() != true) return;

            ShowStatus("Importing...", null);

            var filePath = dlg.FileName;
            var ok       = await Task.Run(() => SaveManager.ImportSave(season, filePath));

            ShowStatus(
                ok ? $"✓  Season {season} imported! Old save was backed up automatically."
                   : "✗  Import failed — file may be corrupted or wrong format.",
                ok);

            await RefreshInfo();
        }

        // ── Status bar with auto-clear after 3 seconds ────────────────────
        private void ShowStatus(string message, bool? success)
        {
            StatusLabel.Text = message;

            StatusLabel.Foreground = success switch
            {
                true  => (Brush)Application.Current.Resources["GreenBrush"],
                false => (Brush)Application.Current.Resources["RedBrush"],
                null  => (Brush)Application.Current.Resources["GoldBrush"]
            };

            StatusBar.Visibility = Visibility.Visible;

            // Reset and restart auto-clear timer
            _statusTimer?.Stop();
            _statusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _statusTimer.Tick += (_, _) =>
            {
                _statusTimer.Stop();
                StatusBar.Visibility = Visibility.Collapsed;
            };
            _statusTimer.Start();
        }
    }
}
