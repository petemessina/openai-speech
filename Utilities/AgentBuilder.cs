using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel;
using ConsoleApp6.Models;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace ConsoleApp6.Utilities
{
    internal class AgentBuilder
    {
        private readonly AssistantModel _model;
        private readonly IKernelBuilder _kernelBuilder;
        private PromptTemplateConfig? _config;
        private string _endpoint;
        private string _apiKey;
        private string _version;
        private readonly KernelPluginCollection _plugins;

        public AgentBuilder(IKernelBuilder kernelBuilder)
        {
            this._kernelBuilder = kernelBuilder;
            this._model = new AssistantModel();
            this._plugins = [];
        }

        public AgentBuilder FromTemplatePath(string templatePath)
        {
            var yamlContent = File.ReadAllText(templatePath);

            this._config = KernelFunctionYaml.ToPromptTemplateConfig(yamlContent);

            this.WithInstructions(this._config.Template.Trim());

            if (!string.IsNullOrWhiteSpace(this._config.Name))
            {
                this.WithName(this._config.Name?.Trim());
            }

            if (!string.IsNullOrWhiteSpace(this._config.Description))
            {
                this.WithDescription(this._config.Description?.Trim());
            }

            return this;
        }

        public AgentBuilder WithInstructions(string instructions)
        {
            this._model.Instructions = instructions;
            return this;
        }

        public AgentBuilder WithName(string? name)
        {
            this._model.Name = name;
            return this;
        }

        public AgentBuilder WithDescription(string? description)
        {
            this._model.Description = description;
            return this;
        }

        public AgentBuilder WithPlugins(IEnumerable<KernelPlugin> plugins)
        {
            this._plugins.AddRange(plugins);
            return this;
        }

        public AgentBuilder WithAzureOpenAIChatCompletion(string endpoint, string model, string apiKey, string? version = null)
        {
            this._apiKey = apiKey;
            this._model.Model = model;
            this._endpoint = $"{endpoint}/openai";
            this._version = version ?? "2024-02-15-preview";

            return this;
        }

        public ChatCompletionAgent Build(CancellationToken cancellationToken = default)
        {
            foreach (KernelPlugin plugin in this._plugins) 
            {
                this._kernelBuilder.Plugins.Add(plugin);
            }

            return new()
            {
                Name = this._model.Name,
                Kernel = _kernelBuilder.Build(),
                Description = this._model.Description,
                Instructions = this._model.Instructions,
                ExecutionSettings = new OpenAIPromptExecutionSettings
                {
                    ModelId = this._config.DefaultExecutionSettings.ModelId,
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                }
            };
        }
    }
}
