using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Navigation;
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

        private Action? MechanismDeleted;


        public ResearcherInterface()
        {
            InitializeComponent();
            LoadDataAsync();
            AddMechanism.MechanismAdded += LoadDataAsync;
            this.MechanismDeleted += LoadDataAsync;
            this.MechanismDeleted += Clear_Expiremntal_Points_Grid;
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
        private void cb_Mechanisms_Experiment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Create_Input_Constants_Grid_Async();
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
        private void bt_Delete_Mechanism_Click(object sender, RoutedEventArgs e)
        {
            Delete_Mechanism_Async(sender);
        }

        /// <summary>
        /// Обработчик выбора комбобокса вкладки "Экспериментальные точки" окна Researcher.xaml
        /// </summary>
        private void cb_Mechanisms_Points_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Create_Expermiental_Points_Grid_Async();
        }

        /// <summary>
        /// Кнопка добавления экспериментальной точки вкладки "Экспериментальные точки" окна Researcher.xaml
        /// </summary>
        private void bt_Add_Experimental_Point_Click(object sender, RoutedEventArgs e)
        {
            Add_Experimental_Points_Async();
        }

        private void bt_Calculate_Click(object sender, RoutedEventArgs e)
        {
            var selectedMechanism = cb_Mechanisms_Experiment.SelectedItem as Mechanisms;
            if (selectedMechanism == null)
            {
                MessageBox.Show("Выберите модель!");
                return;
            }
            Calculation(selectedMechanism);
        }

        private async void Create_Input_Constants_Grid_Async()
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    if (cb_Mechanisms_Experiment.SelectedItem is Mechanisms selectedMechanism)
                    {
                        var reactions = await GetReactionsForMechanismAsync(context, selectedMechanism);

                        ug_Constants_Inputs_Panel.Children.Clear();

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

                            valueBox.PreviewTextInput += TextBox_PreviewTextInputConcentration;
                            valueBox.PreviewKeyDown += TextBox_PreviewKeyDown;

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

                            ug_Constants_Inputs_Panel.Children.Add(grid);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async void Delete_Mechanism_Async(object sender)
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

                    //LoadDataAsync();

                    MechanismDeleted?.Invoke();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}");
            }
        }

        private async void Create_Expermiental_Points_Grid_Async()
        {
            using (var context = new ApplicationContext())
            {
                if (cb_Mechanisms_Points.SelectedItem is Mechanisms selectedMechanism)
                {
                    var mechanismId = selectedMechanism.ID; // Замените на нужный ID механизма

                    var reactions = await GetReactionsForMechanismAsync(context, selectedMechanism);

                    var baseFormNames = await GetBaseFormsFromReactionsAsync(context, reactions);

                    pointInputsPanel.Children.Clear();

                    foreach (var bFs in baseFormNames)
                    {
                        var grid = new Grid();
                        for (int i = 0; i < 5; i++)
                        {
                            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                        }

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
                            Text = "моль/л",
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(5, 0, 0, 0)
                        };

                        valueAquBox.PreviewTextInput += TextBox_PreviewTextInputConcentration;
                        valueAquBox.PreviewKeyDown += TextBox_PreviewKeyDown;
                        valueOrgBox.PreviewTextInput += TextBox_PreviewTextInputConcentration;
                        valueOrgBox.PreviewKeyDown += TextBox_PreviewKeyDown;

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

        private void Clear_Expiremntal_Points_Grid()
        {
            pointInputsPanel.Children.Clear();
        }

        private async void Add_Experimental_Points_Async()
        {
            using (var context = new ApplicationContext())
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var selectedMechanism = cb_Mechanisms_Points.SelectedItem as Mechanisms;
                        if (selectedMechanism == null)
                        {
                            MessageBox.Show("Выберите модель!");
                            return;
                        }

                        // Создаем новую точку
                        var point = new Points();
                        context.Points.Add(point);
                        await context.SaveChangesAsync(); // Получаем ID точки

                        foreach (var entry in inputBoxes)
                        {
                            var baseForm = await context.BaseForms.FirstOrDefaultAsync(bf => bf.Name == entry.Key);
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
                                await context.SaveChangesAsync();

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
                                await context.SaveChangesAsync();

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

                        await context.SaveChangesAsync();
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

        private async Task<List<Reaction>> GetReactionsForMechanismAsync(ApplicationContext context, Mechanisms selectedMechanism)
        {
            var reactions = await context.ReactionMechanism
                .Where(rm => rm.Mechanism_ID == selectedMechanism.ID)
                .Join(context.Reactions,
                    rm => rm.Reaction_ID,
                    r => r.ID,
                    (rm, r) => r)
                .ToListAsync();
            return reactions;
        }

        private async Task<List<BaseForm>> GetBaseFormsFromReactionsAsync(ApplicationContext context, List<Reaction> reactions)
        {
            var formNames = reactions
                .SelectMany(r => new[] { r.Inp1, r.Inp2, r.Inp3 })
                .Where(name => name != null)
                .Distinct()
                .ToList();

            var baseFormNames = await context.BaseForms
                .Where(bf => formNames.Contains(bf.Name))
                .ToListAsync();
            return baseFormNames;
        }

        private async Task<List<FormingForm>> GetFormingFormsFromReactionsAsync(ApplicationContext context, List<Reaction> reactions)
        {
            // Получаем уникальные имена с сохранением исходного порядка
            var prodNames = reactions
                .Select(r => r.Prod)
                .Where(name => name != null)
                .Distinct()
                .ToList();

            // Загружаем данные из БД асинхронно
            var dbItems = await context.FormingForms
                .Where(ff => prodNames.Contains(ff.Name))
                .ToListAsync();

            // Сортируем на клиенте по порядку prodNames
            var orderedItems = prodNames
                .Select(name => dbItems.FirstOrDefault(ff => ff.Name == name))
                .Where(ff => ff != null)
                .ToList();

            return orderedItems!;
        }

        private List<CalculationResult> InitialConcentrationsFromZDM(List<ConcentrationSummary> concentrationsSum, List<ConcentrationConstant> concentrationConstants,
            List<FormingForm> formingForms, List<Reaction> reactions)
        {
            var results = new List<CalculationResult>();
            var concentrationDict = concentrationsSum
                .GroupBy(c => c.PointId)
                .ToDictionary(g => g.Key, g => g.ToDictionary(c => c.FormName, c => c.TotalConcentration));

            var constantsDict = concentrationConstants.ToDictionary(cc => cc.FormName, cc => cc.Value);

            // Добавляем список реакций в параметры функции
            foreach (var point in concentrationsSum.Select(c => c.PointId).Distinct())
            {
                var pointConcentrations = concentrationDict[point];

                foreach (var form in formingForms)
                {
                    if (!constantsDict.TryGetValue(form.Name, out var constant)) continue;

                    // Найти реакцию, которая образует эту форму
                    var reaction = reactions.FirstOrDefault(r => r.Prod == form.Name);
                    if (reaction == null) continue;

                    // Собрать компоненты с коэффициентами
                    var componentsWithCoeffs = new List<(string Component, int Coefficient)>
                        {
                            (form.Component1, reaction.KInp1 ?? 0),
                            (form.Component2, reaction.KInp2 ?? 0),
                            (form.Component3, reaction.KInp3 ?? 0)
                        }
                    .Where(x => !string.IsNullOrEmpty(x.Component));

                    // Вычислить произведение с учетом степеней
                    double product = constant;
                    foreach (var (component, coeff) in componentsWithCoeffs)
                    {
                        if (pointConcentrations.TryGetValue(component, out var conc))
                            product *= Math.Pow(conc, coeff);
                    }

                    results.Add(new CalculationResult
                    {
                        PointId = point,
                        FormName = form.Name,
                        CalculatedValue = product
                    });
                }
            }
            return results;
        }
        
        private async Task<List<ConcentrationConstant>> GetConstantsForMechanismAsync(Mechanisms selectedMechanism, int ConstantsCount)
        {
            using (var context = new ApplicationContext())
            {
                var consts = await context.ConstantsSeries
                    .Where(cs => cs.ID_Mechanism == selectedMechanism.ID)
                    .Join(
                        context.ConcentrationConstants,
                        cs => cs.ID_Const,
                        cc => cc.ID,
                        (cs, cc) => cc
                    )
                    .OrderByDescending(cc => cc.ID) // Сортируем по убыванию ID
                    .Take(ConstantsCount)                       // Берем 10 последних записей
                    .AsNoTracking()                 // Оптимизация производительности
                    .ToListAsync();

                return consts;
            }
        }

        private async Task<int> GetPointsCountPerMechanismAsync(Mechanisms selectedMechanism)
        {
            using (var context = new ApplicationContext())
            {
                var count = await context.ExperimentalPoints
                    .Where(ep => ep.ID_Mechanism == selectedMechanism.ID)
                    .Select(ep => ep.ID_Point)
                    .Distinct()
                    .CountAsync();
                return count;
            }
        }
        private List<List<int>> BuildComponentMatrix(List<Reaction> reactions, List<BaseForm> baseForms)
        {
            List<List<int>> Matrix = new List<List<int>>();

            for (int i = 0; i < reactions.Count; i++)
            {
                Matrix.Add(new List<int>());
                for (int j = 0; j < baseForms.Count; j++)
                {
                    if (reactions[i].Inp1 == baseForms[j].Name)
                    {
                        Matrix[i].Add(reactions[i].KInp1!.Value);
                        continue;
                    }
                    else if (reactions[i].Inp2 == baseForms[j].Name)
                    {
                        Matrix[i].Add(reactions[i].KInp2!.Value);
                        continue;
                    }
                    else if (reactions[i].Inp3 == baseForms[j].Name)
                    {
                        Matrix[i].Add(reactions[i].KInp3!.Value);
                        continue;
                    }
                    else
                    {
                        Matrix[i].Add(0);
                    }
                }
            }
            return Matrix;
        }

        private async void LoadConstantsToDataBaseAsync(List<FormingForm> formingForms,
            List<double> Constants, Mechanisms selectedMechanism)
        {
            for (int i = 0; i < formingForms.Count; i++)
            {
                using (var context = new ApplicationContext())
                {
                    var constant = new ConcentrationConstant
                    {
                        FormName = formingForms[i].Name,
                        Value = Constants[i]
                    };
                    context.ConcentrationConstants.Add(constant);
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

        private async void Calculation(Mechanisms selectedMechanism)
        {
            List<List<int>> ComponentMatrix = new List<List<int>>();
            List<Reaction> reactions = new List<Reaction>();
            List<BaseForm> baseForms = new List<BaseForm>();
            List<FormingForm> formingForms = new List<FormingForm>();

            using (var context = new ApplicationContext())
            {
                reactions = await GetReactionsForMechanismAsync(context, selectedMechanism);
                baseForms = await GetBaseFormsFromReactionsAsync(context, reactions);
                formingForms = await GetFormingFormsFromReactionsAsync(context, reactions);
            }

            ComponentMatrix = BuildComponentMatrix(reactions, baseForms);

            List<double> Constants = new List<double>();

            Constants.Clear();

            foreach (var child in ug_Constants_Inputs_Panel.Children)
            {
                if (child is Grid grid)
                {
                    // Ищем все TextBox'ы внутри Grid, включая вложенные контейнеры
                    var textBoxes = grid.Children.OfType<TextBox>();

                    foreach (var textBox in textBoxes)
                    {
                        if (double.TryParse(textBox.Text, out double value))
                        {
                            Constants.Add(value);
                        }
                    }
                }
            }

            LoadConstantsToDataBaseAsync(formingForms, Constants, selectedMechanism);


            var concentrationsSum = await GetConcentrationSumsPerPoint();

            var concentrationConstants = await GetConstantsForMechanismAsync(selectedMechanism, Constants.Count);

            concentrationConstants.Reverse();

            List<CalculationResult> initalConcentrations = InitialConcentrationsFromZDM(concentrationsSum, concentrationConstants, formingForms, reactions);

            CalculationResults calculationResults = new CalculationResults(baseForms, formingForms, ComponentMatrix);
            calculationResults.Show();

        }

        private void TextBox_PreviewTextInputConcentration(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            char Symb = e.Text[0];

            if (!char.IsDigit(Symb) && Symb != ',')
                e.Handled = true;
        }
        private void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Space)
                e.Handled = true;
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

        public class CalculationResult
        {
            public int PointId { get; set; }
            public string? FormName { get; set; }
            public double CalculatedValue { get; set; }
        }

        private void Reseacher_Closed(object sender, EventArgs e)
        {
            AddMechanism.MechanismAdded -= LoadDataAsync;
            this.MechanismDeleted -= LoadDataAsync;
            this.MechanismDeleted -= Clear_Expiremntal_Points_Grid;
        }
    }
}