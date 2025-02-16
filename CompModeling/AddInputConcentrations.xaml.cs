using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static CompModeling.ConnectToDB;

namespace CompModeling
{
    /// <summary>
    /// Логика взаимодействия для AddInputConcentrations.xaml
    /// </summary>
    public partial class AddInputConcentrations : Window
    {
        ApplicationContext Context = new ApplicationContext();

        public InputConcentrations? Input { get; private set; }

        public AddInputConcentrations()
        {
            InitializeComponent();
            DataContext = Input;
        }

        private void addConcentration_Click(object sender, RoutedEventArgs e)
        {
            var Formname = addedName.Text;
            var FormValue = addedValue.Text;
            var FormPhase = addedPhase.Text;

            InputConcentrations inputConcentrations = new InputConcentrations();
            inputConcentrations.BaseForm = Formname;
            inputConcentrations.Value = double.Parse(FormValue);
            inputConcentrations.Phase = int.Parse(FormPhase);

            Context.InputConcentrations.Add(inputConcentrations);
            Context.SaveChanges();

            DialogResult = true;
        }

        private void clearAll_Click(object sender, RoutedEventArgs e)
        {
            addedName.Clear();
            addedPhase.Clear();
            addedValue.Clear();
        }
    }
}
