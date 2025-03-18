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
    public partial class ResearcherInterface : Window
    {
        ApplicationContext db = new ApplicationContext();
        private static ObservableCollection<InputConcentration>? InputConcentrations { get; set; }
        private static ObservableCollection<Mechanisms>? Mechanisms { get; set; }
        public static ObservableCollection<InputConcentration> inputConcentrationsProp => InputConcentrations!;

        enum SolutionMethods
        {
            INTERATION,
            NEWTON_RAPHSON
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
                    var mechanismsNames = await context.Mechanisms.ToListAsync();
                    cb_mechanismName.ItemsSource = mechanismsNames;

                    // Обновляем существующую коллекцию, а не пересоздаем
                    if (Mechanisms == null)
                    {
                        Mechanisms = new ObservableCollection<Mechanisms>(mechanismsNames);
                    }
                    else
                    {
                        Mechanisms.Clear();
                        foreach (var mechanism in mechanismsNames)
                        {
                            Mechanisms.Add(mechanism);
                        }
                    }
                    dataGrid_Mechanisms.ItemsSource = Mechanisms;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        public ResearcherInterface()
        {
            InitializeComponent();
            LoadDataAsync();
        }

        /// <summary>
        /// Добавление концентрации в DataGrid
        /// </summary>
        private void addInputConcentrations_Click(object sender, RoutedEventArgs e)
        {
            //AddInputConcentrations AddInputConcentrations = new AddInputConcentrations();
            //AddInputConcentrations.ShowDialog();
            //if (AddInputConcentrations.DialogResult == true) 
            //{
            //    inputConcentrations.Items.Refresh();   
            //}
        }

        private async void cb_mechanismName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    if (cb_mechanismName.SelectedItem is Mechanisms selectedMechanism)
                    {
                        var reactions = await context.ReactionMechanism
                        .Where(rm => rm.Mechanism_ID == selectedMechanism.ID)
                        .Join(context.Reactions,
                            rm => rm.Reaction_ID,
                            r => r.ID,
                            (rm, r) => new { r.Prod })
                        .ToListAsync();

                        // Очищаем панель
                        reactionInputsPanel.Children.Clear();

                        foreach (var reaction in reactions)
                        {
                            var grid = new Grid();
                            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(40, GridUnitType.Auto) });
                            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100, GridUnitType.Auto) });
                            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(70, GridUnitType.Auto) });
                            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(30, GridUnitType.Auto) });

                            //lgK
                            var labelLg = new TextBlock
                            {
                                Text = "lg K",
                                FontSize = 20,
                                FontStyle = FontStyles.Italic,
                                Width = 40
                            };

                            //Название формы
                            var textBlock = new TextBlock
                            {
                                Text = reaction.Prod,
                                FontStyle = FontStyles.Italic,
                                Width = 100
                            };

                            //Константа
                            var valueBox = new TextBox
                            {
                                Width = 70
                            };

                            //Единица измерения
                            var label = new Label
                            {
                                Content = "моль/см^3",
                                Margin = new Thickness(0, 0, 20, 0)
                            };

                            Grid.SetColumn(labelLg, 0);
                            Grid.SetColumn(textBlock, 1);
                            Grid.SetColumn(valueBox, 2);
                            Grid.SetColumn(label, 3);

                            grid.Children.Add(labelLg);
                            grid.Children.Add(textBlock);
                            grid.Children.Add(valueBox);
                            grid.Children.Add(label);

                            reactionInputsPanel.Children.Add(grid);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
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
            string result = "Итераций выполнено: ";
            result += iteration;
            result += "\nРезультаты:";
            for (int i = 0; i < 5; i++)
            {
                result += $"\nx{i + 1} = {XNew[i]:E4}";
            }
            tb_result.Text = result;
            Console.WriteLine($"Итераций выполнено: {iteration}");
            Console.WriteLine("Результаты:");
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"x{i + 1} = {XNew[i]:E4}");
            }

        }


        private void deleteInputConcentrations_Click(object sender, RoutedEventArgs e)
        {
            //var listToDelete = inputConcentrations.SelectedItems;
            //int max = listToDelete.Count;

            //for( int i = 0; i < max; i++) 
            //{
            //    db.InputConcentrations.Remove((InputConcentration)listToDelete[0]!);
            //    db.SaveChanges();
            //    InputConcentrations!.Remove((InputConcentration)listToDelete[0]!);
            //}
            //db.SaveChanges();
        }

        private void calculate_Click(object sender, RoutedEventArgs e)
        {
            var lgs = new List<double>()
            {
                1,
                1,
                1,
                1,
                1,
                //Math.Pow(10, double.Parse(tb_LgK6.Text)),
                //Math.Pow(10, double.Parse(tb_LgK7.Text)),
                //Math.Pow(10, double.Parse(tb_LgK8.Text)),
                //Math.Pow(10, double.Parse(tb_LgK9.Text)),
                //Math.Pow(10, double.Parse(tb_LgK10.Text)),
                //Math.Pow(10, double.Parse(tb_LgK11.Text)),
                //Math.Pow(10, double.Parse(tb_LgK12.Text)),
                //Math.Pow(10, double.Parse(tb_LgK13.Text)),
                //Math.Pow(10, double.Parse(tb_LgK14.Text)),
                //Math.Pow(10, double.Parse(tb_LgK15.Text))
            };
            IteratonMethod(lgs);
        }

        private void add_Mechanism_Click(object sender, RoutedEventArgs e)
        {
            AddMechanism addMechanism = new AddMechanism();

            addMechanism.Show();
        }

        //private async void delete_Mechanism_Click(object sender, RoutedEventArgs e)
        //{
        //    var button = (Button)sender;
        //    var mechanismId = (int)button.Tag;

        //    // Подтверждение удаления
        //    var result = MessageBox.Show("Удалить этот механизм?", "Подтверждение",
        //                                MessageBoxButton.YesNo,
        //                                MessageBoxImage.Warning);

        //    if (result != MessageBoxResult.Yes) return;

        //    try
        //    {
        //        using (var context = new ApplicationContext())
        //        {
        //            // Находим механизм и связанные реакции
        //            var mechanism = await context.Mechanisms
        //                .Include(m => m.ReactionMechanism)
        //                .FirstOrDefaultAsync(m => m.ID == mechanismId);

        //            if (mechanism == null)
        //            {
        //                MessageBox.Show("Механизм не найден!");
        //                return;
        //            }

        //            // Удаляем связи с реакциями
        //            context.ReactionMechanism.RemoveRange(mechanism.ReactionMechanisms);

        //            // Удаляем сам механизм
        //            context.Mechanisms.Remove(mechanism);

        //            await context.SaveChangesAsync();

        //            // Обновляем список
        //            await LoadDataAsync();
        //            dataGrid_Mechanisms.ItemsSource = _mechanisms;

        //            MessageBox.Show("Механизм успешно удален!");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка удаления: {ex.Message}");
        //    }
        //}


        //private async void delete_Mechanism_Click(object sender, RoutedEventArgs e)
        //{
        //    var button = (Button)sender;
        //    if (button.Tag is int mechanismId)
        //    {
        //        var result = MessageBox.Show("Вы уверены, что хотите удалить этот механизм?",
        //                                   "Подтверждение удаления",
        //                                   MessageBoxButton.YesNo,
        //                                   MessageBoxImage.Warning);

        //        if (result == MessageBoxResult.Yes)
        //        {
        //            try
        //            {
        //                using (var context = new ApplicationContext())
        //                {
        //                    // Находим механизм с зависимостями
        //                    var mechanism = await context.Mechanisms
        //                        .Where(m => m.ID == mechanismId)
        //                        .FirstOrDefaultAsync(m => m.ID == mechanismId);
        //                    var _mechanisms = await context.ReactionMechanism
        //                        .Where(rm => rm.ID == mechanismId)
        //                        .FirstOrDefaultAsync(m => m.ID == mechanismId);
        //                    {
        //                        // Удаляем связанные реакции
        //                        context.ReactionMechanism.RemoveRange(mechanism.ReactionMechanism);

        //                        // Удаляем сам механизм
        //                        context.Mechanisms.Remove(mechanism);

        //                        await context.SaveChangesAsync();

        //                        // Обновляем список механизмов
        //                        var mechanisms = await context.Mechanisms.ToListAsync();
        //                        dataGrid_Mechanisms.ItemsSource = mechanisms;

        //                        MessageBox.Show("Механизм успешно удален!");
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show($"Ошибка при удалении: {ex.Message}\n{ex.InnerException?.Message}");
        //            }
        //        }
        //    }
        //}

    }
}
