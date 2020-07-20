# FFXIVWeather
FFXIV weather forecast library for C# applications operating on a system with a local game installation. Preferable over the regular version of the package due to its smaller file size.

## Installation
`Install-Package FFXIVWeather.Lumina` or other methods as described [here](https://www.nuget.org/packages/FFXIVWeather.Lumina/).

## Example
Code:
```cs
var lumina = new Lumina.Lumina(pathToGameFolder, new Lumina.LuminaOptions
{
    DefaultExcelLanguage = Lumina.Data.Language.English
});
var weatherService = new FFXIVWeatherLuminaService(lumina);
var zone = "Eureka Pyros";
var count = 15U;

var forecast = weatherService.GetForecast(zone, count);

Console.WriteLine($"Weather for {zone}:");
Console.WriteLine("|\tWeather\t\t|\tTime\t|");
Console.WriteLine("+-----------------------+---------------+");
foreach (var (weather, startTime) in forecast)
{
    Console.WriteLine($"|\t{(weather.ToString().Length < 8 ? weather.ToString() + '\t' : weather.ToString())}\t|\t{Math.Round((startTime - DateTime.UtcNow).TotalMinutes)}m\t|");
}
```

Output:
```
Weather for Eureka Pyros:
|       Weather         |       Time    |
+-----------------------+---------------+
|       Umbral Wind     |       -3m     |
|       Blizzards       |       20m     |
|       Thunder         |       43m     |
|       Umbral Wind     |       67m     |
|       Umbral Wind     |       90m     |
|       Blizzards       |       113m    |
|       Thunder         |       137m    |
|       Heat Waves      |       160m    |
|       Umbral Wind     |       183m    |
|       Blizzards       |       207m    |
|       Heat Waves      |       230m    |
|       Thunder         |       253m    |
|       Blizzards       |       277m    |
|       Umbral Wind     |       300m    |
|       Thunder         |       323m    |
```
