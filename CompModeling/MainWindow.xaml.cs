using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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