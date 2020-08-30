using System;
using System.Collections.Generic;
using System.Text;

namespace Telegram_bot
{
    public class Weather
    {
        public double Temp  { get; set; } // Температура

        public double speed { get; set; } // Скорость ветра

        public int all      { get; set; } // Облачность

        public int sunrise  { get; set; } // Время рассвета

        public int sunset   { get; set; } // Время заката
        public int humidity { get; set; } // Влажность
    }

    public class WeatherResponse
    {
        public Weather Main   { get; set; }
        public Weather wind   { get; set; }
        public Weather clouds { get; set; }
        public string Name    { get; set; }
        public Weather sys    { get; set; }

    }
}
