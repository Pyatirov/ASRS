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
using System.Windows.Media;

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

        private ObservableCollection<Mechanisms> _mechanisms = new();

        private ObservableCollection<ExperimentalPoints> _points = new();

        private List<BaseForm> _baseForms = new();

        private Dictionary<string, (TextBox AquaBox, TextBox OrgBox)> inputBoxes = new Dictionary<string, (TextBox, TextBox)>();

        private int currentPointId = 1;

        private async void LoadDataAsync()
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    var mechanismsNames = await context.Mechanisms.ToListAsync();
                    //Mechanisms = new ObservableCollection<Mechanisms>(mechanismsNames);

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
                    cb_mechanismName.ItemsSource = Mechanisms;
                    cbMechanisms.ItemsSource = Mechanisms;
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

        private void add_Mechanism_Click(object sender, RoutedEventArgs e)
        {
            AddMechanism addMechanism = new AddMechanism();

            addMechanism.ShowDialog();
            if (DialogResult == true) 
            { 
                LoadDataAsync();       
            }
        }

        private void update_Mechanism_Click(object sender, RoutedEventArgs e)
        {
            LoadDataAsync();
        }

        private async void delete_Mechanism_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var mechanismId = (int)button.Tag;

            // Подтверждение удаления
            var result = MessageBox.Show("Удалить этот механизм?", "Подтверждение",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                using (var context = new ApplicationContext())
                {
                    var mechanisms = await context.Mechanisms
                        .Where(m => m.ID == mechanismId)
                        .ToListAsync();
                    context.Mechanisms.RemoveRange(mechanisms);

                    await context.SaveChangesAsync();

                    MessageBox.Show("Механизм успешно удален!");

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}");
            }
        }

        private void cbMechanisms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            using (var context = new ApplicationContext())
            {
                if (cbMechanisms.SelectedItem is Mechanisms selectedMechanism)
                {
                    var baseFormNames = context.BaseForms;

                    // Очищаем панель
                    pointInputsPanel.Children.Clear();

                    foreach (var bFs in baseFormNames)
                    {
                        var grid = new Grid();
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(40, GridUnitType.Auto) });
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100, GridUnitType.Auto) });
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(70, GridUnitType.Auto) });
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(70, GridUnitType.Auto) });

                        //lgK
                        var bfName = new TextBlock
                        {
                            Text = bFs.Name,
                            FontSize = 20,
                            FontStyle = FontStyles.Italic,
                            Width = 40
                        };

                        var labelAq = new TextBlock
                        {
                            Text = "Вода",
                            Margin = new Thickness(0, 0, 0, 40)
                        };

                        var labelOrg = new TextBlock
                        {
                            Text = "Органика",
                            Margin = new Thickness(0, 0, 0, 40)
                        };

                        var valueAquBox = new TextBox
                        {
                            Width = 70
                        };

                        var valueOrgBox = new TextBox
                        {
                            Width = 70
                        };

                        var edinica = new TextBlock
                        {
                            Text = "моль/дм^3"
                        };

                        Grid.SetColumn(bfName, 0);
                        Grid.SetColumn(labelAq, 1);
                        Grid.SetColumn(valueAquBox, 1);
                        Grid.SetColumn(labelOrg, 2);
                        Grid.SetColumn(valueOrgBox, 2);
                        Grid.SetColumn(edinica, 3);

                        inputBoxes[bFs.Name] = (valueAquBox, valueOrgBox);

                        grid.Children.Add(bfName);
                        grid.Children.Add(labelAq);
                        grid.Children.Add(labelOrg);
                        grid.Children.Add(valueAquBox);
                        grid.Children.Add(valueOrgBox);
                        grid.Children.Add(edinica);
                        pointInputsPanel.Children.Add(grid);

                    }

                }
            }
        }

        private void Button_add_Experimental_Point_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new ApplicationContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var selectedMechanism = cbMechanisms.SelectedItem as Mechanisms;
                        if (selectedMechanism == null)
                        {
                            MessageBox.Show("Выберите механизм!");
                            return;
                        }

                        // Создаем новую точку
                        var point = new Points();
                        context.Points.Add(point);
                        context.SaveChanges(); // Получаем ID точки

                        foreach (var entry in inputBoxes)
                        {
                            var baseForm = context.BaseForms.FirstOrDefault(bf => bf.Name == entry.Key);
                            if (baseForm == null) continue;

                            // Сохраняем концентрацию для воды (фаза 1)
                            if (double.TryParse(entry.Value.AquaBox.Text, out double aquaValue))
                            {
                                var inputAqua = new InputConcentration
                                {
                                    BaseForm = entry.Key,
                                    Value = aquaValue,
                                    Phase = 1
                                };
                                context.InputConcentrations.Add(inputAqua);
                                context.SaveChanges();

                                var expPointAqua = new ExperimentalPoints
                                {
                                    ID_Point = point.ID,
                                    ID_InputConcentration = inputAqua.ID,
                                    ID_BaseForm = baseForm.ID,
                                    Phase = 1,
                                    ID_Mechanism = selectedMechanism.ID
                                };
                                context.ExperimentalPoints.Add(expPointAqua);
                            }

                            // Сохраняем концентрацию для органики (фаза 2)
                            if (double.TryParse(entry.Value.OrgBox.Text, out double orgValue))
                            {
                                var inputOrg = new InputConcentration
                                {
                                    BaseForm = entry.Key,
                                    Value = orgValue,
                                    Phase = 0,
                                };
                                context.InputConcentrations.Add(inputOrg);
                                context.SaveChanges();

                                var expPointOrg = new ExperimentalPoints
                                {
                                    ID_Point = point.ID,
                                    ID_InputConcentration = inputOrg.ID,
                                    ID_BaseForm = baseForm.ID,
                                    Phase = 0,
                                    ID_Mechanism = selectedMechanism.ID
                                };
                                context.ExperimentalPoints.Add(expPointOrg);
                            }
                        }

                        context.SaveChanges();
                        transaction.Commit();
                        MessageBox.Show($"Точка №{currentPointId} успешно сохранена!");
                        currentPointId++;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка: {ex.Message}");
                    }
                }
            }
        }

        //public async Task<List<string?>> GetBaseFormNamesForMechanismAsync(int mechanismId)
        //{
        //    using (var context = new ApplicationContext())
        //    {
        //        return await context.ReactionMechanism
        //            .Where(rm => rm.Mechanism_ID == mechanismId)
        //            .Join(context.Reactions,
        //                rm => rm.Reaction_ID,
        //                r => r.ID,
        //                (rm, r) => new { r.Inp1, r.Inp2, r.Inp3 })
        //            .SelectMany(r => new[] { r.Inp1, r.Inp2, r.Inp3 })
        //            .Join(context.BaseForms,
        //                inp => inp,
        //                bf => bf.Name,
        //                (inp, bf) => bf.Name)
        //            .Distinct()
        //            .ToListAsync();
        //    }
        //}

        //private void BtnAddPoint_Click(object sender, RoutedEventArgs e)
        //{
        //    if (cbMechanisms.SelectedItem is not Mechanisms mechanism)
        //    {
        //        MessageBox.Show("Выберите механизм!");
        //        return;
        //    }

        //    var point = new ExperimentalPoints
        //    {
        //        ID = 1,
        //    };

        //    // Собираем данные из полей ввода
        //    foreach (var container in icFormsInput.Items)
        //    {
        //        if (container is FormInput form)
        //        {
        //            var aqueous = FindAqueousValue(form.FormName);
        //            var organic = FindOrganicValue(form.FormName);

        //            point.Data.Add(new FormData
        //            {
        //                FormName = form.FormName,
        //                AqueousValue = aqueous,
        //                OrganicValue = organic
        //            });
        //        }
        //    }

        //    _points.Add(point);
        //    dgPoints.ItemsSource = _points;
        //}

        //private double? FindAqueousValue(string formName)
        //{
        //    // Поиск значения в водной фазе по тегу
        //    var txtBox = icFormsInput.Items
        //        .OfType<FormInput>()
        //        .Select(f => icFormsInput.ItemContainerGenerator.ContainerFromItem(f))
        //        .Select(container => GetChild<TextBox>(container, "txtAqueous"))
        //        .FirstOrDefault(t => t?.Tag?.ToString() == $"{formName}_Aqueous");

        //    return double.TryParse(txtBox?.Text, out var value) ? value : null;
        //}

        //private T GetChild<T>(DependencyObject parent, string name) where T : FrameworkElement
        //{
        //    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        //    {
        //        var child = VisualTreeHelper.GetChild(parent, i);
        //        if (child is T result && result.Name == name)
        //            return result;

        //        var descendant = GetChild<T>(child, name);
        //        if (descendant != null)
        //            return descendant;
        //    }
        //    return null;
        //}

        //private async void BtnSave_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        using (var context = new ApplicationContext())
        //        {
        //            foreach (var point in _points)
        //            {
        //                var experiment = new Experiment
        //                {
        //                    MechanismId = point.Mechanism.ID,
        //                    Data = JsonConvert.SerializeObject(point.Data)
        //                };

        //                context.Experiments.Add(experiment);
        //            }

        //            await context.SaveChangesAsync();
        //            MessageBox.Show("Данные успешно сохранены!");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка сохранения: {ex.Message}");
        //    }
        //}

        //// Вспомогательные классы
        //public class FormInput
        //{
        //    public string FormName { get; set; }
        //}

        //public class ExperimentalPoint
        //{
        //    public Mechanisms Mechanism { get; set; }
        //    public List<FormData> Data { get; set; }
        //}

        //public class FormData
        //{
        //    public string FormName { get; set; }
        //    public double? AqueousValue { get; set; }
        //    public double? OrganicValue { get; set; }
        //}
    }
 
}
