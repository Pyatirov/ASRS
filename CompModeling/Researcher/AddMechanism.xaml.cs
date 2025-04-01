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
        public AddMechanism()
        {
            InitializeComponent();
            LoadData();
        }

        private async void LoadData()
        {
            using (var context = new ApplicationContext())
            {
                var reactions = await context.Reactions.ToListAsync();
                _reactions = new ObservableCollection<ReactionWrapper>(
                    reactions.Select(r => new ReactionWrapper(r)));

                dgReactions.ItemsSource = _reactions;
            }
        }

        // Создание нового механизма
        private async void BtnCreateMechanism_Click(object sender, RoutedEventArgs e)
        {
            var selectedReactions = _reactions!
                .Where(r => r.IsSelected)
                .Select(r => r.Reaction)
                .ToList();

            if (string.IsNullOrWhiteSpace(txtMechanismName.Text) || !selectedReactions.Any())
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
                        Info = txtMechanismName.Text.Trim()
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
                    txtMechanismName.Clear();
                    MessageBox.Show("Механизм успешно создан!");
                    this.DialogResult = true;
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\n{ex.InnerException?.Message}");
                this.DialogResult= false;
            }
        }

        // Открытие окна добавления реакции
        private void BtnAddReaction_Click(object sender, RoutedEventArgs e)
        {
            var addReactionWindow = new AddReactionWindow();
            addReactionWindow.ShowDialog();
        }

        private void BtnCreatePoints_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    // Класс для отображения реакций с флажком
    //public class ReactionWrapper
    //{
    //    public Reaction Reaction { get; }
    //    public bool IsSelected { get; set; }
    //    public string DisplayReaction =>
    //        $"({Reaction.KInp1}{Reaction.Inp1}){Reaction.Ind1} + ({Reaction.KInp2}{Reaction.Inp2}){Reaction.Ind2} + ({Reaction.KInp3}{Reaction.Inp3}){Reaction.Ind3} → {Reaction.KProd}{Reaction.Prod}";

    //    public ReactionWrapper(Reaction reaction)
    //    {
    //        Reaction = reaction;
    //    }
    //}

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
        public string? Ind1Display => Reaction.Ind1 != 1 ? Reaction.Ind1.ToString() : "";

        public string? KInp2Display => Reaction.KInp2 != 1 ? Reaction.KInp2?.ToString() : "";
        public string? Ind2Display => Reaction.Ind2 != 1 ? Reaction.Ind2?.ToString() : "";

        public string? KInp3Display => Reaction.KInp3 != 1 ? Reaction.KInp3?.ToString() : "";
        public string? Ind3Display => Reaction.Ind3 != 1 ? Reaction.Ind3?.ToString() : "";

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

