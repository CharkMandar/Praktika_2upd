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
        }
    }
}
