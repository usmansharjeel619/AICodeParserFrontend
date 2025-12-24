using System.Windows;
using AICodeParser.ViewModels;

namespace AICodeParser
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void SimulinkButton_Click(object sender, RoutedEventArgs e)
        {
            var simulinkWindow = new SimulinkWindow();
            simulinkWindow.Show();
        }
    }
}