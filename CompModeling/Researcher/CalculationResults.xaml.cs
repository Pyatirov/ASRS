using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CompModeling
{
    /// <summary>
    /// Логика взаимодействия для CalculationResults.xaml
    /// </summary>
    public partial class CalculationResults : Window
    {
        public ObservableCollection<ObservableCollection<int>>? MatrixData { get; set; }

        public CalculationResults(List<List<int>> coeffs)
        {
            InitializeComponent();
            ConvertToMatrix(coeffs);
            CreateColumns();
            dg_Component_Matrix.ItemsSource = MatrixData;
        }

        private void ConvertToMatrix(List<List<int>> coeffs)
        {
            MatrixData = new ObservableCollection<ObservableCollection<int>>(
                coeffs.Select(row => new ObservableCollection<int>(row))
            );
        }

        private void CreateColumns()
        {
            if (MatrixData!.Any())
            {
                int columnsCount = MatrixData!.First().Count;

                for (int i = 0; i < columnsCount; i++)
                {
                    dg_Component_Matrix.Columns.Add(new DataGridTextColumn
                    {
                        Header = $"Столбец {i + 1}",
                        Binding = new Binding($"[{i}]")
                    });
                }
            }
        }
    }
}
