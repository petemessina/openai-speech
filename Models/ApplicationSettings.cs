namespace ConsoleApp6.Models
{
    public record ApplicationSettings(
        string PromptFileLocation,
        string GrammarFileLocation,
        string LeaderPromptFileLocation,
        string WeatherAgentPromptFileLocation,
        string HomeAutomationPromptFileLocation,
        string InternalSelectionPromptFileLocation,
        string OuterTerminationPromptFileLocation,
        OpenAI OpenAI,
        AzureAISearch AzureAISearch,
        AzureAISpeech AzureAISpeech
    );

    public record OpenAI(
        string Endpoint,
        string ModelName,
        string Key
    );

    public record AzureAISearch(
        string ResourceName,
        string Key,
        string IndexName,
        string LanguageName
    );

    public record AzureAISpeech(
        string Region,
        string Key
    );
}
