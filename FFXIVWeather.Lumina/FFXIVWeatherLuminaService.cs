﻿using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using Cyalume = Lumina.GameData;

namespace FFXIVWeather.Lumina
{
    public class FFXIVWeatherLuminaService : IFFXIVWeatherLuminaService
    {
        private const double Seconds = 1;
        private const double Minutes = 60 * Seconds;
        private const double WeatherPeriod = 23 * Minutes + 20 * Seconds;

        private static readonly DateTime UnixEpoch = new(1970, 1, 1);

        private readonly Cyalume cyalume;

        public FFXIVWeatherLuminaService(Cyalume lumina)
        {
            this.cyalume = lumina;
        }

        public IList<(Weather, DateTime)> GetForecast(string placeName, uint count = 1, double secondIncrement = WeatherPeriod, double initialOffset = 0 * Minutes)
            => GetForecast(GetTerritory(placeName), count, secondIncrement, initialOffset);

        public IList<(Weather, DateTime)> GetForecast(int terriTypeId, uint count = 1, double secondIncrement = WeatherPeriod, double initialOffset = 0 * Minutes)
            => GetForecast(GetTerritory(terriTypeId), count, secondIncrement, initialOffset);

        public IList<(Weather, DateTime)> GetForecast(TerritoryType terriType, uint count = 1, double secondIncrement = WeatherPeriod, double initialOffset = 0 * Minutes)
        {
            if (count == 0) return Array.Empty<(Weather, DateTime)>();

            var weatherRateIndex = GetTerritoryTypeWeatherRateIndex(terriType);

            // Initialize the return value with the current stuff
            var forecast = new List<(Weather, DateTime)> { GetCurrentWeather(terriType, initialOffset) };

            // Fill out the list
            for (var i = 1; i < count; i++)
            {
                var time = forecast[0].Item2.AddSeconds(i * secondIncrement);
                var weatherTarget = CalculateTarget(time);
                var weather = GetWeather(weatherRateIndex, weatherTarget);
                forecast.Add((weather, time));
            }

            return forecast;
        }

        public (Weather, DateTime) GetCurrentWeather(string placeName, double initialOffset = 0 * Minutes)
            => GetCurrentWeather(GetTerritory(placeName), initialOffset);

        public (Weather, DateTime) GetCurrentWeather(int terriTypeId, double initialOffset = 0 * Minutes)
            => GetCurrentWeather(GetTerritory(terriTypeId), initialOffset);

        public (Weather, DateTime) GetCurrentWeather(TerritoryType terriType, double initialOffset = 0 * Minutes)
        {
            var rootTime = GetCurrentWeatherRootTime(initialOffset);
            var target = CalculateTarget(rootTime);

            var weatherRateIndex = GetTerritoryTypeWeatherRateIndex(terriType);
            var weather = GetWeather(weatherRateIndex, target);

            return (weather, rootTime);
        }

        private Weather GetWeather(WeatherRate weatherRateIndex, int target)
        {
            // Based on our constraints, we know there's no null case here.
            // Every zone has at least one target at 100, and weatherTarget's domain is [0,99].
            var rateAccumulator = 0;
            var weatherId = -1;
            for (var i = 0; i < weatherRateIndex.Rate.Count; i++)
            {
                // var w = weatherRateIndex.UnkData0[i];

                rateAccumulator += weatherRateIndex.Rate[i];
                if (target < rateAccumulator)
                {
                    weatherId = (int)weatherRateIndex.Weather[i].RowId;
                    break;
                }
            }

            if (weatherId == -1)
            {
                throw new ArgumentException("No weather matching the provided parameters was found.", nameof(target));
            }

            var weather = this.cyalume.GetExcelSheet<Weather>().ToList()[weatherId];
            return weather;
        }

        private WeatherRate GetTerritoryTypeWeatherRateIndex(TerritoryType terriType)
        {
            var terriTypeWeatherRateId = terriType.WeatherRate;
            var weatherRateIndex = this.cyalume.GetExcelSheet<WeatherRate>().ToList()[terriTypeWeatherRateId];
            return weatherRateIndex;
        }

        private TerritoryType GetTerritory(string placeName)
        {
            var ciPlaceName = placeName.ToLowerInvariant();
            var terriType = this.cyalume.GetExcelSheet<TerritoryType>().FirstOrDefault(tt => ((string)tt.PlaceName.Value.Name.ExtractText()).ToLowerInvariant() == ciPlaceName);
            if (terriType.RowId == 0) throw new ArgumentException("Specified place does not exist.", nameof(placeName));
            return terriType;
        }

        private TerritoryType GetTerritory(int terriTypeId)
        {
            var terriType = this.cyalume.GetExcelSheet<TerritoryType>().FirstOrDefault(tt => tt.RowId == terriTypeId);
            if (terriType.RowId == 0) throw new ArgumentException("Specified territory type does not exist.", nameof(terriTypeId));
            return terriType;
        }

        private static DateTime GetCurrentWeatherRootTime(double initialOffset)
        {
            // Calibrate the time to the beginning of the weather period
            var now = DateTime.UtcNow;
            var adjustedNow = now.AddMilliseconds(-now.Millisecond).AddSeconds(initialOffset);
            var rootTime = adjustedNow;
            var seconds = (long)(rootTime - UnixEpoch).TotalSeconds % WeatherPeriod;
            rootTime = rootTime.AddSeconds(-seconds);
            return rootTime;
        }

        /// <summary>
        ///     Calculate the value used for the <see cref="WeatherRate"/> at a specific <see cref="DateTime" />.
        ///     This method is lifted straight from SaintCoinach.
        /// </summary>
        /// <param name="time"><see cref="DateTime"/> for which to calculate the value.</param>
        /// <returns>The value from 0..99 (inclusive) calculated based on <paramref name="time"/>.</returns>
        private static int CalculateTarget(DateTime time)
        {
            var unix = (int)(time - UnixEpoch).TotalSeconds;
            // Get Eorzea hour for weather start
            var bell = unix / 175;
            // Do the magic 'cause for calculations 16:00 is 0, 00:00 is 8 and 08:00 is 16
            var increment = ((uint)(bell + 8 - (bell % 8))) % 24;

            // Take Eorzea days since unix epoch
            var totalDays = (uint)(unix / 4200);

            var calcBase = (totalDays * 0x64) + increment;

            var step1 = (calcBase << 0xB) ^ calcBase;
            var step2 = (step1 >> 8) ^ step1;

            return (int)(step2 % 0x64);
        }
    }
}
