using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace CrowdSimulator.Weather
{
    public class WeatherDisplay : MonoBehaviour
    {
        [Header("API")]
        [SerializeField]
        private string apiUrl = "https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41&current=temperature_2m,wind_speed_10m&hourly=temperature_2m,relative_humidity_2m,wind_speed_10m";

        [Header("UI")]
        [SerializeField]
        private TMP_Text temperatureText;

        [SerializeField]
        private TMP_Text windSpeedText;

        [SerializeField]
        private TMP_Text humidityText;

        [Header("Weather Effects")]
        [Tooltip("Particle system that plays when the weather is calm/clear.")]
        [SerializeField]
        private ParticleSystem clearEffect;

        [Tooltip("Particle system that plays when humidity suggests rain.")]
        [SerializeField]
        private ParticleSystem rainEffect;

        [Tooltip("Particle system that plays when temperature is below freezing and humidity is high.")]
        [SerializeField]
        private ParticleSystem snowEffect;

        [Tooltip("Particle system that plays when wind speed is high.")]
        [SerializeField]
        private ParticleSystem windEffect;

        [SerializeField]
        private float windyThreshold = 8f;

        [SerializeField]
        private float humidThreshold = 80f;

        [SerializeField]
        private float freezingTemperature = 1f;

        private void Start()
        {
            StartCoroutine(FetchWeather());
        }

        private IEnumerator FetchWeather()
        {
            using (var request = UnityWebRequest.Get(apiUrl))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    DisplayError($"Weather download failed: {request.error}");
                    yield break;
                }

                var payload = request.downloadHandler.text;
                var response = JsonUtility.FromJson<WeatherApiResponse>(payload);

                if (response == null)
                {
                    DisplayError("Weather data could not be parsed.");
                    yield break;
                }

                UpdateUI(response);
                UpdateEffects(response);
            }
        }

        private void UpdateUI(WeatherApiResponse data)
        {
            if (data.current != null)
            {
                temperatureText.SetText($"Temp: {data.current.temperature_2m:0.#}°C");
                windSpeedText.SetText($"Wind: {data.current.wind_speed_10m:0.#} m/s");
            }

            var humidity = GetFirstHourlyHumidity(data);
            if (humidity.HasValue)
            {
                humidityText.SetText($"Humidity: {humidity.Value:0.#}%");
            }
            else
            {
                humidityText.SetText("Humidity: n/a");
            }
        }

        private void UpdateEffects(WeatherApiResponse data)
        {
            var humidity = GetFirstHourlyHumidity(data) ?? 0f;
            var temperature = data.current?.temperature_2m ?? 0f;
            var windSpeed = data.current?.wind_speed_10m ?? 0f;

            var shouldSnow = temperature <= freezingTemperature && humidity >= humidThreshold;
            var shouldRain = temperature > freezingTemperature && humidity >= humidThreshold;
            var shouldBeClear = !shouldSnow && !shouldRain;
            var isWindy = windSpeed >= windyThreshold;

            SetEffectActive(clearEffect, shouldBeClear);
            SetEffectActive(rainEffect, shouldRain);
            SetEffectActive(snowEffect, shouldSnow);
            SetEffectActive(windEffect, isWindy);
        }

        private void SetEffectActive(ParticleSystem effect, bool shouldPlay)
        {
            if (effect == null)
            {
                return;
            }

            var isPlaying = effect.isPlaying;

            if (shouldPlay && !isPlaying)
            {
                effect.Play();
            }
            else if (!shouldPlay && isPlaying)
            {
                effect.Stop();
            }
        }

        private float? GetFirstHourlyHumidity(WeatherApiResponse data)
        {
            if (data.hourly?.relative_humidity_2m != null && data.hourly.relative_humidity_2m.Length > 0)
            {
                return data.hourly.relative_humidity_2m[0];
            }

            return null;
        }

        private void DisplayError(string message)
        {
            if (temperatureText != null)
            {
                temperatureText.SetText(message);
            }

            if (windSpeedText != null)
            {
                windSpeedText.SetText(string.Empty);
            }

            if (humidityText != null)
            {
                humidityText.SetText(string.Empty);
            }
        }
    }
}
