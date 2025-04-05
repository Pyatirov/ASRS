using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
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
using static System.Collections.Specialized.BitVector32;

namespace CompModeling
{
    /// <summary>
    /// Логика взаимодействия для AddReactionWindow.xaml
    /// </summary>
    public partial class AddReactionWindow : Window
    {
        //public event Action? ReactionAdded;
        public ObservableCollection<Reaction>? allReactions { get; private set; }   
        public AddReactionWindow()
        {
            InitializeComponent();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e) //Динамическое обновление DataGrid
        {
            using (var context = new ApplicationContext())
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                // Добавляем компоненты в BaseForms
                var inp1 = await GetOrCreateBaseForm(context, txtInp1.Text);
                var inp2 = await GetOrCreateBaseForm(context, txtInp2.Text);
                var inp3 = await GetOrCreateBaseForm(context, txtInp3.Text);

                // Добавляем продукт в FormingForms
                var product = await GetOrCreateFormingForm(context, txtProd.Text);

                // Создаем реакцию
                var newReaction = new Reaction
                {
                    Inp1 = inp1.Name,
                    Inp2 = inp2.Name,
                    Inp3 = inp3.Name,
                    Prod = product.Name,
                    KInp1 = int.Parse(txtKInp1.Text),
                    KInp2 = int.Parse(txtKInp2.Text),
                    KInp3 = int.Parse(txtKInp3.Text),
                    KProd = int.Parse(txtKProd.Text),
                };

                context.Reactions.Add(newReaction);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                MessageBox.Show("Реакция успешно сохранена!");
            }
        }

        private async Task<BaseForm> GetOrCreateBaseForm(ApplicationContext context, string name)
        {
            var form = context.BaseForms.FirstOrDefault(f => f.Name == name);
            if (form == null)
            {
                form = new BaseForm { Name = name };
                context.BaseForms.Add(form);
                await context.SaveChangesAsync();
            }
            return form;
        }

        private async Task<FormingForm> GetOrCreateFormingForm(ApplicationContext context, string name)
        {
            var form = context.FormingForms.FirstOrDefault(f => f.Name == name);
            if (form == null)
            {
                form = new FormingForm { Name = name };
                context.FormingForms.Add(form);
                await context.SaveChangesAsync();
            }
            return form;
        }
    }
}
