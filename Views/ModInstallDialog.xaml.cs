using System.Windows;
using System.Windows.Controls;

namespace StoryForge.Views
{
    public partial class ModInstallDialog : Window
    {
        public string  ModDescription { get; private set; } = "No description";
        public string  ModVersion     { get; private set; } = "1.0";
        public string  ModCategory    { get; private set; } = "Other";
        public bool    Confirmed      { get; private set; } = false;

        public ModInstallDialog(Window owner)
        {
            InitializeComponent();
            Owner = owner;
        }

        private void Install_Click(object s, RoutedEventArgs e)
        {
            ModDescription = string.IsNullOrWhiteSpace(DescBox.Text)
                ? "No description" : DescBox.Text.Trim();
            ModVersion  = string.IsNullOrWhiteSpace(VersionBox.Text)
                ? "1.0" : VersionBox.Text.Trim();
            ModCategory = ((ComboBoxItem)CategoryBox.SelectedItem)?.Content?.ToString()
                ?? "Other";
            Confirmed = true;
            Close();
        }

        private void Cancel_Click(object s, RoutedEventArgs e) => Close();
    }
}
