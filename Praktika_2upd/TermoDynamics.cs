using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Praktika_2upd
{
    public class HeatEquationSolver
    {
        private int n1, n2; // Количество узлов для каждого стержня
        private double h;    // Шаг по пространству (одинаковый для обоих стержней)
        private double tau;  // Шаг по времени
        private double a2;   // Коэффициент температуропроводности

        private double[] x1, x2; // Координаты узлов
        private double[] t;      // Временные слои
        private double[,] u1, u2; // Температуры в стержнях

        double[,] totalU1, totalU2;

        public HeatEquationSolver(int n1, int n2, double L1, double L2, double totalTime, double a2)
        {
            this.n1 = n1;
            this.n2 = n2;
            this.a2 = a2;

            h = L1 / n1; // Одинаковый шаг для обоих стержней
            tau = totalTime / 100; // Примерный шаг по времени

            x1 = new double[n1 + 1];
            x2 = new double[n2 + 1];
            u1 = new double[n1 + 1, 2]; // Только текущий и следующий слой
            u2 = new double[n2 + 1, 2];

            totalU1 = new double[n1 + 1, 101]; // Переменные для хранения всей функции температуры
            totalU2 = new double[n2 + 1, 101];

            // Инициализация сетки
            for (int i = 0; i <= n1; i++) x1[i] = i * h;
            for (int j = 0; j <= n2; j++) x2[j] = j * h;

            // Начальные условия
            for (int i = 0; i <= n1; i++) u1[i, 0] = 0;
            for (int j = 0; j <= n2; j++) u2[j, 0] = 0;
        }

        public void Solve(double T0)
        {
            for (int k = 1; k <= 100; k++) // Итерации по времени
            {
                // Решение для первого стержня
                SolveRod(u1, n1, T0, 0);

                // Условия на стыке
                double u_interface = (u1[n1, 1] + u2[0, 1]) / 2; // Среднее значение
                u1[n1, 1] = u_interface;
                u2[0, 1] = u_interface;

                // Решение для второго стержня
                SolveRod(u2, n2, u_interface, 1);


                // Переход на следующий временной слой
                for (int i = 0; i <= n1; i++)
                {
                    u1[i, 0] = u1[i, 1];
                    totalU1[i, k] = u1[i, 1];
                }

                for (int j = 0; j <= n2; j++)
                {
                    u2[j, 0] = u2[j, 1];
                    totalU2[j, k] = u2[j, 1];
                }
            }
        }

        private void SolveRod(double[,] u, int n, double leftBoundary, int boundaryType)
        {
            // Реализация метода прогонки для одного стержня
            // boundaryType: 0 - фиксированная температура, 1 - теплоизоляция

            double alpha = a2 * tau / (h * h);
            double[] a = new double[n + 1];
            double[] b = new double[n + 1];
            double[] c = new double[n + 1];
            double[] d = new double[n + 1];

            // Заполнение матрицы (теперь до n-1)
            for (int i = 1; i < n; i++) //Цикл до n-1, так как граничные условия обрабатываются отдельно
            {
                a[i] = -alpha;
                b[i] = 1 + 2 * alpha;
                c[i] = -alpha;
                d[i] = u[i, 0];
            }

            // Граничные условия
            if (boundaryType == 0) //Первый стержень
            {
                // Фиксированная температура на левом конце (условие Дирихле)
                b[0] = 1;
                c[0] = 0;
                d[0] = leftBoundary; //T0
                a[n] = -alpha;
                b[n] = 1 + 2 * alpha;
                d[n] = u[n, 0];
            }
            else //Второй стержень
            {

                //Условие теплообмена на правом конце (теперь правильно!)
                double h_теплоотдачи = 10; // Примерное значение (Вт/(м^2*К))
                double T_окружающей_среды = 20; // Примерная температура окружающей среды (градусы Цельсия)
                double k = 50; // Примерная теплопроводность стали (Вт/(м*К))

                // Аппроксимируем производную du/dx на границе (n) с помощью разности назад
                b[n] = 1 + alpha + (h_теплоотдачи * h / k); // Главная диагональ для правой границы
                a[n] = -alpha; // Нижняя диагональ для правой границы
                d[n] = u[n, 0] + (h_теплоотдачи * h / k) * T_окружающей_среды; // Правая часть с учетом теплообмена

                //Левый конец (0) второго стержня - условие Дирихле от интерфейса
                b[0] = 1;
                c[0] = 0;
                d[0] = leftBoundary;
            }

            // Решение методом прогонки
            double[] alpha_coef = new double[n + 1];
            double[] beta_coef = new double[n + 1];

            alpha_coef[0] = -c[0] / b[0];
            beta_coef[0] = d[0] / b[0];

            for (int i = 1; i <= n; i++)
            {
                double denom = b[i] + a[i] * alpha_coef[i - 1];
                alpha_coef[i] = -c[i] / denom;
                beta_coef[i] = (d[i] - a[i] * beta_coef[i - 1]) / denom;
            }

            u[n, 1] = beta_coef[n];
            for (int i = n - 1; i >= 0; i--)
            {
                u[i, 1] = alpha_coef[i] * u[i + 1, 1] + beta_coef[i];
            }
        }

        public (double[,], double[,]) GetTemperatureDistribution()
        {
            return (totalU1, totalU2);
        }

        public double GetTotalLength()
        {
            return (n1 + n2) * h;
        }

        public double GetFirstRodLength()
        {
            return n1 * h;
        }
    }
}
