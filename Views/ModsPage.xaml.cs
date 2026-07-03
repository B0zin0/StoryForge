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
            var total    = enabled.Length + disabled.Length;

            ModCountLabel.Text = total == 0
                ? "No mods installed yet"
                : $"{total} mod{(total != 1 ? "s" : "")} installed  ·  {enabled.Length} active  ·  {disabled.Length} disabled";

            if (total == 0)
            {
                var empty = new TextBlock
                {
                    Text       = "Drop mods here or click \"+ Install Mod\" to get started.",
                    Foreground = (Brush)Application.Current.Resources["Muted2Brush"],
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize   = 12,
                    Margin     = new Thickness(0, 24, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                ModList.Children.Add(empty);
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

            // Card using App.xaml resource brushes — reskin-safe
            var card = new Border
            {
                Background      = (Brush)Application.Current.Resources[
                                      isEnabled ? "Surface2Brush" : "SurfaceBrush"],
                BorderBrush     = (Brush)Application.Current.Resources["BorderBrush"],
                BorderThickness = new Thickness(1),
                CornerRadius    = new CornerRadius(8),
                Margin          = new Thickness(0, 0, 0, 6),
                Padding         = new Thickness(16, 12, 16, 12),
                Opacity         = isEnabled ? 1.0 : 0.6
            };

            var outer = new StackPanel();

            // Top row: category tag + name + size | buttons
            var topRow = new Grid();
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var leftStack = new StackPanel { Orientation = Orientation.Horizontal };

            // Category pill
            var catPill = new Border
            {
                Background        = CategoryBrush(info.Category),
                CornerRadius      = new CornerRadius(4),
                Padding           = new Thickness(7, 2, 7, 2),
                Margin            = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            catPill.Child = new TextBlock
            {
                Text       = info.Category,
                Foreground = new SolidColorBrush(Color.FromRgb(0x0f, 0x0f, 0x17)),
                FontFamily = new FontFamily("Segoe UI"),
                FontWeight = FontWeights.Bold,
                FontSize   = 9
            };

            var nameBlock = new TextBlock
            {
                Text              = $"{name}  v{info.Version}",
                Foreground        = (Brush)Application.Current.Resources[
                                        isEnabled ? "GoldBrush" : "Muted2Brush"],
                FontFamily        = new FontFamily("Segoe UI"),
                FontWeight        = FontWeights.SemiBold,
                FontSize          = 12,
                VerticalAlignment = VerticalAlignment.Center
            };

            var sizeBlock = new TextBlock
            {
                Text              = $"  ·  {sizeStr}",
                Foreground        = (Brush)Application.Current.Resources["Muted2Brush"],
                FontFamily        = new FontFamily("Segoe UI"),
                FontSize          = 11,
                VerticalAlignment = VerticalAlignment.Center
            };

            leftStack.Children.Add(catPill);
            leftStack.Children.Add(nameBlock);
            leftStack.Children.Add(sizeBlock);
            Grid.SetColumn(leftStack, 0);

            // Action buttons
            var btnRow = new StackPanel { Orientation = Orientation.Horizontal };

            var toggleBtn = new Button
            {
                Content         = isEnabled ? "Disable" : "Enable",
                Foreground      = new SolidColorBrush(Colors.White),
                Background      = isEnabled
                    ? new SolidColorBrush(Color.FromRgb(0x3b, 0x3b, 0x8f))
                    : new SolidColorBrush(Color.FromRgb(0x1a, 0x6b, 0x3a)),
                BorderThickness = new Thickness(0),
                Padding         = new Thickness(12, 5, 12, 5),
                FontFamily      = new FontFamily("Segoe UI"),
                FontWeight      = FontWeights.SemiBold,
                FontSize        = 11,
                Cursor          = System.Windows.Input.Cursors.Hand,
                Margin          = new Thickness(0, 0, 6, 0),
                Tag             = (name, isEnabled)
            };
            // Apply corner radius via template-less approach
            toggleBtn.Template = RoundedButtonTemplate(
                isEnabled
                    ? Color.FromRgb(0x3b, 0x3b, 0x8f)
                    : Color.FromRgb(0x1a, 0x6b, 0x3a));
            toggleBtn.Click += Toggle_Click;

            var removeBtn = new Button
            {
                Content         = "Remove",
                Foreground      = new SolidColorBrush(Colors.White),
                Background      = new SolidColorBrush(Color.FromRgb(0x7f, 0x1d, 0x1d)),
                BorderThickness = new Thickness(0),
                Padding         = new Thickness(12, 5, 12, 5),
                FontFamily      = new FontFamily("Segoe UI"),
                FontWeight      = FontWeights.SemiBold,
                FontSize        = 11,
                Cursor          = System.Windows.Input.Cursors.Hand,
                Tag             = (name, isEnabled)
            };
            removeBtn.Template = RoundedButtonTemplate(Color.FromRgb(0x7f, 0x1d, 0x1d));
            removeBtn.Click += Remove_Click;

            btnRow.Children.Add(toggleBtn);
            btnRow.Children.Add(removeBtn);
            Grid.SetColumn(btnRow, 1);

            topRow.Children.Add(leftStack);
            topRow.Children.Add(btnRow);

            // Description
            var desc = new TextBlock
            {
                Text         = info.Description,
                Foreground   = (Brush)Application.Current.Resources["MutedBrush"],
                FontFamily   = new FontFamily("Segoe UI"),
                FontSize     = 11,
                Margin       = new Thickness(0, 6, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };

            outer.Children.Add(topRow);
            outer.Children.Add(desc);
            card.Child = outer;
            ModList.Children.Add(card);
        }

        // Minimal rounded ControlTemplate for code-behind buttons
        private static ControlTemplate RoundedButtonTemplate(Color bg)
        {
            var tpl = new ControlTemplate(typeof(Button));
            var factory = new FrameworkElementFactory(typeof(Border));
            factory.SetValue(Border.BackgroundProperty, new SolidColorBrush(bg));
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
            factory.SetValue(Border.PaddingProperty, new Thickness(12, 5, 12, 5));
            var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            factory.AppendChild(presenter);
            tpl.VisualTree = factory;
            return tpl;
        }

        private static Brush CategoryBrush(string cat) => cat switch
        {
            "Texture" => new SolidColorBrush(Color.FromRgb(0x38, 0xbd, 0xf8)), // sky
            "Script"  => (Brush)Application.Current.Resources["GoldBrush"],
            "Audio"   => new SolidColorBrush(Color.FromRgb(0x4a, 0xde, 0x80)), // green
            _         => (Brush)Application.Current.Resources["MutedBrush"]
        };

        private void Toggle_Click(object s, RoutedEventArgs e)
        {
            if (s is not Button btn) return;
            var (name, isEnabled) = ((string, bool))btn.Tag;

            var src      = isEnabled ? ModsFolder    : DisabledFolder;
            var dest     = isEnabled ? DisabledFolder : ModsFolder;
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

            var win    = Window.GetWindow(this);
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
    }
}
