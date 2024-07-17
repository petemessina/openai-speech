using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace ConsoleApp6.Plugins
{
    internal sealed class HomeAssistantPlugin
    {
        [KernelFunction, Description("Provides the humidity in the house.")]
        public string GetHomeHumidity()
        {
            return @"65%";
        }
    }
}
