using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using StoryForge.Models;

namespace StoryForge.Views
{
    public partial class SavesPage : Page
    {
        public SavesPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await RefreshInfo();
        }

        private async Task RefreshInfo()
        {
            S1SaveInfo.Text = "Looking for saves...";
            S2SaveInfo.Text = "Looking for saves...";

            var s1 = await Task.Run(() => SaveManager.FindSaveFolder(1));
            var s2 = await Task.Run(() => SaveManager.FindSaveFolder(2));

            S1SaveInfo.Text = s1 != null
                ? $"Found at:\n{s1}\n\n{SaveManager.GetSaveFiles(1).Length} save file(s)"
                : "No saves found. Launch Season 1 at least once to generate a save.";

            S2SaveInfo.Text = s2 != null
                ? $"Found at:\n{s2}\n\n{SaveManager.GetSaveFiles(2).Length} save file(s)"
                : "No saves found. Launch Season 2 at least once to generate a save.";
        }

        private void ExportS1_Click(object s, RoutedEventArgs e) => ExportSave(1);
        private void ExportS2_Click(object s, RoutedEventArgs e) => ExportSave(2);
        private void ImportS1_Click(object s, RoutedEventArgs e) => ImportSave(1);
        private void ImportS2_Click(object s, RoutedEventArgs e) => ImportSave(2);

        private void ExportSave(int season)
        {
            var dlg = new SaveFileDialog
            {
                Filter   = "StoryForge Save Backup|*.sfsave",
                FileName = $"MCSM_Season{season}_Save"
            };
            if (dlg.ShowDialog() != true) return;

            var ok = SaveManager.ExportSave(season, dlg.FileName);
            StatusLabel.Foreground = ok
                ? (System.Windows.Media.Brush)Application.Current.Resources["GreenBrush"]
                : (System.Windows.Media.Brush)Application.Current.Resources["RedBrush"];
            StatusLabel.Text = ok
                ? $"Season {season} save exported successfully!"
                : $"Couldn't find Season {season} saves. Have you played it yet?";
        }

        private async void ImportSave(int season)
        {
            var result = MessageBox.Show(
                $"This will replace your current Season {season} save.\n\nStoryForge will back up your existing save automatically before importing.\n\nAre you sure?",
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

            StatusLabel.Foreground = (System.Windows.Media.Brush)Application.Current.Resources["GoldBrush"];
            StatusLabel.Text       = "Importing...";

            var filePath = dlg.FileName;
            var ok       = await Task.Run(() => SaveManager.ImportSave(season, filePath));

            StatusLabel.Foreground = ok
                ? (System.Windows.Media.Brush)Application.Current.Resources["GreenBrush"]
                : (System.Windows.Media.Brush)Application.Current.Resources["RedBrush"];
            StatusLabel.Text = ok
                ? $"Season {season} save imported! Your old save was backed up automatically."
                : "Import failed. The file might be corrupted or wrong format.";

            await RefreshInfo();
        }

        private void Back_Click(object s, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.Navigate(new HomePage());
        }
    }
}
