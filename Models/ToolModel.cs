namespace ConsoleApp6.Models
{
    internal sealed record ToolModel
    {
        public string Type { get; init; } = string.Empty;

        public FunctionModel? Function { get; init; }

        public sealed record FunctionModel
        {
            public string Name { get; init; } = string.Empty;

            public string? Description { get; init; }

            public OpenAIParameters Parameters { get; init; } = OpenAIParameters.Empty;
        }
    }
}
