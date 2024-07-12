using Azure.AI.OpenAI;
using ConsoleApp6.Models;
using ConsoleApp6.Plugins;
using ConsoleApp6.Utilities;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Models;

namespace ConsoleApp6.Services
{
    public sealed class OpenAIChatService
    {
        private readonly IKernelBuilder _kernelBuilder;
        private readonly ApplicationSettings _applicationSettings;
        private readonly SpeechConfig _speechConfig;
        private readonly AudioConfig _audioConfig;
        private IReadOnlyList<string> sentenceSaperators = new List<string> { ".", "!", "?", ";", "。", "！", "？", "；", "\n" };
        IEnumerable<ChatMessageContent> _chatHistory = new List<ChatMessageContent>();

        Agent internalLeaderAgent;
        Agent weatherAgent;
        Agent homeAgent;
        Kernel kernel;
        KernelFunction innerSelectionFunction;
        KernelArguments arguments;

        public OpenAIChatService(
            IKernelBuilder kernelBuilder,
            ApplicationSettings applicationSettings,
            SpeechConfig speechConfig,
            AudioConfig audioConfig
        ) {
            _kernelBuilder = kernelBuilder;
            _applicationSettings = applicationSettings;
            _speechConfig = speechConfig;
            _audioConfig = audioConfig;

            internalLeaderAgent = CreateLeaderAgent();
            weatherAgent = CreateWeatherAgent();
            homeAgent = CreateHomeAutomationAgent();
            kernel = CreateKernelWithChatCompletion();
            innerSelectionFunction = CreateInnerSelectionFunction();
            arguments = new()
            {
                ["internalLeaderName"] = internalLeaderAgent.Name,
                ["weatherAgentName"] = weatherAgent.Name,
                ["homeAgentName"] = homeAgent.Name
            };
        }

        public async Task<List<ChatMessageContent>> Chat(string prompt, IEnumerable<ChatMessageContent> chatHistory)
        {
            _chatHistory = chatHistory;
            KernelFunction outerTerminationFunction = CreateOuterTerminationFunction();
            AggregatorAgent myAssistantAgent = new(CreateChat)
            {
                Name = "MyAssistant",
                Mode = AggregatorMode.Nested,
            };

            AgentGroupChat chat = new(myAssistantAgent)
            {
                ExecutionSettings = new()
                {
                    TerminationStrategy = new KernelFunctionTerminationStrategy(outerTerminationFunction, CreateKernelWithChatCompletion())
                    {
                        ResultParser = (result) =>
                        {
                            OuterTerminationResult? jsonResult = JsonResultTranslator.Translate<OuterTerminationResult>(result.GetValue<string>());
                            return (jsonResult?.isAnswered ?? false) || (jsonResult?.waitingForUser ?? false);
                        },
                        MaximumIterations = 5,
                    },
                }
            };

            foreach (var chatMessage in _chatHistory)
            {
                chat.AddChatMessage(chatMessage);
            }

            chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, prompt));

            await foreach (var content in chat.InvokeAsync())
            {
                if (sentenceSaperators.Any(content.Content.Contains))
                {
                    var sentence = content.Content.Trim();
                    if (!string.IsNullOrEmpty(sentence))
                    {
                        using var speechSynthesizer = new SpeechSynthesizer(_speechConfig, _audioConfig);
                        await speechSynthesizer.SpeakTextAsync(sentence);
                    }
                }
            }

            return await chat.GetChatMessagesAsync().ToListAsync();
        }

        public AgentGroupChat CreateChat()
        {
            return new(internalLeaderAgent, weatherAgent, homeAgent)
            {
                ExecutionSettings = new()
                {
                    SelectionStrategy = new KernelFunctionSelectionStrategy(innerSelectionFunction, kernel)
                    {
                        Arguments = arguments,
                        ResultParser =
                            (result) =>
                            {
                                AgentSelectionResult? jsonResult = JsonResultTranslator.Translate<AgentSelectionResult>(result.GetValue<string>());
                                string? agentName = string.IsNullOrWhiteSpace(jsonResult?.name) ? null : jsonResult?.name;
                                Console.WriteLine(agentName);
                                return agentName ?? internalLeaderAgent.Name;
                            }
                    },
                    TerminationStrategy = new AgentTerminationStrategy()
                    {
                        Agents = [internalLeaderAgent],
                        MaximumIterations = 7,
                        AutomaticReset = true,
                    }
                }
            };
        }

        public Kernel CreateKernelWithChatCompletion()
        {
            var builder = Kernel.CreateBuilder();

            builder.AddAzureOpenAIChatCompletion(
                _applicationSettings.OpenAI.ModelName,
                _applicationSettings.OpenAI.Endpoint,
                _applicationSettings.OpenAI.Key);

            return builder.Build();
        }

        public KernelFunction CreateInnerSelectionFunction()
        {
            string innerSelectionInstructions = File.ReadAllText(_applicationSettings.InternalSelectionPromptFileLocation);
            OpenAIPromptExecutionSettings jsonSettings = new() { ResponseFormat = ChatCompletionsResponseFormat.JsonObject };

            return KernelFunctionYaml.FromPromptYaml(
                innerSelectionInstructions,
                new HandlebarsPromptTemplateFactory()
            );
        }

        public KernelFunction CreateOuterTerminationFunction()
        {
            string outerTerminationInstructions = File.ReadAllText(_applicationSettings.OuterTerminationPromptFileLocation);
            OpenAIPromptExecutionSettings jsonSettings = new() { ResponseFormat = ChatCompletionsResponseFormat.JsonObject };

            return KernelFunctionYaml.FromPromptYaml(
                outerTerminationInstructions,
                new HandlebarsPromptTemplateFactory()
            );
        }

        public ChatCompletionAgent CreateLeaderAgent() 
        {
            return AgentFactory.AgentFromFileTemplate(
                _kernelBuilder,
                _applicationSettings.LeaderPromptFileLocation,
                _applicationSettings.OpenAI.Endpoint,
                _applicationSettings.OpenAI.Key
            );
        }

        public ChatCompletionAgent CreateWeatherAgent() 
        {
            KernelPlugin weatherPlugin = KernelPluginFactory.CreateFromType<WeatherPlugin>();
            return AgentFactory.AgentFromFileTemplate(
                _kernelBuilder,
                _applicationSettings.WeatherAgentPromptFileLocation,
                _applicationSettings.OpenAI.Endpoint,
                _applicationSettings.OpenAI.Key,
                new List<KernelPlugin> { weatherPlugin }
            );
        }

        public ChatCompletionAgent CreateHomeAutomationAgent() 
        {
            KernelPlugin homePlugin = KernelPluginFactory.CreateFromType<HomeAssistantPlugin>();
            return AgentFactory.AgentFromFileTemplate(
                _kernelBuilder,
                _applicationSettings.HomeAutomationPromptFileLocation,
                _applicationSettings.OpenAI.Endpoint,
                _applicationSettings.OpenAI.Key,
                new List<KernelPlugin> { homePlugin }
            );
        }
    }
}
