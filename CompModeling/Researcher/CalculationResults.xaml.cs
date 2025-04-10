using MathNet.Numerics.LinearAlgebra;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

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
        private int pointsCount;
        private List<List<double>> expPointsValues;

        public CalculationResults(
            List<BaseForm> baseForms,
            List<FormingForm> formingForms,
            List<List<int>> componentMatrix,
            List<List<double>> _b,
            List<List<double>> _XStart, 
            List<double> _inputK,
            int _pointsCount,
            List<List<double>> _expPointsValues)
        {
            InitializeComponent();

            _baseForms = baseForms ?? throw new System.ArgumentNullException(nameof(baseForms));
            _formingForms = formingForms ?? throw new System.ArgumentNullException(nameof(formingForms));
            _componentMatrix = componentMatrix ?? throw new System.ArgumentNullException(nameof(componentMatrix));
            initial_b = _b ?? throw new System.ArgumentNullException(nameof(_b));
            XStart = _XStart ?? throw new System.ArgumentNullException(nameof(_XStart));
            inputK = _inputK ?? throw new System.ArgumentNullException(nameof(_inputK));
            pointsCount = _pointsCount;
            expPointsValues = _expPointsValues;


            ValidateData();
            InitializeGridColumns();
            PopulateRows();
            DataContext = this;
            List<double> xInitial = new List<double>();
            List<double> b = new List<double>();
            List<List<double>> Solutions = new List<List<double>>();
            List<List<double>> optimalSolutions = new List<List<double>>();
            for (int i = 0; i < pointsCount; i++)
            {
                xInitial = XStart[i];
                b = xInitial;
                var solver = new PythonSolver();
                var solution = solver.Solve(xInitial, inputK, b);
                Solutions.Add(solution);
                List<double> optimalInputK = new List<double>(inputK);
                optimalInputK[12] = 28.973;
                optimalInputK[13] = 0;
                var newsolver = new PythonSolver();
                var optimalsolution = solver.Solve(xInitial, optimalInputK, b);
                optimalSolutions.Add(optimalsolution);
            }

            List<List<double>> Entry = new List<List<double>>();

            foreach (var str in Solutions)
            {
                for (int i = 0; i < _baseForms.Count; i++)
                {
                    tb_Results.Text += $"x[{i}] = " + Math.Round(str[i], 3).ToString() + '\n';
                }
                tb_Results.Text += '\n';

            }

            tb_Results.Text += '\n';

            for (int i = 0; i < Solutions.Count; i++)
            {
                tb_Results.Text += $"Точка №{i + 1}\n\n";
                tb_Results.Text += "Концентрации образующихся форм:\n";
                List<double> xFinal = new List<double>(Solutions[i]);

                xFinal.Add(inputK[5] * xFinal[1] * xFinal[2]);
                tb_Results.Text += $" x6 = {Math.Round(xFinal[xFinal.Count - 1], 4)}\tHNO3\n";

                xFinal.Add(inputK[6] * xFinal[1] * xFinal[4]);
                tb_Results.Text += $" x7 = {Math.Round(xFinal[xFinal.Count - 1],4)}\tNH4NO3\n";

                xFinal.Add(inputK[7] * xFinal[0] * xFinal[1]);
                tb_Results.Text += $" x8 = {Math.Round(xFinal[xFinal.Count - 1], 4)}\tLaNO3\n";

                xFinal.Add(inputK[8] * xFinal[1] * Math.Pow(xFinal[3], 2) * xFinal[4]);
                tb_Results.Text += $" x9 = {Math.Round(xFinal[xFinal.Count - 1], 4)}\tNH4NO3TBP2\n";

                xFinal.Add(inputK[9] * Math.Pow(xFinal[1], 3) * Math.Pow(xFinal[2], 3) * xFinal[3]);
                tb_Results.Text += $" x10 = {Math.Round(xFinal[xFinal.Count - 1], 4)}\tH3(NO3)3TBP\n";

                xFinal.Add(inputK[10] * xFinal[1] * xFinal[2] * xFinal[3]);
                tb_Results.Text += $" x11 = {Math.Round(xFinal[xFinal.Count - 1], 4)}\tHNO3TBP\n";

                xFinal.Add(inputK[11] * Math.Pow(xFinal[1], 2) * Math.Pow(xFinal[2], 2) * xFinal[3]);
                tb_Results.Text += $" x12 = {Math.Round(xFinal[xFinal.Count - 1], 4)}\tH2(NO3)2TBP\n";

                xFinal.Add(inputK[12] * xFinal[0] * Math.Pow(xFinal[1], 3) * Math.Pow(xFinal[3], 3));
                tb_Results.Text += $" x13 = {Math.Round(xFinal[xFinal.Count - 1], 4)}\tLa(NO3)3TBP3\n";

                xFinal.Add(inputK[13] * xFinal[0] * Math.Pow(xFinal[1], 3) * Math.Pow(xFinal[3], 4));
                tb_Results.Text += $" x14 = {Math.Round(xFinal[xFinal.Count - 1], 4)}\tLa(NO3)3TBP4\n";

                xFinal.Add(inputK[14] * Math.Pow(xFinal[3], 2));
                tb_Results.Text += $" x15 = {Math.Round(xFinal[xFinal.Count - 1], 4)}\tTBP\n\n";

                Entry.Add(xFinal);

                tb_Results.Text += "Равновесные концентрации в водной и органической фазах\n\n";

                tb_Results.Text += $"La_wat = {Math.Round(xFinal[0] + xFinal[7], 4)} моль/л\n";
                tb_Results.Text += $"La_org = {Math.Round(xFinal[12] + xFinal[13], 4)} моль/л\n\n";

                tb_Results.Text += $"NO3_wat = {Math.Round(xFinal[1] + xFinal[5]) + xFinal[6] + xFinal[7], 4} моль/л\n";
                tb_Results.Text += $"NO3_org = {Math.Round(xFinal[8] + 3 * xFinal[9] + xFinal[10] + 
                    2 * xFinal[11] + 3 * xFinal[12] + 3 * xFinal[13], 4)} моль/л\n\n";

                tb_Results.Text += $"H_wat = {Math.Round(xFinal[2] + xFinal[5])} моль/л\n";
                tb_Results.Text += $"H_org = {Math.Round(3 * xFinal[9] + xFinal[10] + 2 * xFinal[11], 4)} моль/л\n\n";

                tb_Results.Text += $"TBP_wat = 0 моль/л\n";
                tb_Results.Text += $"TBP_org = {Math.Round(xFinal[3] + 2 * xFinal[9] + xFinal[10] +
                    xFinal[11] + 3 * xFinal[12] + 4 * xFinal[13] + 2 * xFinal[14], 4)} моль/л\n\n";

                tb_Results.Text += $"NH4_wat = {Math.Round(xFinal[4] + xFinal[6], 4)} моль/л\n";
                tb_Results.Text += $"NH4_org = {Math.Round(xFinal[8], 4)} моль/л\n\n";
            }

            
            List<List<double>> calcEqConcs = new List<List<double>>();
            for (int i = 0; i < optimalSolutions.Count; i++)
            {
                tb_Results.Text += "Оптимальные значения: \n";
                tb_Results.Text += $"Точка №{i + 1}\n\n";
                tb_Results.Text += "Концентрации образующихся форм:\n";
                List<double> xFinalOptimal = new List<double>(Solutions[i]);

                xFinalOptimal.Add(inputK[5] * xFinalOptimal[1] * xFinalOptimal[2]);
                tb_Results.Text += $" x6 = {Math.Round(xFinalOptimal[xFinalOptimal.Count - 1], 4)}\tHNO3\n";

                xFinalOptimal.Add(inputK[6] * xFinalOptimal[1] * xFinalOptimal[4]);
                tb_Results.Text += $" x7 = {Math.Round(xFinalOptimal[xFinalOptimal.Count - 1], 4)}\tNH4NO3\n";

                xFinalOptimal.Add(inputK[7] * xFinalOptimal[0] * xFinalOptimal[1]);
                tb_Results.Text += $" x8 = {Math.Round(xFinalOptimal[xFinalOptimal.Count - 1], 4)}\tLaNO3\n";

                xFinalOptimal.Add(inputK[8] * xFinalOptimal[1] * Math.Pow(xFinalOptimal[3], 2) * xFinalOptimal[4]);
                tb_Results.Text += $" x9 = {Math.Round(xFinalOptimal[xFinalOptimal.Count - 1], 4)}\tNH4NO3TBP2\n";

                xFinalOptimal.Add(inputK[9] * Math.Pow(xFinalOptimal[1], 3) * Math.Pow(xFinalOptimal[2], 3) * xFinalOptimal[3]);
                tb_Results.Text += $" x10 = {Math.Round(xFinalOptimal[xFinalOptimal.Count - 1], 4)}\tH3(NO3)3TBP\n";

                xFinalOptimal.Add(inputK[10] * xFinalOptimal[1] * xFinalOptimal[2] * xFinalOptimal[3]);
                tb_Results.Text += $" x11 = {Math.Round(xFinalOptimal[xFinalOptimal.Count - 1], 4)}\tHNO3TBP\n";

                xFinalOptimal.Add(inputK[11] * Math.Pow(xFinalOptimal[1], 2) * Math.Pow(xFinalOptimal[2], 2) * xFinalOptimal[3]);
                tb_Results.Text += $" x12 = {Math.Round(xFinalOptimal[xFinalOptimal.Count - 1], 4)}\tH2(NO3)2TBP\n";

                xFinalOptimal.Add(inputK[12] * xFinalOptimal[0] * Math.Pow(xFinalOptimal[1], 3) * Math.Pow(xFinalOptimal[3], 3));
                tb_Results.Text += $" x13 = {Math.Round(xFinalOptimal[xFinalOptimal.Count - 1], 4)}\tLa(NO3)3TBP3\n";

                xFinalOptimal.Add(inputK[13] * xFinalOptimal[0] * Math.Pow(xFinalOptimal[1], 3) * Math.Pow(xFinalOptimal[3], 4));
                tb_Results.Text += $" x14 = {Math.Round(xFinalOptimal[xFinalOptimal.Count - 1], 4)}\tLa(NO3)3TBP4\n";

                xFinalOptimal.Add(inputK[14] * Math.Pow(xFinalOptimal[3], 2));
                tb_Results.Text += $" x15 = {Math.Round(xFinalOptimal[xFinalOptimal.Count - 1], 4)}\tTBP\n\n";

                Entry.Add(xFinalOptimal);

                tb_Results.Text += "Равновесные концентрации в водной и органической фазах\n\n";

                calcEqConcs.Add(new List<double>());

                tb_Results.Text += $"La_wat = {Math.Round(xFinalOptimal[0] + xFinalOptimal[7], 4)} моль/л\n";
                calcEqConcs[i].Add(xFinalOptimal[0] + xFinalOptimal[7]);
                tb_Results.Text += $"La_org = {Math.Round(xFinalOptimal[12] + xFinalOptimal[13], 4)} моль/л\n\n";
                calcEqConcs[i].Add(xFinalOptimal[12] + xFinalOptimal[13]);

                tb_Results.Text += $"NO3_wat = {Math.Round(xFinalOptimal[1] + xFinalOptimal[5] + xFinalOptimal[6] + xFinalOptimal[7],4)} моль/л\n";
                calcEqConcs[i].Add(xFinalOptimal[1] + xFinalOptimal[5] + xFinalOptimal[6] + xFinalOptimal[7]);

                tb_Results.Text += $"NO3_org = {Math.Round(xFinalOptimal[8] + 3 * xFinalOptimal[9] + xFinalOptimal[10] +
                    2 * xFinalOptimal[11] + 3 * xFinalOptimal[12] + 3 * xFinalOptimal[13], 4)} моль/л\n\n";
                calcEqConcs[i].Add(xFinalOptimal[8] + 3 * xFinalOptimal[9] + xFinalOptimal[10] +
                    2 * xFinalOptimal[11] + 3 * xFinalOptimal[12] + 3 * xFinalOptimal[13]);


                tb_Results.Text += $"H_wat = {Math.Round(xFinalOptimal[2] + xFinalOptimal[5])} моль/л\n";
                calcEqConcs[i].Add(xFinalOptimal[2] + xFinalOptimal[5]);

                tb_Results.Text += $"H_org = {Math.Round(3 * xFinalOptimal[9] + xFinalOptimal[10] + 2 * xFinalOptimal[11], 4)} моль/л\n\n";
                calcEqConcs[i].Add(3 * xFinalOptimal[9] + xFinalOptimal[10] + 2 * xFinalOptimal[11]);


                tb_Results.Text += $"TBP_wat = 0 моль/л\n";
                calcEqConcs[i].Add(0);
                tb_Results.Text += $"TBP_org = {Math.Round(xFinalOptimal[3] + 2 * xFinalOptimal[9] + xFinalOptimal[10] +
                    xFinalOptimal[11] + 3 * xFinalOptimal[12] + 4 * xFinalOptimal[13] + 2 * xFinalOptimal[14], 4)} моль/л\n\n";
                calcEqConcs[i].Add(xFinalOptimal[3] + 2 * xFinalOptimal[9] + xFinalOptimal[10] +
                    xFinalOptimal[11] + 3 * xFinalOptimal[12] + 4 * xFinalOptimal[13] + 2 * xFinalOptimal[14]);

                tb_Results.Text += $"NH4_wat = {Math.Round(xFinalOptimal[4] + xFinalOptimal[6], 4)} моль/л\n";
                calcEqConcs[i].Add(xFinalOptimal[4] + xFinalOptimal[6]);

                tb_Results.Text += $"NH4_org = {Math.Round(xFinalOptimal[8], 4)} моль/л\n\n";
                calcEqConcs[i].Add(xFinalOptimal[8]);
            }

            List<double> result = new List<double>();

            double totalSum = 0;

            for (int i = 0; i < expPointsValues.Count; i++)
            {
                List<double> expPoint = expPointsValues[i];
                List<double> calcPoint = calcEqConcs[i];
                double sum = 0;

                for (int j = 0; j < expPoint.Count; j += 2)
                {
                    double exp_wat = expPoint[j];
                    double exp_org = expPoint[j + 1];
                    double calc_wat = calcPoint[j];
                    double calc_org = calcPoint[j + 1];

                    // Проверка на деление на ноль для C_wat
                    double term_wat = 0;
                    if (calc_wat != 0 && exp_wat != 0) // [[2]]
                    {
                        term_wat = Math.Pow((exp_wat / calc_wat) - 1, 2) + Math.Pow((calc_wat / exp_wat) - 1, 2);
                    }

                    // Проверка на деление на ноль для C_org
                    double term_org = 0;
                    if (calc_org != 0 && exp_org != 0) // [[2]]
                    {
                        term_org = Math.Pow((exp_org / calc_org) - 1, 2) + Math.Pow((calc_org / exp_org) - 1, 2);
                    }

                    sum += term_wat + term_org;

                    tb_Results.Text += $"Сумма квадратов отклонений в точке №{i + 1} (пара {j / 2 + 1}):\n\n";
                    tb_Results.Text += sum.ToString("F4") + "\n"; // [[2]]
                }
                totalSum += sum;
                result.Add(sum);
            }

            tb_Results.Text += $"Сумма по всем точкам F = {totalSum}";

            //List<List<double>> totalCalcConcs = new List<List<double>>();
            //foreach (var innerList in calcEqConcs)
            //{
            //    List<double> pairedSumList = new List<double>();
            //    for (int i = 0; i < innerList.Count; i += 2)
            //    {
            //        pairedSumList.Add(Math.Round(innerList[i] + innerList[i + 1], 4)); // [[1]]
            //    }
            //    totalCalcConcs.Add(pairedSumList);
            //}

            //for (int i = 0; i < totalCalcConcs.Count; i++)
            //{
            //    for (int j = 0; j < b.Count; j++)
            //    {

            //    }
            //}

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
                    StandardErrorEncoding = System.Text.Encoding.UTF8,
                    CreateNoWindow = true
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
                    dynamic output = JsonConvert.DeserializeObject(jsonOutput)!;
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

