using Microsoft.Win32;
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

        private readonly ModsMetaStore _meta;

        public ModsPage()
        {
            InitializeComponent();
            _meta = MainWindow.ModsMeta;

            if (!Directory.Exists(ModsFolder))
                Directory.CreateDirectory(ModsFolder);

            Loaded += (_, _) => RefreshList();
        }

        private void RefreshList()
        {
            ModList.Children.Clear();

            var files = Directory.GetFiles(ModsFolder);
            if (files.Length == 0)
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

            foreach (var file in files)
            {
                var name = Path.GetFileName(file);
                if (!_meta.Mods.TryGetValue(name, out var info))
                    info = new ModInfo();

                ModList.Children.Add(BuildCard(name, info));
            }
        }

        private UIElement BuildCard(string name, ModInfo info)
        {
            // Outer card border
            var card = new Border
            {
                Background       = new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x1A)),
                BorderBrush      = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)),
                BorderThickness  = new Thickness(1),
                Margin           = new Thickness(0, 0, 0, 4),
                Padding          = new Thickness(10, 8, 10, 8)
            };

            var outer = new StackPanel();

            var topRow = new Grid();
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameBlock = new TextBlock
            {
                Text       = $"{name}  v{info.Version}",
                Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xAA, 0x00)),
                FontFamily = new FontFamily("Segoe UI"),
                FontWeight = FontWeights.Bold,
                FontSize   = 12
            };
            Grid.SetColumn(nameBlock, 0);

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
                Tag             = name
            };
            removeBtn.Click += Remove_Click;
            Grid.SetColumn(removeBtn, 1);

            topRow.Children.Add(nameBlock);
            topRow.Children.Add(removeBtn);

            var desc = new TextBlock
            {
                Text       = info.Description,
                Foreground = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA)),
                FontFamily = new FontFamily("Segoe UI"),
                FontSize   = 10,
                Margin     = new Thickness(0, 4, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };

            outer.Children.Add(topRow);
            outer.Children.Add(desc);
            card.Child = outer;
            return card;
        }

        // ── Need to go back and redo this
        private void Install_Click(object s, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Mod files|*.zip;*.landb;*.pak|All files|*.*",
                Title  = "Select mod file"
            };
            if (dlg.ShowDialog() != true) return;

            var dest = Path.Combine(ModsFolder, Path.GetFileName(dlg.FileName));
            File.Copy(dlg.FileName, dest, overwrite: true);

            var fn = Path.GetFileName(dlg.FileName);
            if (!_meta.Mods.ContainsKey(fn))
                _meta.Mods[fn] = new ModInfo();

            _meta.Save();
            RefreshList();
        }

        private void Remove_Click(object s, RoutedEventArgs e)
        {
            if (s is not Button btn) return;
            var name = (string)btn.Tag;
            var path = Path.Combine(ModsFolder, name);
            if (File.Exists(path)) File.Delete(path);
            _meta.Mods.Remove(name);
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
