using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
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

namespace CompModeling
{
    /// <summary>
    /// Логика взаимодействия для ResearcherInterface.xaml
    /// </summary>
    public partial class ResearcherInterface : Window
    {
        public ResearcherInterface()
        {
            InitializeComponent();
            // Исходные данные
            double Vw = 1.0; // Объем водной фазы
            double Vo = 1.0; // Объем органической фазы
            double[] b = { 1.0, 1.0, 1.0, 1.0, 1.0 }; // Вектор исходных концентраций
            double[] K = { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 }; // Константы реакций

            // Начальные приближения для неизвестных концентраций
            double[] initialGuess = { 1.0, 1.0, 1.0, 1.0, 1.0 };

            // Решение системы уравнений методом Ньютона-Рафсона
            double[] solution = NewtonRaphsonSolve(initialGuess, Vw, Vo, b, K);

            // Вывод результатов
            Console.WriteLine("Решение системы уравнений:");
            for (int i = 0; i < solution.Length; i++)
            {
                Console.WriteLine($"C{i + 1} = {solution[i]}");
            }

            static double[] NewtonRaphsonSolve(double[] initialGuess, double Vw, double Vo, double[] b, double[] K)
            {
                double tolerance = 1e-6; // Точность решения
                int maxIterations = 100; // Максимальное количество итераций
                double[] x = (double[])initialGuess.Clone(); // Начальное приближение

                for (int iteration = 0; iteration < maxIterations; iteration++)
                {
                    // Вычисляем вектор функций F(x)
                    double[] F = Equations(x, Vw, Vo, b, K);

                    // Вычисляем якобиан J(x)
                    double[,] J = Jacobian(x, Vw, Vo, b, K);

                    // Решаем систему линейных уравнений J * deltaX = -F
                    var matrixJ = Matrix<double>.Build.DenseOfArray(J);
                    var vectorF = Vector<double>.Build.Dense(F);
                    var deltaX = matrixJ.Solve(-vectorF);

                    // Обновляем решение
                    for (int i = 0; i < x.Length; i++)
                    {
                        x[i] += deltaX[i];
                    }

                    // Проверяем условие сходимости
                    if (deltaX.L2Norm() < tolerance)
                    {
                        Console.WriteLine($"Сходимость достигнута на итерации {iteration + 1}");
                        break;
                    }
                }

                return x;
            }

            static double[] Equations(double[] C, double Vw, double Vo, double[] b, double[] K)
            {
                double[] F = new double[5];

                // Уравнения материального баланса
                F[0] = Vw * C[0] + Vw * K[7] * C[0] * C[1] + Vo * K[12] * C[0] * Math.Pow(C[1], 2) * Math.Pow(C[2], 2) + Vo * K[13] * C[0] * Math.Pow(C[1], 3) * Math.Pow(C[3], 4) - Vw * b[0];
                F[1] = Vw * C[1] + Vw * K[5] * C[1] * C[2] + Vw * K[6] * C[1] * C[4] + Vw * K[7] * C[0] * C[1] + Vo * K[8] * C[1] * Math.Pow(C[3], 2) * C[4] + Vo * 3 * K[9] * Math.Pow(C[1], 3) * Math.Pow(C[2], 3) * C[3] + Vo * K[10] * C[1] * C[2] * C[3] + Vo * 2 * K[11] * Math.Pow(C[1], 2) * Math.Pow(C[2], 2) * C[3] + Vo * 3 * K[12] * C[0] * Math.Pow(C[1], 2) * Math.Pow(C[2], 2) + Vo * 3 * K[13] * C[0] * Math.Pow(C[1], 3) * Math.Pow(C[3], 4) - Vw * b[1];
                F[2] = Vw * C[2] + Vw * K[5] * C[1] * C[2] + Vo * 3 * K[9] * Math.Pow(C[1], 3) * Math.Pow(C[2], 3) * C[3] + Vo * K[10] * C[1] * C[2] * C[3] + Vo * 2 * K[11] * Math.Pow(C[1], 2) * Math.Pow(C[2], 2) * C[3] - Vw * b[2];
                F[3] = Vw * C[3] + Vo * 2 * K[8] * C[1] * Math.Pow(C[3], 2) * C[4] + Vo * K[9] * Math.Pow(C[1], 3) * Math.Pow(C[2], 3) * C[3] + Vo * K[10] * C[1] * C[2] * C[3] + Vo * K[11] * Math.Pow(C[1], 2) * Math.Pow(C[2], 2) * C[3] + Vo * 3 * K[12] * C[0] * Math.Pow(C[1], 2) * Math.Pow(C[2], 2) + Vo * 4 * K[13] * C[0] * Math.Pow(C[1], 3) * Math.Pow(C[3], 4) + Vo * 2 * K[14] * Math.Pow(C[3], 2) - Vo * b[3];
                F[4] = Vw * C[4] + Vw * K[6] * C[1] * C[4] + Vo * K[8] * C[1] * Math.Pow(C[3], 2) * C[4] - Vw * b[4];

                return F;
            }

            static double[,] Jacobian(double[] C, double Vw, double Vo, double[] b, double[] K)
            {
                double[,] J = new double[5, 5];

                // Вычисляем частные производные для якобиана
                J[0, 0] = Vw + Vw * K[7] * C[1] + Vo * K[12] * Math.Pow(C[1], 2) * Math.Pow(C[2], 2) + Vo * K[13] * Math.Pow(C[1], 3) * Math.Pow(C[3], 4);
                J[0, 1] = Vw * K[7] * C[0] + Vo * 2 * K[12] * C[0] * C[1] * Math.Pow(C[2], 2) + Vo * 3 * K[13] * C[0] * Math.Pow(C[1], 2) * Math.Pow(C[3], 4);
                J[0, 2] = Vo * 2 * K[12] * C[0] * Math.Pow(C[1], 2) * C[2];
                J[0, 3] = Vo * 4 * K[13] * C[0] * Math.Pow(C[1], 3) * Math.Pow(C[3], 3);
                J[0, 4] = 0;

                // Аналогично вычисляем остальные элементы якобиана...
                // (Здесь нужно заполнить остальные элементы матрицы J)

                return J;
            }
        }
    }
}