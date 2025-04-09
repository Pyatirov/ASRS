using MathNet.Numerics.LinearAlgebra;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
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
        public ObservableCollection<MatrixRow> Rows { get; } = new ObservableCollection<MatrixRow>();
        private readonly List<BaseForm> _baseForms;
        private readonly List<FormingForm> _formingForms;
        private readonly List<List<int>> _componentMatrix;
        private List<List<double>> initial_b;
        private List<List<double>> XStart;
        private List<double> inputK;
        private List<List<double>>? results;
        private int pointsCount;

        public CalculationResults(
            List<BaseForm> baseForms,
            List<FormingForm> formingForms,
            List<List<int>> componentMatrix,
            List<List<double>> _b,
            List<List<double>> _XStart, 
            List<double> _inputK,
            int _pointsCount)
        {
            InitializeComponent();

            _baseForms = baseForms ?? throw new System.ArgumentNullException(nameof(baseForms));
            _formingForms = formingForms ?? throw new System.ArgumentNullException(nameof(formingForms));
            _componentMatrix = componentMatrix ?? throw new System.ArgumentNullException(nameof(componentMatrix));
            initial_b = _b ?? throw new System.ArgumentNullException(nameof(_b));
            XStart = _XStart ?? throw new System.ArgumentNullException(nameof(_XStart));
            inputK = _inputK ?? throw new System.ArgumentNullException(nameof(_inputK));
            pointsCount = _pointsCount;


            ValidateData();
            InitializeGridColumns();
            PopulateRows();
            DataContext = this;
            List<double> xInitial = new List<double>();
            List<double> b = new List<double>();
            for (int i = 0; i < pointsCount; i++)
            {
                xInitial = XStart[i];
                b = xInitial;
                var solver = new PythonSolver();
                var solution = solver.Solve(xInitial, inputK, b);
            }

        }

        private void ValidateData()
        {
            if (_componentMatrix.Count != _formingForms.Count)
                throw new ArgumentException("Количество строк в матрице не совпадает с количеством образующих форм");

            if (_componentMatrix.Any(row => row.Count != _baseForms.Count))
                throw new ArgumentException("Количество столбцов в матрице не совпадает с количеством базовых форм");
        }

        private void InitializeGridColumns()
        {
            // Очищаем существующие колонки
            dg_Component_Matrix.Columns.Clear();

            // Первая колонка - названия образующих форм
            dg_Component_Matrix.Columns.Add(new DataGridTextColumn
            {
                Header = "Образующиеся формы",
                Binding = new Binding("FormingFormName"),
                Width = new DataGridLength(2, DataGridLengthUnitType.Star)
            });

            // Колонки для базовых форм
            foreach (var baseForm in _baseForms)
            {
                dg_Component_Matrix.Columns.Add(new DataGridTextColumn
                {
                    Header = baseForm.Name,
                    Binding = new Binding($"Coefficients[{baseForm.Name}]"),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                });
            }
        }

        private void PopulateRows()
        {
            for (int i = 0; i < _formingForms.Count; i++)
            {
                var rowData = new MatrixRow
                {
                    FormingFormName = _formingForms[i].Name,
                    Coefficients = new Dictionary<string, int>()
                };

                for (int j = 0; j < _baseForms.Count; j++)
                {
                    rowData.Coefficients[_baseForms[j].Name!] = _componentMatrix[i][j];
                }

                Rows.Add(rowData);
            }
        }


    }
    public class PythonSolver
        {
            public List<double> Solve(
                List<double> xInitial,
                List<double> K,
                List<double> b)
            {
                // 1. Подготовка входных данных
                var input = new
                {
                    x = xInitial,
                    K = K,
                    b = b
                };
                string jsonInput = JsonConvert.SerializeObject(input);

                // 2. Настройка процесса Python
                var processInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "main.py",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                    StandardErrorEncoding = System.Text.Encoding.UTF8
                };

                // 3. Запуск скрипта
                using (var process = new Process())
                {
                    process.StartInfo = processInfo;
                    process.Start();

                    // 4. Передача данных в Python
                    using (var sw = process.StandardInput)
                    {
                        sw.WriteLine(jsonInput);
                    }

                    // 5. Чтение результатов
                    string jsonOutput = process.StandardOutput.ReadToEnd();
                    string errors = process.StandardError.ReadToEnd();
                    process.WaitForExit(5000);

                    Console.WriteLine("Логи Python:");
                    Console.WriteLine(errors);

                    if (process.ExitCode != 0)
                        throw new Exception($"Python error: {errors}");

                    // 6. Десериализация результатов
                    dynamic output = JsonConvert.DeserializeObject(jsonOutput);
                    return (
                        output.x.ToObject<List<double>>()
                    );
                }
            }
        }

    public class MatrixRow
    {
        public string? FormingFormName { get; set; }
        public Dictionary<string, int>? Coefficients { get; set; }
    }
}

