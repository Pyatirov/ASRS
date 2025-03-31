using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using static CompModeling.ConnectToDB;

namespace CompModeling
{
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
    public class ReactionWrapper
    {
        public Reaction Reaction { get; }
        public bool IsSelected { get; set; }
        public string DisplayReaction =>
            $"({Reaction.KInp1}{Reaction.Inp1}){Reaction.Ind1} + ({Reaction.KInp2}{Reaction.Inp2}){Reaction.Ind2} + ({Reaction.KInp3}{Reaction.Inp3}){Reaction.Ind3} → {Reaction.KProd}{Reaction.Prod}";

        public ReactionWrapper(Reaction reaction)
        {
            Reaction = reaction;
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

