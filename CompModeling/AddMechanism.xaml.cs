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
using Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

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

                dgReactions.ItemsSource = _reactions;
            }
            //foreach (var reaction in _allReactions)
            //{
            //    var tb_KI1 = new TextBlock
            //    {
            //        Text = reaction.KInp1.ToString()
            //    };
            //    var tb_I1 = new TextBlock
            //    {
            //        Text = reaction.Inp1.ToString()
            //    };
            //    var tb_Ind1 = new TextBlock
            //    {
            //        Text = reaction.Ind1.ToString()
            //    };
            //    var tb_KI2 = new TextBlock
            //    {
            //        Text = reaction.KInp2.ToString()
            //    };
            //    var tb_I2 = new TextBlock
            //    {
            //        Text = reaction.Inp2.ToString()
            //    };
            //    var tb_Ind2 = new TextBlock
            //    {
            //        Text = reaction.Ind2.ToString()
            //    };
            //    var tb_KI3 = new TextBlock
            //    {
            //        Text = reaction.KInp3.ToString()
            //    };
            //    var tb_I3 = new TextBlock
            //    {
            //        Text = reaction.Inp3.ToString()
            //    };
            //    var tb_Ind3 = new TextBlock
            //    {
            //        Text = reaction.Ind3.ToString()
            //    };

            //    wp_Reactions.Children.Add(tb_KI1);
            //    wp_Reactions.Children.Add(tb_I1);
            //    wp_Reactions.Children.Add(tb_Ind1);
            //    wp_Reactions.Children.Add(tb_KI2);
            //    wp_Reactions.Children.Add(tb_I2);
            //    wp_Reactions.Children.Add(tb_Ind2);
            //    wp_Reactions.Children.Add(tb_KI3);
            //    wp_Reactions.Children.Add(tb_I3);
            //    wp_Reactions.Children.Add(tb_Ind3);
            //}
        }

        //string Check(Reaction reaction)
        //{
        //    if ( (reaction.Inp2 == null) || 
        //        (reaction.Inp3 == null) || 
        //        (reaction.KInp2 == null) ||
        //        (reaction.KInp3 == null) ||
        //        (reaction.Ind2 == null) ||
        //        (reaction.Ind3 == null))
        //    {
        //        return " ";
        //    }
        //    return reaction.Inp3.ToString();
        //}

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
            //addReactionWindow.ReactionAdded += () => LoadData();
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
}

