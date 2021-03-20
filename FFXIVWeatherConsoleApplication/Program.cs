using FFXIVWeather.Lumina;
using System;
using Lumina;

namespace FFXIVWeatherConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var pathToGameFolder = args[0];
            var lumina = new GameData(pathToGameFolder);
            var weatherService = new FFXIVWeatherLuminaService(lumina);
            const string zone = "Eureka Pyros";
            const uint count = 15U;

            /*var stopwatch = new Stopwatch();
            for (var i = 0; i < 100000; i++)
            {
                stopwatch.Start();
                weatherService.GetForecast(zone, count);
                stopwatch.Stop();
            }

            Console.WriteLine($"Finished in {stopwatch.ElapsedMilliseconds}ms.");*/

            var forecast = weatherService.GetForecast(zone, count);

            Console.WriteLine($"Weather for {zone}:");
            Console.WriteLine("|\tWeather\t\t|\tTime\t|");
            Console.WriteLine("+-----------------------+---------------+");
            foreach (var (weather, startTime) in forecast)
            {
                Console.WriteLine($"|\t{(((string)weather.Name).Length < 8 ? weather.Name + '\t' : weather.Name)}\t|\t{Math.Round((startTime - DateTime.UtcNow).TotalMinutes)}m\t|");
            }
        }
    }
}
