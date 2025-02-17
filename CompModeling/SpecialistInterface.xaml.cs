using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
//using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using static CompModeling.ConnectToDB;

namespace CompModeling
{
    /// <summary>
    /// Логика взаимодействия для SpecialistInterface.xaml
    /// </summary>
    public partial class SpecialistInterface : Window
    {
        ApplicationContext db = new ApplicationContext();


        public SpecialistInterface()
        {
            InitializeComponent();
            LoadDataAsync();
        }
        /// <summary>
        /// Загрузка данных из БД при загрузке окна
        /// </summary>
        //private void SpecialistInterface_Loaded(object sender, RoutedEventArgs e)
        //{
        //    // гарантируем, что база данных создана
        //    db.Database.EnsureCreated();
        //    // загружаем данные из БД
        //    db.InputConcentrations.Load();
        //    // и устанавливаем данные в качестве контекста
        //    LoadDataAsync();
        //}

        /// <summary>
        /// Добавление концентрации в DataGrid
        /// </summary>
        private void addInputConcentrations_Click(object sender, RoutedEventArgs e)
        {
            AddInputConcentrations AddInputConcentrations = new AddInputConcentrations();
            AddInputConcentrations.ShowDialog();
        }

        private async void LoadDataAsync()
        {
            try
            {
                using (var context = new ApplicationContext())
                {
                    // Получаем данные из таблицы InputConcentrations
                    var concentrations = await context.InputConcentrations.ToListAsync();
                    var forms = await context.BaseForms.ToListAsync();
                    var phases = await context.Phases.ToListAsync();

                    // Привязываем данные к DataGrid
                    inputConcentrations.ItemsSource = concentrations;
                    filterForm.ItemsSource = forms;
                    filterPhase.ItemsSource = phases;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}
