using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
//using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static CompModeling.ConnectToDB;

namespace CompModeling
{
    /// <summary>
    /// Логика взаимодействия для Add.xaml
    /// </summary>
    public partial class AddMechanism : Window
    {
        private ObservableCollection<ReactionWrapper>? _reactions;
        private ObservableCollection<Reaction>? _allReactions;
        private ObservableCollection<BaseForm>? _baseforms;
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

                _allReactions = new ObservableCollection<Reaction>(reactions);

                dgReactions.ItemsSource = _reactions;
                var baseforms = await context.BaseForms.ToListAsync();
                _baseforms = new ObservableCollection<BaseForm>(baseforms);
                dgPhasesValues.ItemsSource = _baseforms;
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

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\n{ex.InnerException?.Message}");
            }
        }



        // Открытие окна добавления реакции
        private void BtnAddReaction_Click(object sender, RoutedEventArgs e)
        {
            var addReactionWindow = new AddReactionWindow();
            addReactionWindow.ReactionAdded += () => LoadData();
            addReactionWindow.ShowDialog();
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
}

