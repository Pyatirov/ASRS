using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Windows;
using static CompModeling.ConnectToDB;

namespace CompModeling
{
    /// <summary>
    /// Логика взаимодействия для SpecialistInterface.xaml
    /// </summary>
    public partial class SpecialistInterface : Window
    {
        ApplicationContext db = new ApplicationContext();
        public ObservableCollection<InputConcentrations> Concentrations { get; set; } = new();


        public SpecialistInterface()
        {
            //db.Database.EnsureCreated();
            //InitializeComponent();
            //LoadData();
            //inputConcentrations.DataContext = Concentrations;
            //inputConcentrations.ItemsSource = Concentrations;
            InitializeComponent();
            Loaded += AddInputConcentrations_Loaded;
        }

        private void AddInputConcentrations_Loaded(object sender, RoutedEventArgs e)
        {
            // гарантируем, что база данных создана
            db.Database.EnsureCreated();
            // загружаем данные из БД
            db.InputConcentrations.Load();
            // и устанавливаем данные в качестве контекста
            DataContext = db.InputConcentrations.Local.ToObservableCollection();
        }

        // добавление
        private void addInputConcentrations_Click(object sender, RoutedEventArgs e)
        {
            AddInputConcentrations AddInputConcentrations = new AddInputConcentrations();
            AddInputConcentrations.ShowDialog();
        }

        private void LoadData()
        {
            db.InputConcentrations.Load();
            var inputConc = db.InputConcentrations;

            Concentrations = inputConc.Local.ToObservableCollection();

        }

    }
}
