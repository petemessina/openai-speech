using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp6.Plugins
{
    internal sealed class WeatherPlugin
    {
        [KernelFunction, Description("Provides the weather for today.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "Too smart")]
        public string GetTodaysWeather(string location)
        {
            return @"Sunny with a high of 80 and tonight possibly a thunderstorm";
        }
    }
}
