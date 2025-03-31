using System.Windows;

namespace CompModeling
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void researcherInterfaceButton_Click(object sender, RoutedEventArgs e)
        {
            ResearcherInterface newWindow = new ResearcherInterface();

            newWindow.Show();
        }

        private void specialistInterfaceButton_Click(object sender, RoutedEventArgs e)
        {
            SpecialistInterface newWindow = new SpecialistInterface();

            newWindow.Show();
        }
    }
}