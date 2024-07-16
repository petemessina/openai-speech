using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace ConsoleApp6.Utilities
{
    internal class AgentFactory
    {
        public static ChatCompletionAgent AgentFromFileTemplate(
            IKernelBuilder kernelBuilder,
            string templatePath,
            string azureOpenAIEndpoint,
            string apiKey,
            IEnumerable<KernelPlugin> plugins
        )
        {
            string yamlContent = File.ReadAllText(templatePath);
            PromptTemplateConfig promptTemplateConfig = KernelFunctionYaml.ToPromptTemplateConfig(yamlContent);
            ChatCompletionAgent agent = BuildAgent(templatePath, kernelBuilder, plugins);

            kernelBuilder.Services.AddKeyedSingleton(
                promptTemplateConfig.Name,
                (provider, key) => agent
            );

            return agent;
        }

        public static ChatCompletionAgent AgentFromFileTemplate(
            IKernelBuilder kernelBuilder,
            string templatePath,
            string azureOpenAIEndpoint,
            string apiKey
        )
        {
            string yamlContent = File.ReadAllText(templatePath);
            PromptTemplateConfig promptTemplateConfig = KernelFunctionYaml.ToPromptTemplateConfig(yamlContent);
            ChatCompletionAgent agent = BuildAgent(templatePath, kernelBuilder);

            kernelBuilder.Services.AddKeyedSingleton(
                promptTemplateConfig.Name,
                (provider, key) => agent
            );

            return agent;
        }

        private static ChatCompletionAgent BuildAgent(
            string templatePath,
            IKernelBuilder kernelBuilder,
            IEnumerable<KernelPlugin> plugins = null
        )
        {
            var yamlContent = File.ReadAllText(templatePath);
            var config = KernelFunctionYaml.ToPromptTemplateConfig(yamlContent);

            if (plugins != null)
            {
                foreach (KernelPlugin plugin in plugins)
                {
                    kernelBuilder.Plugins.Add(plugin);
                }
            }

            return new()
            {
                Name = config.Name,
                Kernel = kernelBuilder.Build(),
                Description = config.Description,
                Instructions = config.Template.Trim(),
                ExecutionSettings = new OpenAIPromptExecutionSettings
                {
                    ModelId = config.DefaultExecutionSettings.ModelId,
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                }
            };
        }
    }
}
