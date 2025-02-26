using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
//using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using static CompModeling.ConnectToDB;
using System.Windows.Data;
using System.ComponentModel;

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
            "La + TBP",
        };

        // Представление коллекции для фильтрации
        //private ICollectionView _collectionView;
        //List<double>? lgs;

        enum SolutionMethods
        {
            INTERATION,
            NEWTON_RAPHSON
        }

        public SpecialistInterface()
        {
            InitializeComponent();
            LoadDataAsync();
            cb_mechanismName.ItemsSource = MethodsSol;
            tb_LgK6.Text = "-1,530";
            tb_LgK7.Text = "-9,5";
            tb_LgK8.Text = "1,3";
            tb_LgK9.Text = "-3,39";
            tb_LgK10.Text = "-2,931";
            tb_LgK11.Text = "0,1374";
            tb_LgK12.Text = "2,003";
            tb_LgK13.Text = "-0,805";
            tb_LgK14.Text = "-0,008";
            tb_LgK15.Text = "-0,620";
            var lgs = new List<double>()
            {
                1,
                1,
                1,
                1,
                1,
                Math.Pow(10, double.Parse(tb_LgK6.Text)),
                Math.Pow(10, double.Parse(tb_LgK7.Text)),
                Math.Pow(10, double.Parse(tb_LgK8.Text)),
                Math.Pow(10, double.Parse(tb_LgK9.Text)),
                Math.Pow(10, double.Parse(tb_LgK10.Text)),
                Math.Pow(10, double.Parse(tb_LgK11.Text)),
                Math.Pow(10, double.Parse(tb_LgK12.Text)),
                Math.Pow(10, double.Parse(tb_LgK13.Text)),
                Math.Pow(10, double.Parse(tb_LgK14.Text)),
                Math.Pow(10, double.Parse(tb_LgK15.Text))
            };
            IteratonMethod(lgs);
            //comboBoxSolutionMethod.ItemsSource = MethodsSol;
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

        private void IteratonMethod(List<double> lgs)
        {
            var K = lgs;
            List<double> b = [0.7298, 2.1994, 0.01, 3.661, Math.Pow(10, -6)];
            // Параметры решения
            int maxIterations = 5000;
            double epsilon = 0.01;
            //double[] XStart = new double[5] { 0.0233653054, 1.0715750606, 0.0023444158941, 1.6068767489, 0.00000099887410312 } ;
            double[] XStart = [0.0233, 1.0715, 0.00234, 1.6068, 0.00000099887410];
            double[] XNew = new double[5];
            int iteration = 0;
            double diff;
            // Начальное приближение
            //b.CopyTo(XStart, 0);

            do
            {
                // Вычисление новых значений
                XNew[0] = b[0] / (1 +
                    K[7] * XStart[1] +
                    K[12] * Math.Pow(XStart[1], 3) * Math.Pow(XStart[3], 3) +
                    K[13] * Math.Pow(XStart[1], 3) * Math.Pow(XStart[3], 4));

                XNew[1] = b[1] / (1 +
                    K[5] * XStart[2] +
                    K[6] * XStart[4] +
                    K[7] * XStart[0] +
                    K[8] * Math.Pow(XStart[3], 2) * XStart[4] +
                    3 * (K[9] * Math.Pow(XStart[1], 2) * Math.Pow(XStart[2], 3) * XStart[3]) +
                    K[10] * XStart[2] * XStart[3] +
                    2 * (K[11] * XStart[1] * Math.Pow(XStart[2], 2) * XStart[3]) +
                    3 * (K[12] * XStart[0] * Math.Pow(XStart[1], 2) * Math.Pow(XStart[3], 3)) +
                    3 * (K[13] * XStart[0] * Math.Pow(XStart[1], 2) * Math.Pow(XStart[3], 4)));

                XNew[2] = b[2] / (1 +
                    K[5] * XStart[1] +
                    K[9] * Math.Pow(XStart[1], 3) * Math.Pow(XStart[2], 2) * XStart[3] +
                    K[10] * XStart[1] * XStart[3] +
                    2 * (K[11] * Math.Pow(XStart[1], 2) * XStart[2] * XStart[3]));

                XNew[3] = b[3] / (1 +
                    2 * (K[8] * XStart[1] * XStart[3] * XStart[4]) +
                    K[9] * Math.Pow(XStart[1], 3) * Math.Pow(XStart[2], 3) +
                    K[10] * XStart[1] * XStart[2] +
                    K[11] * Math.Pow(XStart[1], 2) * Math.Pow(XStart[2], 2) +
                    3 * (K[12] * XStart[0] * Math.Pow(XStart[1], 3) * Math.Pow(XStart[3], 2)) +
                    4 * (K[13] * XStart[0] * Math.Pow(XStart[1], 3) * Math.Pow(XStart[3], 3)) +
                    2 * (K[14] * XStart[3]));

                XNew[4] = b[4] / (1 +
                    K[6] * XStart[1] +
                    K[8] * XStart[1] * Math.Pow(XStart[3], 2));

                // Вычисление максимального изменения
                diff = 0;
                for (int i = 0; i < 5; i++)
                {
                    double currentDiff = Math.Abs(XNew[i] - XStart[i]);
                    if (currentDiff > diff) diff = currentDiff;
                    XStart[i] = XNew[i]; // Обновление значений для следующей итерации
                }

                iteration++;
            }
            while (diff > epsilon && iteration < maxIterations);

            // Вывод результатов
            Console.WriteLine($"Итераций выполнено: {iteration}");
            Console.WriteLine("Результаты:");
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"x{i + 1} = {XNew[i]:E4}");
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
                    //filterForm.ItemsSource = forms;
                    //filterPhase.ItemsSource = phases;
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
