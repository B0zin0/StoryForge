using System.Windows;

namespace StoryForge.Views
{
    public partial class ErrorDialog : Window
    {
        public ErrorDialog(string message, Window owner)
        {
            InitializeComponent();
            Owner = owner;
            MessageText.Text = message;
        }

        private void Ok_Click(object s, RoutedEventArgs e) => Close();
    }
}
