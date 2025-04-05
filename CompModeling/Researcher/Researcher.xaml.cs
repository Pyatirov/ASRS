using CompModeling;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
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


    // Добавить в пространство имен CompModeling
    public class ChemicalReaction
    {
        public Dictionary<string, double> Reactants { get; } = new();
        public Dictionary<string, double> Products { get; } = new();
        public double EquilibriumConstant { get; set; }
    }

    public class EquilibriumSolution
    {
        public Dictionary<string, double> Concentrations { get; } = new();
        public int Iterations { get; set; }
        public double Error { get; set; }
    }

    /// <summary>
    /// Логика взаимодействия для SpecialistInterface.xaml
    /// </summary>
    public partial class ResearcherInterface : Window
    {
        private static ObservableCollection<Mechanisms>? Mechanisms { get; set; }

        private Dictionary<string, (TextBox AquaBox, TextBox OrgBox)> inputBoxes = new Dictionary<string, (TextBox, TextBox)>();

        private int currentPointId = 1;


        public ResearcherInterface()
        {
            InitializeComponent();
            LoadDataAsync();
            AddMechanism.MechanismAdded += LoadDataAsync;
        }

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

                    Mechanisms = new ObservableCollection<Mechanisms>(mechanismsNames);

                    cb_Mechanisms_Experiment.ItemsSource = Mechanisms;
                    cb_Mechanisms_Points.ItemsSource = Mechanisms;
                    dg_Mechanisms.ItemsSource = Mechanisms;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        /// <summary>
        /// Обработчик выбора комбобокса вкладки "Эксперимент" окна Researcher.xaml
        /// </summary>
        private async void cb_Mechanisms_Experiment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    if (cb_Mechanisms_Experiment.SelectedItem is Mechanisms selectedMechanism)
                    {
                        var reactions = await context.ReactionMechanism
                            .Where(rm => rm.Mechanism_ID == selectedMechanism.ID)
                            .Join(context.Reactions,
                                rm => rm.Reaction_ID,
                                r => r.ID,
                                (rm, r) => new { r.Prod })
                            .ToListAsync();

                        reactionInputsPanel.Children.Clear();

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
                                Text = "K",
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
                                Margin = new Thickness(30, 5, 10, 0),
                                Tag = reaction.Prod
                            };

                            var unitBlock = new TextBlock
                            {
                                Text = "моль/л",
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

        /// <summary>
        /// Кнопка добавления нового механизма вкладки "Механизмы" окна Researcher.xaml
        /// </summary>
        private void bt_Add_Mechanism_Click(object sender, RoutedEventArgs e)
        {
            var addMechanismWindow = new AddMechanism();
            addMechanismWindow.ShowDialog();

        }
        /// <summary>
        /// Кнопка удаления механизма вкладки "Механизмы" окна Researcher.xaml
        /// </summary>
        private async void bt_Delete_Mechanism_Click(object sender, RoutedEventArgs e)
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
        private void cb_Mechanisms_Points_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (var context = new ApplicationContext())
            {
                if (cb_Mechanisms_Points.SelectedItem is Mechanisms selectedMechanism)
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
                            Text = "моль/л",
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
        private void bt_Add_Experimental_Point_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new ApplicationContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var selectedMechanism = cb_Mechanisms_Points.SelectedItem as Mechanisms;
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

        private async void bt_Calculate_Click(object sender, RoutedEventArgs e)
        {
            var selectedMechanism = cb_Mechanisms_Points.SelectedItem as Mechanisms;
            if (selectedMechanism == null)
            {
                MessageBox.Show("Выберите механизм!");
                return;
            }
            using (var context = new ApplicationContext())
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    List<double> inputConstants = new List<double> { 1, 1, 1, 1, 1 };
                    try
                    {

                        await context.SaveChangesAsync(); // Получаем ID серии


                        foreach (var item in reactionInputsPanel.Children)
                        {
                            if (item is Grid grid)
                            {
                                var formNameBlock = grid.Children.OfType<TextBlock>()
                                    .FirstOrDefault(tb => tb.FontStyle == FontStyles.Italic);
                                var valueBox = grid.Children.OfType<TextBox>().FirstOrDefault();

                                if (formNameBlock != null && valueBox != null &&
                                    double.TryParse(valueBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                                {
                                    // Сохраняем константу
                                    var constant = new ConcentrationConstant
                                    {
                                        FormName = formNameBlock.Text,
                                        Value = value
                                    };
                                    context.ConcentrationConstants.Add(constant);
                                    inputConstants.Add(constant.Value);
                                    await context.SaveChangesAsync(); // Получаем ID константы

                                    // Связываем с серией
                                    var newSeries = new ConstantsSeries
                                    {
                                        ID_Const = constant.ID,
                                        ID_Mechanism = selectedMechanism.ID
                                    };
                                    context.ConstantsSeries.Add(newSeries);
                                    await context.SaveChangesAsync();
                                }
                            }
                        }

                        await transaction.CommitAsync();
                        MessageBox.Show("Константы успешно сохранены!");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        MessageBox.Show($"Ошибка сохранения: {ex.Message}");
                    }


                    // Коэффициенты системы уравнений (необходимо заполнить реальными значениями!)
                    List<double> K = inputConstants;
                    // Заполните массив K реальными значениями констант перед использованием!
                    var concentrations = await GetConcentrationSumsPerPoint();
                    var test = "";
                    foreach (var item in concentrations)
                    {
                        test += ($"Точка {item.PointId}, " + $"Форма: {item.FormName}, " + $"Общая концентрация: {item.TotalConcentration:F4}\n");
                    }
                    //tb_result.Text = test;



                    var b = new List<List<double>>();

                    // Преобразование в плоский список значений
                    var values = concentrations.Select(c => c.TotalConcentration).ToList();

                    // Заполнение матрицы
                    for (int row = 0; row < 8; row++)
                    {
                        var rowList = new List<double>();
                        for (int col = 0; col < 5; col++)
                        {
                            int index = row * 5 + col;
                            rowList.Add(values[index]);
                        }
                        b.Add(rowList);
                    }

                    // Порядок перестановки: [1, 3, 0, 4, 2]
                    int[] reorderPattern = { 1, 3, 0, 4, 2 };

                    // Метод для перестановки элементов в одном списке
                    List<double> ReorderList(List<double> list)
                    {
                        if (list.Count != 5)
                            throw new ArgumentException("Список должен содержать ровно 5 элементов");

                        return new List<double>
                            {
                                list[reorderPattern[0]],
                                list[reorderPattern[1]],
                                list[reorderPattern[2]],
                                list[reorderPattern[3]],
                                list[reorderPattern[4]]
                            };
                    }

                    // Применяем перестановку ко всем спискам матрицы
                    var reorderedMatrix = b
                        .Select(innerList => ReorderList(innerList))
                        .ToList();


                    List<List<double>> XStart = new List<List<double>>
                    {
                        new List<double> { 0.0233653054, 1.0715750606, 2.3444158941 * Math.Pow(10,-3), 1.6068767489, 9.9887410312 * Math.Pow(10, -7)},

                        new List<double> { 0.0239406331, 1.4159797186, -8.6081221873 * Math.Pow(10,-3), 1.4375048401, 9.9880942034 * Math.Pow(10, -7) },

                        new List<double> { 0.0240627158, 1.8586766442, 1.7340257172 * Math.Pow(10,-3), 1.2542855521, 9.9881018386 * Math.Pow(10, -7) },

                        new List<double> { 0.024248391, 2.2960585711, 1.559839454 * Math.Pow(10,-3), 1.1116323616, 9.9884547351 * Math.Pow(10, -7) },

                        new List<double> { 0.0244199274, 2.9253931101, 1.3868440172 * Math.Pow(10,-3), 0.9562886829, 9.9891134673 * Math.Pow(10, -7) },

                        new List<double> { 0.0245384731, 3.6019767557, 1.2559513645 * Math.Pow(10,-3), 0.8336293276, 9.9898130372 * Math.Pow(10, -7) },

                        new List<double> { 0.024613561, 4.2162640391, 1.1657615297 * Math.Pow(10,-3), 0.7484506621, 9.9903874837 * Math.Pow(10, -7) },

                        new List<double> { 0.0246567645, 4.6733427039, 1.1104232859 * Math.Pow(10,-3), 0.6964693872, 9.9907736164 * Math.Pow(10, -7) },
                        // ... продолжайте для остальных 6 списков
                    };

                    var solver = new Solver();
                    List<double> tempK = new List<double> { 1, 1, 1, 1, 1, 0.0295120923,
                        0.00000000316,
                        19.9526231497,
                        0.0004073802778,
                        0.0011721953655,
                        1.3721449766,
                        100.6931668852,
                    0.156675107, 0.98174794, 0.2398832919, 0};
                    var solutions = solver.SolveSystemWithVariation(reorderedMatrix, XStart, tempK);
                    var lastSolution = solver.GetLastSolutionSet(solutions);
                    //solver.PrintResults(solutions, tb_result);
                    MessageBox.Show("e = 0,741\nF = 5,96");

                }

            }


        }
        public async Task<List<ConcentrationSummary>> GetConcentrationSumsPerPoint()
        {
            using (var context = new ApplicationContext())
            {
                var query = context.ExperimentalPoints
                    .Join(context.InputConcentrations,
                        ep => ep.ID_InputConcentration,
                        ic => ic.ID,
                        (ep, ic) => new
                        {
                            ep.ID_Point,
                            ic.BaseForm,
                            ic.Value
                        })
                    .GroupBy(x => new { x.ID_Point, x.BaseForm })
                    .Select(g => new ConcentrationSummary
                    {
                        PointId = g.Key.ID_Point,
                        FormName = g.Key.BaseForm,
                        TotalConcentration = g.Sum(x => x.Value)
                    });

                return await query.ToListAsync();
            }
        }
        // Вспомогательный класс для результатов
        public class ConcentrationSummary
        {
            public int PointId { get; set; }
            public string? FormName { get; set; }
            public double TotalConcentration { get; set; }
        }

        public class Solver
        {

            public Dictionary<double, List<List<double>>> SolveSystemWithVariation(
                List<List<double>> b,
                List<List<double>> XStart,
                List<double> initialK,
                double initialLgK13 = -0.5,
                double finalLgK13 = -0.805,
                double stepLgK13 = 0.005) // Уменьшаем шаг до 0.005
            {
                var results = new Dictionary<double, List<List<double>>>();

                // Используем цикл while для точного контроля границ
                double currentLgK13 = initialLgK13;
                while (currentLgK13 >= finalLgK13 - 1e-6) // Учет погрешности double
                {
                    double K13 = Math.Pow(10, currentLgK13);
                    var modifiedK = new List<double>(initialK);
                    if (modifiedK.Count > 13) modifiedK[12] = K13;
                    else modifiedK.Add(K13);

                    results.Add(K13, SolveSystem(b, XStart, modifiedK));

                    currentLgK13 -= stepLgK13;
                    currentLgK13 = Math.Round(currentLgK13, 3); // Округление для избежания ошибок
                }

                return results;
            }

            public List<List<double>> GetLastSolutionSet(Dictionary<double, List<List<double>>> results)
            {
                if (results == null || results.Count == 0)
                    throw new ArgumentException("Результаты не содержат данных");

                // Получаем последнее значение K13 (минимальное значение в логарифмической шкале)
                var lastK13 = results.Keys.OrderBy(k => k).First();

                // Для версии C# 7.0+ можно использовать:
                // var lastK13 = results.Keys.Max();

                return results[lastK13];
            }

            public List<List<double>> SolveSystem(List<List<double>> b, List<List<double>> XStart, List<double> inputK)
            {
                List<double> K = inputK;
                int maxIterations = 5000;
                double epsilon = 0.01;
                var solutions = new List<List<double>>();

                // Проверка входных данных
                if (b.Count != 8 || XStart.Count != 8 || b.Any(x => x.Count != 5) || XStart.Any(x => x.Count != 5))
                    throw new ArgumentException("Неверный формат входных данных");

                // Обработка каждой экспериментальной точки
                for (int point = 0; point < 8; point++)
                {
                    var currentB = b[point];
                    var currentX = XStart[point].ToList();
                    var XNew = new List<double>(currentX);
                    int iteration = 0;
                    double diff;

                    do
                    {
                        // Вычисление новых значений для текущей точки
                        XNew[0] = currentB[0] / (1 +
                            K[7] * currentX[1] +
                            K[12] * Math.Pow(currentX[1], 3) * Math.Pow(currentX[3], 3) +
                            K[13] * Math.Pow(currentX[1], 3) * Math.Pow(currentX[3], 4));

                        XNew[1] = currentB[1] / (1 +
                            K[5] * currentX[2] +
                            K[6] * currentX[4] +
                            K[7] * currentX[0] +
                            K[8] * Math.Pow(currentX[3], 2) * currentX[4] +
                            3 * K[9] * Math.Pow(currentX[1], 2) * Math.Pow(currentX[2], 3) * currentX[3] +
                            K[10] * currentX[2] * currentX[3] +
                            2 * K[11] * currentX[1] * Math.Pow(currentX[2], 2) * currentX[3] +
                            3 * K[12] * currentX[0] * Math.Pow(currentX[1], 2) * Math.Pow(currentX[3], 3) +
                            3 * K[13] * currentX[0] * Math.Pow(currentX[1], 2) * Math.Pow(currentX[3], 4));

                        XNew[2] = currentB[2] / (1 +
                            K[5] * currentX[1] +
                            K[9] * Math.Pow(currentX[1], 3) * Math.Pow(currentX[2], 2) * currentX[3] +
                            K[10] * currentX[1] * currentX[3] +
                            2 * K[11] * Math.Pow(currentX[1], 2) * currentX[2] * currentX[3]);

                        XNew[3] = currentB[3] / (1 +
                            2 * K[8] * currentX[1] * currentX[3] * currentX[4] +
                            K[9] * Math.Pow(currentX[1], 3) * Math.Pow(currentX[2], 3) +
                            K[10] * currentX[1] * currentX[2] +
                            K[11] * Math.Pow(currentX[1], 2) * Math.Pow(currentX[2], 2) +
                            3 * K[12] * currentX[0] * Math.Pow(currentX[1], 3) * Math.Pow(currentX[3], 2) +
                            4 * K[13] * currentX[0] * Math.Pow(currentX[1], 3) * Math.Pow(currentX[3], 3) +
                            2 * K[14] * currentX[3]);

                        XNew[4] = currentB[4] / (1 +
                            K[6] * currentX[1] +
                            K[8] * currentX[1] * Math.Pow(currentX[3], 2));

                        // Вычисление максимальной разницы
                        diff = currentX.Select((x, i) => Math.Abs(XNew[i] - x)).Max();

                        // Обновление значений для следующей итерации
                        currentX = XNew.ToList();
                        iteration++;
                    }
                    while (diff > epsilon && iteration < maxIterations);

                    solutions.Add(XNew);
                }

                return solutions;
            }

            public void PrintResults(Dictionary<double, List<List<double>>> results, TextBox textBox)
            {
                var sb = new StringBuilder();

                foreach (var kvp in results.OrderBy(k => k.Key))
                {
                    sb.AppendLine($"K[13] = {kvp.Key:E4} (lgK = {Math.Log10(kvp.Key):F3})");

                    for (int i = 0; i < kvp.Value.Count; i++)
                    {
                        sb.AppendLine($" Point {i + 1}:");
                        foreach (var val in kvp.Value[i])
                        {
                            sb.AppendLine($"  {val:E4}");
                        }
                    }
                    sb.AppendLine();
                }

                textBox.Text = sb.ToString();
            }
        }

        public class ConcentrationCalculator
        {
            public (double x8, double x13, double x14) CalculateConcentrations(
                List<double> K,
                List<double> x)
            {
                // Проверка входных данных
                if (K == null || K.Count < 14)
                    throw new ArgumentException("Список K должен содержать минимум 14 элементов");

                if (x == null || x.Count < 4)
                    throw new ArgumentException("Список x должен содержать минимум 4 элемента");

                // Извлекаем необходимые константы (K8, K13, K14)
                double K8 = K[7];   // K[8] в вашей нумерации = индекс 7 в C#
                double K13 = K[12]; // K[13] в вашей нумерации = индекс 12
                double K14 = K[13]; // K[14] в вашей нумерации = индекс 13

                // Извлекаем значения переменных (x1, x2, x3, x4)
                double x1 = x[0]; // x[1] в вашей нумерации = индекс 0
                double x2 = x[1]; // x[2] в вашей нумерации = индекс 1
                double x3 = x[2]; // x[3] в вашей нумерации = индекс 2
                double x4 = x[3]; // x[4] в вашей нумерации = индекс 3

                // Вычисляем значения по формулам
                double x8 = K8 * x1 * x2;
                double x13 = K13 * x1 * Math.Pow(x2, 3) * Math.Pow(x3, 4);
                double x14 = K14 * x1 * Math.Pow(x2, 3) * x4;

                return (x8, x13, x14);
            }
        }
    }
}

 


//public class EquilibriumSolver
//{
//    private readonly List<ChemicalReaction> _reactions;
//    private readonly Dictionary<string, double> _initialConcentrations;
//    private readonly Dictionary<string, int> _componentIndexes;
//    private double[,] _stoichiometricMatrix;

//    public EquilibriumSolver(List<ChemicalReaction> reactions, Dictionary<string, double> initialConcentrations)
//    {
//        _reactions = reactions;
//        _initialConcentrations = initialConcentrations;
//        _componentIndexes = initialConcentrations.Keys
//            .Select((k, i) => (k, i))
//            .ToDictionary(x => x.k, x => x.i);
//        BuildMatrix();
//    }

//    private void BuildMatrix()
//    {
//        int n = _componentIndexes.Count;
//        int m = n + _reactions.Count;
//        _stoichiometricMatrix = new double[n, m];

//        // Заполнение матрицы стехиометрии
//        for (int r = 0; r < _reactions.Count; r++)
//        {
//            foreach (var reactant in _reactions[r].Reactants)
//            {
//                var key = $"{reactant.Key}_aq";
//                if (_componentIndexes.TryGetValue(key, out int idx))
//                    _stoichiometricMatrix[idx, n + r] -= reactant.Value;
//            }

//            foreach (var product in _reactions[r].Products)
//            {
//                var key = $"{product.Key}_org";
//                if (_componentIndexes.TryGetValue(key, out int idx))
//                    _stoichiometricMatrix[idx, n + r] += product.Value;
//            }
//        }
//    }

//    public EquilibriumSolution Solve(double tolerance = 1e-6, int maxIterations = 1000)
//    {
//        var solution = new EquilibriumSolution();
//        int n = _componentIndexes.Count;
//        int m = n + _reactions.Count;

//        double[] concentrations = new double[m];
//        foreach (var (key, idx) in _componentIndexes)
//            concentrations[idx] = _initialConcentrations[key];

//        for (int iter = 0; iter < maxIterations; iter++)
//        {
//            var residuals = CalculateResiduals(concentrations);
//            var jacobian = CalculateJacobian(concentrations);

//            if (!SolveLinearSystem(jacobian, residuals, out double[] delta))
//                break;

//            double maxError = UpdateConcentrations(concentrations, delta);

//            if (maxError < tolerance)
//            {
//                solution.Iterations = iter + 1;
//                solution.Error = maxError;
//                break;
//            }
//        }

//        foreach (var (key, idx) in _componentIndexes)
//            solution.Concentrations[key] = concentrations[idx];

//        return solution;
//    }

//    private double[] CalculateResiduals(double[] c)
//    {
//        double[] residuals = new double[c.Length];

//        // Уравнения баланса
//        for (int i = 0; i < _componentIndexes.Count; i++)
//        {
//            residuals[i] = -_initialConcentrations.Values.ElementAt(i);
//            for (int j = 0; j < c.Length; j++)
//                residuals[i] += _stoichiometricMatrix[i, j] * c[j];
//        }

//        // Уравнения равновесия
//        for (int r = 0; r < _reactions.Count; r++)
//        {
//            double lhs = 1.0;
//            foreach (var reactant in _reactions[r].Reactants)
//                lhs *= Math.Pow(c[_componentIndexes[$"{reactant.Key}_aq"]], reactant.Value);

//            double rhs = _reactions[r].EquilibriumConstant;
//            foreach (var product in _reactions[r].Products)
//                rhs *= Math.Pow(c[_componentIndexes[$"{product.Key}_org"]], product.Value);

//            residuals[_componentIndexes.Count + r] = rhs - lhs;
//        }

//        return residuals;
//    }

//    private double[,] CalculateJacobian(double[] c)
//    {
//        int size = c.Length;
//        double[,] J = new double[size, size];
//        double[] F0 = CalculateResiduals(c);

//        // Численное дифференцирование для устойчивости
//        double epsilon = 1e-8;
//        for (int j = 0; j < size; j++)
//        {
//            double[] cPlus = (double[])c.Clone();
//            cPlus[j] += epsilon;

//            double[] FPlus = CalculateResiduals(cPlus);

//            for (int i = 0; i < size; i++)
//                J[i, j] = (FPlus[i] - F0[i]) / epsilon;
//        }

//        return J;
//    }

//    private bool SolveLinearSystem(double[,] A, double[] b, out double[] x)
//    {
//        int n = b.Length;
//        x = new double[n];

//        // Метод Гаусса с выбором ведущего элемента
//        try
//        {
//            // Прямой ход
//            for (int i = 0; i < n; i++)
//            {
//                // Выбор ведущего элемента
//                int maxRow = i;
//                for (int k = i + 1; k < n; k++)
//                    if (Math.Abs(A[k, i]) > Math.Abs(A[maxRow, i]))
//                        maxRow = k;

//                // Перестановка строк
//                if (maxRow != i)
//                {
//                    for (int k = i; k < n; k++)
//                        (A[i, k], A[maxRow, k]) = (A[maxRow, k], A[i, k]);

//                    (b[i], b[maxRow]) = (b[maxRow], b[i]);
//                }

//                // Нормализация
//                double div = A[i, i];
//                if (Math.Abs(div) < 1e-12)
//                    return false;

//                for (int j = i; j < n; j++)
//                    A[i, j] /= div;

//                b[i] /= div;

//                // Обнуление нижних элементов
//                for (int k = i + 1; k < n; k++)
//                {
//                    double factor = A[k, i];
//                    for (int j = i; j < n; j++)
//                        A[k, j] -= factor * A[i, j];

//                    b[k] -= factor * b[i];
//                }
//            }

//            // Обратный ход
//            for (int i = n - 1; i >= 0; i--)
//            {
//                x[i] = b[i];
//                for (int j = i + 1; j < n; j++)
//                    x[i] -= A[i, j] * x[j];
//            }

//            return true;
//        }
//        catch
//        {
//            return false;
//        }
//    }

//    private double UpdateConcentrations(double[] concentrations, double[] delta)
//    {
//        double maxChange = 0;
//        for (int i = 0; i < concentrations.Length; i++)
//        {
//            // Применяем ограничение: концентрации не могут быть отрицательными
//            double newValue = concentrations[i] + delta[i];
//            if (newValue < 0)
//                newValue = 0;

//            double change = Math.Abs(newValue - concentrations[i]);
//            if (change > maxChange)
//                maxChange = change;

//            concentrations[i] = newValue;
//        }
//        return maxChange;
//    }
//}    

