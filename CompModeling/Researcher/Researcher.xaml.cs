using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static CompModeling.ConnectToDB;

namespace CompModeling
{
    /// <summary>
    /// Конвертер для скрытия кнопки "удалить" у пустой ячейки DataGrid
    /// </summary>
    public class RowToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            var mechanism = value as Mechanisms; // Замените YourDataType на ваш класс
            if (mechanism == null)
                return Visibility.Collapsed;

            // Проверка на пустую строку (пример для ID = 0)
            if (mechanism.ID == 0 && string.IsNullOrEmpty(mechanism.Info))
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Логика взаимодействия для SpecialistInterface.xaml
    /// </summary>
    public partial class ResearcherInterface : Window
    {
        private static ObservableCollection<Mechanisms>? Mechanisms { get; set; }

        private Dictionary<string, (TextBox AquaBox, TextBox OrgBox)> inputBoxes = new Dictionary<string, (TextBox, TextBox)>();

        private int currentPointId = 1;

        /// <summary>
        /// Загрузка данных при открытии окна Researcher.xaml
        /// </summary>
        private async void LoadDataAsync()
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    var mechanismsNames = await context.Mechanisms.ToListAsync();

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

        /// <summary>
        /// Обработчик выбора комбобокса вкладки "Эксперимент" окна Researcher.xaml
        /// </summary>
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

                        reactionInputsPanel.Children.Clear();

                        //foreach (var reaction in reactions)
                        //{
                        //    var grid = new Grid
                        //    {
                        //        Margin = new Thickness(0, 5, 0, 0),
                        //        VerticalAlignment = VerticalAlignment.Top
                        //    };

                        //    // Настройка колонок
                        //    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                        //    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                        //    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                        //    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                        //    UIElement[] elements =
                        //    [
                        //        new TextBlock { Text = "lg K", FontSize=16, Margin=new Thickness(0,0,10,0) },
                        //        new TextBlock { Text = reaction.Prod, FontStyle=FontStyles.Italic, MinWidth=120 },
                        //        new TextBox { Width=80, Margin=new Thickness(0,0,10,0) },
                        //        new TextBlock { Text = "моль/cм³" }
                        //    ];

                        //    for (int i = 0; i < elements.Length; i++)
                        //    {
                        //        Grid.SetColumn(elements[i], i);
                        //        grid.Children.Add(elements[i]);
                        //    }

                        //    reactionInputsPanel.Children.Add(grid);
                        //}
                        foreach (var reaction in reactions)
                        {
                            var grid = new Grid
                            {
                                Margin = new Thickness(0, 5, 0, 0),
                                VerticalAlignment = VerticalAlignment.Top
                            };

                            // Настройка строк и колонок
                            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                            // Элементы
                            var lgKBlock = new TextBlock
                            {
                                Text = "lg K",
                                FontSize = 16,
                                VerticalAlignment = VerticalAlignment.Bottom,
                                Margin = new Thickness(10, 0, 0, 0)
                            };

                            var prodBlock = new TextBlock
                            {
                                Text = reaction.Prod,
                                FontStyle = FontStyles.Italic,
                                Margin = new Thickness(30, 0, 0, 0),
                                HorizontalAlignment = HorizontalAlignment.Left
                            };

                            var valueBox = new TextBox
                            {
                                Width = 80,
                                Margin = new Thickness(30, 5, 10, 0)
                            };

                            var unitBlock = new TextBlock
                            {
                                Text = "моль/cм³",
                                VerticalAlignment = VerticalAlignment.Center
                            };

                            // Размещение элементов
                            Grid.SetRow(lgKBlock, 0);
                            Grid.SetColumn(lgKBlock, 0);

                            Grid.SetRow(prodBlock, 1);
                            Grid.SetColumn(prodBlock, 0);
                            Grid.SetColumnSpan(prodBlock, 2);

                            Grid.SetRow(valueBox, 0);
                            Grid.SetColumn(valueBox, 1);

                            Grid.SetRow(unitBlock, 0);
                            Grid.SetColumn(unitBlock, 2);

                            grid.Children.Add(lgKBlock);
                            grid.Children.Add(prodBlock);
                            grid.Children.Add(valueBox);
                            grid.Children.Add(unitBlock);

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

        /// <summary>
        /// Кнопка добавления нового механизма вкладки "Механизмы" окна Researcher.xaml
        /// </summary>
        private void add_Mechanism_Click(object sender, RoutedEventArgs e)
        {
            var addMechanismWindow = new AddMechanism();

            // Проверяем DialogResult именно окна добавления
            if (addMechanismWindow.ShowDialog() == true)
            {
                LoadDataAsync();
            }
        }
        /// <summary>
        /// Кнопка удаления механизма вкладки "Механизмы" окна Researcher.xaml
        /// </summary>
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

                    LoadDataAsync();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}");
            }
        }

        /// <summary>
        /// Обработчик выбора комбобокса вкладки "Экспериментальные точки" окна Researcher.xaml
        /// </summary>
        private void cbMechanisms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (var context = new ApplicationContext())
            {
                if (cbMechanisms.SelectedItem is Mechanisms selectedMechanism)
                {
                    var baseFormNames = context.BaseForms;
                    pointInputsPanel.Children.Clear();

                    foreach (var bFs in baseFormNames)
                    {
                        var grid = new Grid();
                        // Изменено количество колонок и их размеры
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Название формы
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Водная фаза
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Единица измерения
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Органическая фаза
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Единица измерения

                        grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                        grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                        // Название базовой формы
                        var bfName = new TextBlock
                        {
                            Text = bFs.Name,
                            FontSize = 20,
                            FontStyle = FontStyles.Italic,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(0, 0, 15, 0),
                            Width = 40
                        };
                        Grid.SetColumn(bfName, 0);
                        Grid.SetRowSpan(bfName, 2);

                        // Подписи фаз
                        var labelAq = new TextBlock { Text = "Водная фаза" };
                        var labelOrg = new TextBlock { Text = "Органическая фаза", Margin = new Thickness(10, 0, 0, 0) };

                        // Поля ввода
                        var valueAquBox = new TextBox { Width = 110, Margin = new Thickness(0, 5, 0, 10) };
                        var valueOrgBox = new TextBox { Width = 110, Margin = new Thickness(10, 5, 0, 10) };

                        // Новая единица измерения между полями
                        var middleUnit = new TextBlock
                        {
                            Text = "моль/cм³",
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(5, 0, 0, 0)
                        };

                        // Общая единица измерения
                        var rightUnit = new TextBlock
                        {
                            Text = "моль/cм³",
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(5, 0, 0, 0)
                        };

                        // Размещение элементов
                        grid.Children.Add(bfName);

                        // Водная фаза
                        Grid.SetColumn(labelAq, 1);
                        Grid.SetRow(labelAq, 0);
                        grid.Children.Add(labelAq);

                        Grid.SetColumn(valueAquBox, 1);
                        Grid.SetRow(valueAquBox, 1);
                        grid.Children.Add(valueAquBox);

                        // Средняя единица измерения
                        Grid.SetColumn(middleUnit, 2);
                        Grid.SetRow(middleUnit, 1);
                        grid.Children.Add(middleUnit);

                        // Органическая фаза
                        Grid.SetColumn(labelOrg, 3);
                        Grid.SetRow(labelOrg, 0);
                        grid.Children.Add(labelOrg);

                        Grid.SetColumn(valueOrgBox, 3);
                        Grid.SetRow(valueOrgBox, 1);
                        grid.Children.Add(valueOrgBox);

                        // Правая единица измерения
                        Grid.SetColumn(rightUnit, 4);
                        Grid.SetRow(rightUnit, 1);
                        grid.Children.Add(rightUnit);

                        inputBoxes[bFs.Name!] = (valueAquBox, valueOrgBox);
                        pointInputsPanel.Children.Add(grid);
                    }
                }
            }
        }

        /// <summary>
        /// Кнопка добавления экспериментальной точки вкладки "Экспериментальные точки" окна Researcher.xaml
        /// </summary>
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

        private void Button_calculate_Click(object sender, RoutedEventArgs e)
        {

        }
    }
 
}
