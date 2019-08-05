using System.Windows;

namespace F1TelemetryNetCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new F1TelemetryViewModel();
        }
    }
}
