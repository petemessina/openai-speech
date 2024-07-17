using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace ConsoleApp6.Plugins
{
    internal sealed class WeatherPlugin
    {
        [KernelFunction, Description("Provides the weather for today.")]
        public string GetTodaysWeather(string location)
        {
            return @"Sunny with a high of 80 and tonight possibly a thunderstorm";
        }
    }
}
