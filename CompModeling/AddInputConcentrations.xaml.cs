using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
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
using System.Collections.ObjectModel;

namespace CompModeling
{
    /// <summary>
    /// Логика взаимодействия для AddInputConcentrations.xaml
    /// </summary>
    public partial class AddInputConcentrations : Window
    {
        ApplicationContext Context = new ApplicationContext();

        ObservableCollection<InputConcentration>? InputConcentrations { get; set; }

        public AddInputConcentrations()
        {
            InitializeComponent();
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    // Получаем данные из таблицы InputConcentrations
                    var forms = await context.BaseForms.ToListAsync();
                    var phases = await context.Phases.ToListAsync();

                    // Привязываем данные к DataGrid
                    comboBoxFormName.ItemsSource = forms;
                    comboBoxPhase.ItemsSource = phases;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void addConcentration_Click(object sender, RoutedEventArgs e)
        {
            var Formname = (BaseForm)comboBoxFormName.SelectedItem;
            var FormValue = addedValue.Text.ToString();
            var FormPhase = comboBoxPhase.SelectedIndex;

            InputConcentration inputConcentrations = new InputConcentration();
            inputConcentrations.BaseForm = Formname.Name;
            inputConcentrations.Value = double.Parse(FormValue);
            inputConcentrations.Phase = FormPhase;

            Context.InputConcentrations.Add(inputConcentrations);
            ResearcherInterface.inputConcentrationsProp.Add(inputConcentrations);
            Context.SaveChanges();

            DialogResult = true;
        }

    }
}
