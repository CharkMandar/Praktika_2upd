using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;



namespace Praktika_2upd
{
    internal class Temperature
    {
        public PlotModel Model { get; private set; }
        private HeatEquationSolver solver;

        // Параметры трубы
        private double OuterRadius = 100;
        private double InnerRadius = 50;
        private double CenterX = 250;
        private double CenterY = 250;
        private int Resolution = 1000;

        public Temperature(int n1, int n2, double L1, double L2, double totalTime, double a2)
        {
            Model = new PlotModel { Title = "Распределение температуры в трубе" };
            solver = new HeatEquationSolver(n1, n2, L1, L2, totalTime, a2);

            InitializeModel();
        }

        private void InitializeModel()
        {
            InnerRadius = Form1.tr2;
            OuterRadius = Form1.tr1;
            // Настройка осей
            var xAxis = new LinearAxis { Position = AxisPosition.Bottom, IsAxisVisible = false };
            var yAxis = new LinearAxis { Position = AxisPosition.Left, IsAxisVisible = false };
            Model.Axes.Add(xAxis);
            Model.Axes.Add(yAxis);

            // Цветовая шкала
            var colorAxis = new LinearColorAxis
            {
                Palette = OxyPalettes.Jet(100),
                Position = AxisPosition.Right,
                Title = "Температура (°C)"
            };
            Model.Axes.Add(colorAxis);
        }

        public void UpdateVisualization(double T0)
        {
            // Выполняем расчет теплопроводности
            solver.Solve(T0);

            // Получаем распределение температуры
            double[,] temperatureData = GenerateTemperatureData();

            // Обновляем визуализацию
            Model.Series.Clear();

            var heatMapSeries = new HeatMapSeries
            {
                X0 = 0,
                Y0 = 0,
                X1 = Resolution,
                Y1 = Resolution,
                Data = temperatureData,
                Interpolate = true
            };

            Model.Series.Add(heatMapSeries);

            // Обновляем диапазон цветовой шкалы
            var colorAxis = (LinearColorAxis)Model.Axes.FirstOrDefault(a => a is LinearColorAxis);
            if (colorAxis != null)
            {
                colorAxis.Minimum = temperatureData.Cast<double>().Min();
                colorAxis.Maximum = temperatureData.Cast<double>().Max();
            }

            Model.InvalidatePlot(true);
        }

        private double[,] GenerateTemperatureData()
        {
            double[,] temperatureData = new double[Resolution, Resolution];
            var (u1, u2) = solver.GetTemperatureDistribution();

            for (int i = 0; i < Resolution; i++)
            {
                for (int j = 0; j < Resolution; j++)
                {
                    // Преобразуем индексы i и j в координаты x и y в пространстве визуализации.
                    // Считаем, что (0,0) соответствует левому верхнему углу, а (Resolution, Resolution) - правому нижнему.
                    double x = (double)i / Resolution * (2 * OuterRadius) - OuterRadius;
                    double y = (double)j / Resolution * (2 * OuterRadius) - OuterRadius;

                    // Вычисляем расстояние от центра трубы.
                    double distance = Math.Sqrt(x * x + y * y);

                    if (distance < InnerRadius)
                    {
                        // Внутренняя часть трубы: используем первый стержень.
                        // Нормализуем расстояние относительно внутреннего радиуса.
                        double normalizedPos = distance / InnerRadius;

                        // Преобразуем нормализованную позицию в индекс в массиве u1.
                        // Убедимся, что индекс не выходит за границы массива.
                        int index = Math.Min((int)(normalizedPos * (u1.GetLength(0) - 1)), u1.GetLength(0) - 1);


                        temperatureData[i, j] = u1[index, 10]; // Текущий временной слой
                    }
                    else if (distance <= OuterRadius)
                    {
                        // Стенка трубы: используем второй стержень.
                        // Нормализуем расстояние относительно диапазона радиусов стенки.
                        double normalizedPos = (distance - InnerRadius) / (OuterRadius - InnerRadius);

                        // Преобразуем нормализованную позицию в индекс в массиве u2.
                        // Убедимся, что индекс не выходит за границы массива.
                        int index = Math.Min((int)(normalizedPos * (u2.GetLength(0) - 1)), u2.GetLength(0) - 1);


                        temperatureData[i, j] = u2[index, 10]; // Текущий временной слой
                    }
                    else
                    {
                        // Вне трубы: задаем минимальную температуру (или другую логику).
                        temperatureData[i, j] = 20;
                    }
                }
            }

            return temperatureData;
        }

    }
}


/*public PlotModel Model { get; private set; }

        // Константы для трубы (в пикселях, для примера)
        private const double OuterRadius = 100;
        private const double InnerRadius = 50;
        private const double CenterX = 250;
        private const double CenterY = 250;

        // Разрешение модели (количество точек для вычисления температуры)
        private const int Resolution = 500;

        public Temperature()
        {
            Model = new PlotModel { };

            // Создаем оси (можно их скрыть, если нужно)
            var xAxis = new LinearAxis { Position = AxisPosition.Bottom, IsAxisVisible = true };
            var yAxis = new LinearAxis { Position = AxisPosition.Left, IsAxisVisible = true };
            Model.Axes.Add(xAxis);
            Model.Axes.Add(yAxis);

            // Генерируем данные о температуре
            double[,] temperatureData = GenerateTemperatureData();

            // Создаем HeatMapSeries
            var heatMapSeries = new HeatMapSeries
            {
                X0 = 0,
                Y0 = 0,
                X1 = Resolution,
                Y1 = Resolution,
                Data = temperatureData,
                Interpolate = true // Отключаем интерполяцию, чтобы видеть отдельные пиксели
            };
            Model.Series.Add(heatMapSeries);

            // Создаем цветовую шкалу
            var colorAxis = new LinearColorAxis
            {
                Palette = OxyPalettes.Jet(100), // Используем палитру Jet
                Minimum = temperatureData.Cast<double>().Min(), // Автоматическое определение минимума
                Maximum = temperatureData.Cast<double>().Max(), // Автоматическое определение максимума
                Title = "Температура (°C)"
            };
            Model.Axes.Add(colorAxis);
        }

        // Функция для генерации данных о температуре (нужно заменить на вашу логику!)
        private double[,] GenerateTemperatureData()
        {
            double[,] temperatureData = new double[Resolution, Resolution];

            for (int i = 0; i < Resolution; i++)
            {
                for (int j = 0; j < Resolution; j++)
                {
                    // Переводим индекс пикселя в координаты на плоскости
                    double x = (double)i / Resolution * (2 * OuterRadius) - OuterRadius;
                    double y = (double)j / Resolution * (2 * OuterRadius) - OuterRadius;

                    // Сдвигаем координаты относительно центра трубы
                    //x += CenterX - OuterRadius;
                    //y += CenterY - OuterRadius;

                    // Вычисляем расстояние от центра трубы
                    double distance = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

                    // Применяем логику расчета температуры в зависимости от расстояния
                    if (distance < InnerRadius)
                    {
                        // Внутри внутренней трубы: самая высокая температура
                        temperatureData[i, j] = 80;
                    }
                    else if (distance > OuterRadius)
                    {
                        // Вне внешней трубы: самая низкая температура
                        temperatureData[i, j] = 20;
                    }
                    else
                    {
                        // Между трубами: линейное изменение температуры
                        double normalizedDistance = (distance - InnerRadius) / (OuterRadius - InnerRadius);
                        temperatureData[i, j] = 80 - normalizedDistance * (80 - 20);
                    }
                }
            }

            return temperatureData;
        }

        // Проверка находится ли точка внутри трубы
        private bool IsInsidePipe(double x, double y)
        {
            double distance = Math.Sqrt(Math.Pow(x - CenterX, 2) + Math.Pow(y - CenterY, 2));
            return distance >= InnerRadius && distance <= OuterRadius;
        }

        public PlotModel GetModel()
        {
            return Model;
        }*/