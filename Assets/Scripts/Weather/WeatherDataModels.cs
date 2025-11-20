using System;

namespace CrowdSimulator.Weather
{
    [Serializable]
    public class WeatherApiResponse
    {
        public CurrentWeather current;
        public HourlyWeather hourly;
    }

    [Serializable]
    public class CurrentWeather
    {
        public string time;
        public float temperature_2m;
        public float wind_speed_10m;
    }

    [Serializable]
    public class HourlyWeather
    {
        public string[] time;
        public float[] temperature_2m;
        public float[] relative_humidity_2m;
        public float[] wind_speed_10m;
    }
}
