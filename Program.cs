using Azure.AI.OpenAI;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ConsoleApp6.Models;
using ConsoleApp6.Services;

ApplicationSettings applicationSettings = GetApplicationSettings();
IEnumerable<ChatMessageContent> chatHistory = new List<ChatMessageContent>();
var speechConfig = SpeechConfig.FromSubscription(applicationSettings.AzureAISpeech.Key, applicationSettings.AzureAISpeech.Region);

try
{
    speechConfig.SpeechRecognitionLanguage = "en-US";
    await ChatWithOpenAI();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}

async Task AskOpenAI(string prompt, OpenAIChatService openAIChatService)
{
    speechConfig.SpeechSynthesisVoiceName = "en-US-AvaMultilingualNeural";
    chatHistory = await openAIChatService.Chat(prompt, chatHistory);

    foreach (ChatMessageContent content in chatHistory.Reverse())
    {
        Console.WriteLine($">>>> {content.Role} - {content.AuthorName ?? "*"}: '{content.Content}'");
    }
}

async Task ChatWithOpenAI()
{
    // Should be the locale for the speaker's language.
    using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
    using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
    var conversationEnded = false;
    IKernelBuilder kernelBuilder = CreateKernelWithChatCompletion();
    var audioOutputConfig = AudioConfig.FromDefaultSpeakerOutput();
    OpenAIChatService openAIChatService = new(kernelBuilder, applicationSettings, speechConfig, audioOutputConfig);

    while (!conversationEnded)
    {
        Console.WriteLine("Azure OpenAI is listening. Say 'Stop' or press Ctrl-Z to end the conversation.");

        // Get audio from the microphone and then send it to the TTS service.
        var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();

        switch (speechRecognitionResult.Reason)
        {
            case ResultReason.RecognizedSpeech:
                if (speechRecognitionResult.Text == "Stop.")
                {
                    Console.WriteLine("Conversation ended.");
                    conversationEnded = true;
                }
                else
                {
                    Console.WriteLine($"Recognized speech: {speechRecognitionResult.Text}");
                    await AskOpenAI(speechRecognitionResult.Text, openAIChatService);
                }

                break;
            case ResultReason.NoMatch:
                Console.WriteLine($"No speech could be recognized: ");
                break;
            case ResultReason.Canceled:
                var cancellationDetails = CancellationDetails.FromResult(speechRecognitionResult);
                Console.WriteLine($"Speech Recognition canceled: {cancellationDetails.Reason}");
                if (cancellationDetails.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"Error details={cancellationDetails.ErrorDetails}");
                }

                break;
        }
    }
}

IKernelBuilder CreateKernelWithChatCompletion()
{
    var openAISettings = applicationSettings.OpenAI;
    var builder = Kernel.CreateBuilder();

    builder.AddAzureOpenAIChatCompletion(
        openAISettings.ModelName,
        openAISettings.Endpoint,
        openAISettings.Key);

    return builder;
}

static ApplicationSettings GetApplicationSettings()
{
    IConfigurationRoot config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddUserSecrets(Assembly.GetExecutingAssembly())
        .Build();

    return config.GetSection("ApplicationSettings").Get<ApplicationSettings>();
}