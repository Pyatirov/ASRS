using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
//using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using static CompModeling.ConnectToDB;

namespace CompModeling
{
    /// <summary>
    /// Логика взаимодействия для SpecialistInterface.xaml
    /// </summary>
    public partial class SpecialistInterface : Window
    {
        ApplicationContext db = new ApplicationContext();
        private static ObservableCollection<InputConcentration>? InputConcentrations { get; set; }

        public static ObservableCollection<InputConcentration> inputConcentrationsProp => InputConcentrations!;

        public ObservableCollection<string> MethodsSol { get; set; } = new ObservableCollection<string>
        {
            "Итерационный",
            "Метод Ньютона-Рафсона"
        };

        enum SolutionMethods
        {
            INTERATION,
            NEWTON_RAPHSON
        }
        public SpecialistInterface()
        {
            InitializeComponent();
            LoadDataAsync();

            comboBoxSolutionMethod.ItemsSource = MethodsSol;
        }

        /// <summary>
        /// Добавление концентрации в DataGrid
        /// </summary>
        private void addInputConcentrations_Click(object sender, RoutedEventArgs e)
        {
            AddInputConcentrations AddInputConcentrations = new AddInputConcentrations();
            AddInputConcentrations.ShowDialog();
            if (AddInputConcentrations.DialogResult == true) 
            {
                inputConcentrations.Items.Refresh();   
            }
        }

        private async void LoadDataAsync()
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    // Получаем данные из таблицы InputConcentrations
                    var concentrations = await context.InputConcentrations.ToListAsync();
                    InputConcentrations = new ObservableCollection<InputConcentration>(concentrations);
                    var forms = await context.BaseForms.ToListAsync();
                    var phases = await context.Phases.ToListAsync();
                    
                    // Привязываем данные к DataGrid
                    inputConcentrations.ItemsSource = InputConcentrations;
                    filterForm.ItemsSource = forms;
                    filterPhase.ItemsSource = phases;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void deleteInputConcentrations_Click(object sender, RoutedEventArgs e)
        {
            var listToDelete = inputConcentrations.SelectedItems;
            int max = listToDelete.Count;

            for( int i = 0; i < max; i++) 
            {
                db.InputConcentrations.Remove((InputConcentration)listToDelete[0]!);
                db.SaveChanges();
                InputConcentrations!.Remove((InputConcentration)listToDelete[0]!);
            }
            db.SaveChanges();
        }
    }
}
