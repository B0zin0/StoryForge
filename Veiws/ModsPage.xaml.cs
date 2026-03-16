using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StoryForge.Models;

namespace StoryForge.Views
{
    public partial class ModsPage : Page
    {
        private static readonly string ModsFolder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mods");

        private static readonly string DisabledFolder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mods_disabled");

        private readonly ModsMetaStore _meta;

        public ModsPage()
        {
            InitializeComponent();
            _meta = MainWindow.ModsMeta;

            Directory.CreateDirectory(ModsFolder);
            Directory.CreateDirectory(DisabledFolder);

            Loaded += (_, _) => RefreshList();
        }

        private void RefreshList()
        {
            ModList.Children.Clear();

            var enabled  = Directory.GetFiles(ModsFolder);
            var disabled = Directory.GetFiles(DisabledFolder);

            if (enabled.Length == 0 && disabled.Length == 0)
            {
                ModList.Children.Add(new TextBlock
                {
                    Text       = "No mods installed yet.",
                    Foreground = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA)),
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize   = 11,
                    Margin     = new Thickness(8, 16, 8, 16)
                });
                return;
            }

            foreach (var f in enabled)  BuildCard(f, true);
            foreach (var f in disabled) BuildCard(f, false);
        }

        private void BuildCard(string filePath, bool isEnabled)
        {
            var name = Path.GetFileName(filePath);
            if (!_meta.Mods.TryGetValue(name, out var info))
                info = new ModInfo();

            var fileSize = new FileInfo(filePath).Length;
            var sizeStr  = fileSize > 1_048_576
                ? $"{fileSize / 1_048_576.0:F1} MB"
                : $"{fileSize / 1024.0:F0} KB";

            var card = new Border
            {
                Background      = new SolidColorBrush(isEnabled
                    ? Color.FromRgb(0x1A, 0x1A, 0x1A)
                    : Color.FromRgb(0x10, 0x10, 0x10)),
                BorderBrush     = new SolidColorBrush(isEnabled
                    ? Color.FromRgb(0x33, 0x33, 0x33)
                    : Color.FromRgb(0x22, 0x22, 0x22)),
                BorderThickness = new Thickness(1),
                Margin          = new Thickness(0, 0, 0, 4),
                Padding         = new Thickness(12, 10, 12, 10),
                Opacity         = isEnabled ? 1.0 : 0.55
            };

            var outer = new StackPanel();

            // Top row
            var topRow = new Grid();
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var leftStack = new StackPanel { Orientation = Orientation.Horizontal };

            // Category tag
            var catTag = new Border
            {
                Background      = CategoryColor(info.Category),
                CornerRadius    = new CornerRadius(3),
                Padding         = new Thickness(6, 2, 6, 2),
                Margin          = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            catTag.Child = new TextBlock
            {
                Text       = info.Category,
                Foreground = Brushes.Black,
                FontFamily = new FontFamily("Segoe UI"),
                FontWeight = FontWeights.Bold,
                FontSize   = 9
            };

            var nameBlock = new TextBlock
            {
                Text       = $"{name}  v{info.Version}",
                Foreground = new SolidColorBrush(isEnabled
                    ? Color.FromRgb(0xFF, 0xAA, 0x00)
                    : Color.FromRgb(0x77, 0x77, 0x77)),
                FontFamily = new FontFamily("Segoe UI"),
                FontWeight = FontWeights.Bold,
                FontSize   = 12,
                VerticalAlignment = VerticalAlignment.Center
            };

            var sizeBlock = new TextBlock
            {
                Text       = sizeStr,
                Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66)),
                FontFamily = new FontFamily("Segoe UI"),
                FontSize   = 10,
                VerticalAlignment = VerticalAlignment.Center,
                Margin     = new Thickness(10, 0, 0, 0)
            };

            leftStack.Children.Add(catTag);
            leftStack.Children.Add(nameBlock);
            leftStack.Children.Add(sizeBlock);
            Grid.SetColumn(leftStack, 0);

            // Buttons
            var btnRow = new StackPanel { Orientation = Orientation.Horizontal };

            var toggleBtn = new Button
            {
                Content         = isEnabled ? "DISABLE" : "ENABLE",
                Foreground      = Brushes.White,
                Background      = isEnabled
                    ? new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0xAA))
                    : new SolidColorBrush(Color.FromRgb(0x33, 0x88, 0x33)),
                BorderThickness = new Thickness(0),
                Padding         = new Thickness(10, 3, 10, 3),
                FontFamily      = new FontFamily("Segoe UI"),
                FontWeight      = FontWeights.Bold,
                FontSize        = 10,
                Cursor          = System.Windows.Input.Cursors.Hand,
                Margin          = new Thickness(0, 0, 6, 0),
                Tag             = (name, isEnabled)
            };
            toggleBtn.Click += Toggle_Click;

            var removeBtn = new Button
            {
                Content         = "REMOVE",
                Foreground      = Brushes.White,
                Background      = new SolidColorBrush(Color.FromRgb(0xFF, 0x44, 0x44)),
                BorderThickness = new Thickness(0),
                Padding         = new Thickness(10, 3, 10, 3),
                FontFamily      = new FontFamily("Segoe UI"),
                FontWeight      = FontWeights.Bold,
                FontSize        = 10,
                Cursor          = System.Windows.Input.Cursors.Hand,
                Tag             = (name, isEnabled)
            };
            removeBtn.Click += Remove_Click;

            btnRow.Children.Add(toggleBtn);
            btnRow.Children.Add(removeBtn);
            Grid.SetColumn(btnRow, 1);

            topRow.Children.Add(leftStack);
            topRow.Children.Add(btnRow);

            var desc = new TextBlock
            {
                Text         = info.Description,
                Foreground   = new SolidColorBrush(Color.FromRgb(0x77, 0x77, 0x77)),
                FontFamily   = new FontFamily("Segoe UI"),
                FontSize     = 10,
                Margin       = new Thickness(0, 5, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };

            outer.Children.Add(topRow);
            outer.Children.Add(desc);
            card.Child = outer;
            ModList.Children.Add(card);
        }

        private static SolidColorBrush CategoryColor(string cat) => cat switch
        {
            "Texture" => new SolidColorBrush(Color.FromRgb(0x00, 0xCC, 0xFF)),
            "Script"  => new SolidColorBrush(Color.FromRgb(0xFF, 0xAA, 0x00)),
            "Audio"   => new SolidColorBrush(Color.FromRgb(0x55, 0xFF, 0x55)),
            _         => new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA))
        };

        private void Toggle_Click(object s, RoutedEventArgs e)
        {
            if (s is not Button btn) return;
            var (name, isEnabled) = ((string, bool))btn.Tag;

            var src  = isEnabled ? ModsFolder    : DisabledFolder;
            var dest = isEnabled ? DisabledFolder : ModsFolder;

            var srcPath  = Path.Combine(src, name);
            var destPath = Path.Combine(dest, name);

            if (File.Exists(srcPath))
                File.Move(srcPath, destPath, overwrite: true);

            if (_meta.Mods.TryGetValue(name, out var info))
                info.Enabled = !isEnabled;

            _meta.Save();
            RefreshList();
        }

        private void Remove_Click(object s, RoutedEventArgs e)
        {
            if (s is not Button btn) return;
            var (name, isEnabled) = ((string, bool))btn.Tag;
            var folder = isEnabled ? ModsFolder : DisabledFolder;
            var path   = Path.Combine(folder, name);

            if (File.Exists(path)) File.Delete(path);
            _meta.Mods.Remove(name);
            _meta.Save();
            RefreshList();
        }

        private void Install_Click(object s, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Mod files|*.zip;*.landb;*.pak;*.d3dtx;*.bank;*.lua;*.ttarch2|All files|*.*",
                Title  = "Select mod file"
            };
            if (dlg.ShowDialog() != true) return;

            var win = Window.GetWindow(this);
            var dialog = new ModInstallDialog(win!);
            dialog.ShowDialog();
            if (!dialog.Confirmed) return;

            var fn   = Path.GetFileName(dlg.FileName);
            var dest = Path.Combine(ModsFolder, fn);
            File.Copy(dlg.FileName, dest, overwrite: true);

            _meta.Mods[fn] = new ModInfo
            {
                Description = dialog.ModDescription,
                Version     = dialog.ModVersion,
                Category    = dialog.ModCategory,
                Enabled     = true,
                FileSize    = new FileInfo(dest).Length
            };
            _meta.Save();
            RefreshList();
        }

        private void Back_Click(object s, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.Navigate(new HomePage());
        }
    }
}
