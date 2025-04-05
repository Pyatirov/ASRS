using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using static CompModeling.ConnectToDB;

namespace CompModeling
{
    /// <summary>
    /// Конвертер для скрытия нуля в реакциях
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для скрытия единицы в реакциях
    /// </summary>
    public class OneToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is int i && i == 1) || (value is double d && d == 1.0) ?
                Visibility.Collapsed :
                Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Логика взаимодействия для Add.xaml
    /// </summary>
    public partial class AddMechanism : Window
    {
        private ObservableCollection<ReactionWrapper>? _reactions;

        public static event Action? MechanismAdded;


        public AddMechanism()
        {
            InitializeComponent();
            LoadDataAsync();
            AddReactionWindow.ReactionAdded += LoadDataAsync;
        }

        private async void LoadDataAsync()
        {
            using (var context = new ApplicationContext())
            {
                var reactions = await context.Reactions.ToListAsync();
                _reactions = new ObservableCollection<ReactionWrapper>(
                    reactions.Select(r => new ReactionWrapper(r)));

                dg_Reactions.ItemsSource = _reactions;
            }
        }

        // Создание нового механизма
        private async void bt_Create_Mechanism_Click(object sender, RoutedEventArgs e)
        {
            var selectedReactions = _reactions!
                .Where(r => r.IsSelected)
                .Select(r => r.Reaction)
                .ToList();

            if (string.IsNullOrWhiteSpace(tb_Mechanism_Name.Text) || selectedReactions.Count == 0)
            {
                MessageBox.Show("Заполните название и выберите реакции!");
                return;
            }

            try
            {
                // Используем отдельный контекст для операции
                using (var context = new ApplicationContext())
                {
                    await using var transaction = await context.Database.BeginTransactionAsync();

                    // Создаем новый механизм
                    var mechanism = new Mechanisms
                    {
                        Info = tb_Mechanism_Name.Text.Trim()
                    };

                    context.Mechanisms.Add(mechanism);
                    await context.SaveChangesAsync(); // Сохраняем механизм первый раз

                    // Создаем связи с реакциями
                    var links = selectedReactions.Select(reaction => new ReactionMechanism
                    {
                        Mechanism_ID = mechanism.ID,
                        Reaction_ID = reaction.ID
                    }).ToList();

                    // Добавляем все связи одним вызовом
                    await context.ReactionMechanism.AddRangeAsync(links);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();


                    // Обновляем UI
                    tb_Mechanism_Name.Clear();
                    MessageBox.Show("Механизм успешно создан!");

                    MechanismAdded?.Invoke();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\n{ex.InnerException?.Message}");
            }
        }

        // Открытие окна добавления реакции
        private void bt_Add_Reaction_Click(object sender, RoutedEventArgs e)
        {
            var АddReaction_Window = new AddReactionWindow();
            АddReaction_Window.ShowDialog();
        }
    }

    public class ReactionWrapper : INotifyPropertyChanged
    {
        public Reaction Reaction { get; }
        public bool IsSelected { get; set; }

        public bool HasReagent1 => !string.IsNullOrEmpty(Reaction.Inp1);
        public bool HasReagent2 => !string.IsNullOrEmpty(Reaction.Inp2);
        public bool HasReagent3 => !string.IsNullOrEmpty(Reaction.Inp3);

        public bool ShowPlus1 => HasReagent1 && (HasReagent2 || HasReagent3);
        public bool ShowPlus2 => HasReagent2 && HasReagent3;

        public string? KInp1Display => Reaction.KInp1 != 1 ? Reaction.KInp1.ToString() : "";

        public string? KInp2Display => Reaction.KInp2 != 1 ? Reaction.KInp2?.ToString() : "";

        public string? KInp3Display => Reaction.KInp3 != 1 ? Reaction.KInp3?.ToString() : "";

        public string? KProdDisplay => Reaction.KProd != 1 ? Reaction.KProd.ToString() : "";

        public ReactionWrapper(Reaction reaction)
        {
            Reaction = reaction;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class ReactionTemplate
    {
        public static HorizontalAlignment GetHorizontalAlignment(DependencyObject obj)
        {
            return (HorizontalAlignment)obj.GetValue(HorizontalAlignmentProperty);
        }

        public static void SetHorizontalAlignment(DependencyObject obj, HorizontalAlignment value)
        {
            obj.SetValue(HorizontalAlignmentProperty, value);
        }

        public static readonly DependencyProperty HorizontalAlignmentProperty =
            DependencyProperty.RegisterAttached("HorizontalAlignment",
                typeof(HorizontalAlignment),
                typeof(ReactionTemplate),
                new UIPropertyMetadata(HorizontalAlignment.Left));
    }
}

