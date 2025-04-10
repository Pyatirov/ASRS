using MathNet.Numerics.LinearAlgebra;
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

        //private List<CalculationResult> InitialConcentrationsFromZDM(List<ConcentrationSummary> concentrationsSum, List<ConcentrationConstant> concentrationConstants,
        //    List<FormingForm> formingForms, List<Reaction> reactions)
        //{
        //    var results = new List<CalculationResult>();
        //    var concentrationDict = concentrationsSum
        //        .GroupBy(c => c.PointId)
        //        .ToDictionary(g => g.Key, g => g.ToDictionary(c => c.FormName, c => c.TotalConcentration));

        //    var constantsDict = concentrationConstants.ToDictionary(cc => cc.FormName, cc => cc.Value);

        //    // Добавляем список реакций в параметры функции
        //    foreach (var point in concentrationsSum.Select(c => c.PointId).Distinct())
        //    {
        //        var pointConcentrations = concentrationDict[point];

        //        foreach (var form in formingForms)
        //        {
        //            if (!constantsDict.TryGetValue(form.Name, out var constant)) continue;

        //            // Найти реакцию, которая образует эту форму
        //            var reaction = reactions.FirstOrDefault(r => r.Prod == form.Name);
        //            if (reaction == null) continue;

        //            // Собрать компоненты с коэффициентами
        //            var componentsWithCoeffs = new List<(string Component, int Coefficient)>
        //                {
        //                    (form.Component1, reaction.KInp1 ?? 0),
        //                    (form.Component2, reaction.KInp2 ?? 0),
        //                    (form.Component3, reaction.KInp3 ?? 0)
        //                }
        //            .Where(x => !string.IsNullOrEmpty(x.Component));

        //            // Вычислить произведение с учетом степеней
        //            double product = constant;
        //            foreach (var (component, coeff) in componentsWithCoeffs)
        //            {
        //                if (pointConcentrations.TryGetValue(component, out var conc))
        //                    product *= Math.Pow(conc, coeff);
        //            }

        //            results.Add(new CalculationResult
        //            {
        //                PointId = point,
        //                FormName = form.Name,
        //                CalculatedValue = product
        //            });
        //        }
        //    }
        //    return results;
        //}
        
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

        public static async Task<int> GetPointsCountPerMechanismAsync(Mechanisms selectedMechanism)
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

            List<double> Constants = new List<double> ();

            Constants.Clear();

            Constants.AddRange(new List<double> { 1, 1, 1, 1, 1 });

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

            //LoadConstantsToDataBaseAsync(formingForms, Constants, selectedMechanism);


            var concentrationsSum = await GetConcentrationSumsPerPoint();

            var concentrationConstants = await GetConstantsForMechanismAsync(selectedMechanism, Constants.Count);

            concentrationConstants.Reverse();

            //List<CalculationResult> initalConcentrations = InitialConcentrationsFromZDM(concentrationsSum, concentrationConstants, formingForms, reactions);

//            var builder = new SystemBuilder(
//                                concentrationsSum,
//                                concentrationConstants,
//                                reactions,
//                                baseForms,
//                                formingForms
//                                  );
//            var system = builder.BuildEquations();

            List<List<double>> b = new List<List<double>>();

            List<double> concentrat = new List<double>();
            foreach (var conc in concentrationsSum)
            {
                concentrat.Add(conc.TotalConcentration);
            }

            for (int i = 0; i < concentrat.Count; i += 5)
            {
                var original = concentrat.GetRange(i, 5);
                var reordered = new List<double>
                {
                    original[1],  // Первый элемент нового списка
                    original[3],  // Второй элемент
                    original[0],  // Третий элемент
                    original[4],  // Четвертый элемент
                    original[2]   // Пятый элемент
                };
                b.Add(reordered);
            }

            List<List<double>> XStart = b;

            var pointsCount = await GetPointsCountPerMechanismAsync(selectedMechanism);

            CalculationResults calculationResults = new CalculationResults(baseForms, formingForms, ComponentMatrix, b, XStart, Constants, pointsCount);
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
        

        public class SystemBuilder
    {
        private List<ConcentrationSummary> concentrationsSum;
        private List<ConcentrationConstant> concentrationConstants;
        private List<Reaction> reactions;
        private List<BaseForm> baseForms;
        private List<FormingForm> formingForms;

        public SystemBuilder(
            List<ConcentrationSummary> concentrationsSum,
            List<ConcentrationConstant> concentrationConstants,
            List<Reaction> reactions,
            List<BaseForm> baseForms,
            List<FormingForm> formingForms
        )
        {
            this.concentrationsSum = concentrationsSum;
            this.concentrationConstants = concentrationConstants;
            this.reactions = reactions;
            this.baseForms = baseForms;
            this.formingForms = formingForms;
        }

        //public Dictionary<int, Dictionary<string, string>> BuildEquations()
        //{
        //    var equationSystem = new Dictionary<int, Dictionary<string, string>>();

        //    // Словарь: FormName → Константа K
        //    var constantsDict = concentrationConstants
        //        .ToDictionary(cc => cc.FormName, cc => cc.Value);

        //    // Словарь: FormName → Реакция
        //    var reactionForForm = formingForms
        //        .ToDictionary(
        //            ff => ff.Name,
        //            ff => reactions.FirstOrDefault(r => r.Prod == ff.Name)
        //        );

        //    // Для каждой точки
        //    foreach (var point in concentrationsSum
        //        .Select(c => c.PointId)
        //        .Distinct())
        //    {
        //        var pointConcentrations = concentrationsSum
        //            .Where(c => c.PointId == point)
        //            .ToDictionary(c => c.FormName, c => c.TotalConcentration);

        //        var equationsForPoint = new Dictionary<string, string>();

        //        // Для каждой базовой формы
        //        foreach (var baseForm in baseForms)
        //        {
        //            var formName = baseForm.Name;
        //            var equation = $"[{formName}] = ";

        //            // Суммируем вклады всех реакций, где форма участвует
        //            var terms = new List<string>();
        //            foreach (var reaction in reactions)
        //            {
        //                // Проверяем, участвует ли форма как входной компонент
        //                if (reaction.Inp1 == formName || reaction.Inp2 == formName || reaction.Inp3 == formName)
        //                {
        //                    // Получаем коэффициенты компонентов
        //                    var components = new[]
        //                    {
        //                    (reaction.Inp1, reaction.KInp1),
        //                    (reaction.Inp2, reaction.KInp2),
        //                    (reaction.Inp3, reaction.KInp3)
        //                };

        //                    // Формируем член уравнения
        //                    var term = $"{constantsDict[reaction.Prod]}";
        //                    foreach (var (component, coeff) in components)
        //                    {
        //                        if (string.IsNullOrEmpty(component) || coeff == null || coeff == 0)
        //                            continue;
        //                        term += $" * [{component}]^{coeff.Value}";
        //                    }
        //                    terms.Add($"({term})");
        //                }
        //            }

        //            equation += string.Join(" + ", terms);
        //            equationsForPoint[formName] = equation;
        //        }

        //        equationSystem[point] = equationsForPoint;
        //    }

        //    return equationSystem;
        //}
    }

        public class ConcentrationSummary
        {
            public int PointId { get; set; }
            public string? FormName { get; set; }
            public double TotalConcentration { get; set; }
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