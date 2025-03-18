using System;
using System.Collections.Generic;
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
        public event Action? ReactionAdded;
        public AddReactionWindow()
        {
            InitializeComponent();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var newReaction = new Reaction
            {
                ID = 11,
                Inp1 = txtInp1.Text,
                Inp2 = txtInp2.Text,
                Inp3 = txtInp3.Text,
                Prod = txtProd.Text,
                KInp1 = int.Parse(txtKInp1.Text),
                KInp2 = int.Parse(txtKInp2.Text),
                KInp3 = int.Parse(txtKInp3.Text),
                KProd = int.Parse(txtKProd.Text),
                Ind1 = int.Parse(txtInd1.Text),
                Ind2 = int.Parse(txtInd2.Text),
                Ind3 = int.Parse(txtInd3.Text)
            };

            using (var context = new ApplicationContext())
            {
                context.Reactions.Add(newReaction);
                await context.SaveChangesAsync();
                var exists = await context.Reactions.AnyAsync(r => r.ID == newReaction.ID);
                if (!exists)
                {
                    MessageBox.Show($"Реакция с ID {newReaction.ID} не найдена!");
                    return;
                }
            }

            ReactionAdded?.Invoke();
            Close();
        }

    }
}
