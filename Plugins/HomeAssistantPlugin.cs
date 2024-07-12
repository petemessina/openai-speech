using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp6.Plugins
{
    internal sealed class HomeAssistantPlugin
    {
        [KernelFunction, Description("Provides the humidity in the house.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "Too smart")]
        public string GetHomeHumidity()
        {
            return @"65%";
        }
    }
}
