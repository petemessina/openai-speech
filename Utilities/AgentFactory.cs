using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

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
        ) {
            var agentBuilder = new AgentBuilder(kernelBuilder);
            string yamlContent = File.ReadAllText(templatePath);
            PromptTemplateConfig promptTemplateConfig = KernelFunctionYaml.ToPromptTemplateConfig(yamlContent);
            ChatCompletionAgent agent = agentBuilder.FromTemplatePath(templatePath)
                    .WithAzureOpenAIChatCompletion(azureOpenAIEndpoint, promptTemplateConfig.DefaultExecutionSettings.ModelId, apiKey)
                    .WithPlugins(plugins)
                    .Build();

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
        ) {
            var agentBuilder = new AgentBuilder(kernelBuilder);
            string yamlContent = File.ReadAllText(templatePath);
            PromptTemplateConfig promptTemplateConfig = KernelFunctionYaml.ToPromptTemplateConfig(yamlContent);
            ChatCompletionAgent agent = agentBuilder.FromTemplatePath(templatePath)
                    .WithAzureOpenAIChatCompletion(azureOpenAIEndpoint, promptTemplateConfig.DefaultExecutionSettings.ModelId, apiKey)
                    .Build();

            kernelBuilder.Services.AddKeyedSingleton(
                promptTemplateConfig.Name,
                (provider, key) => agent
            );

            return agent;
        }
    }
}
