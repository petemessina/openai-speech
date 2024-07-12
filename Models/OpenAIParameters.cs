namespace ConsoleApp6.Models
{
    internal sealed class OpenAIParameters
    {
        public static readonly OpenAIParameters Empty = new();

        public string Type { get; set; } = "object";

        public Dictionary<string, OpenAIParameter> Properties { get; set; } = [];

        public List<string>? Required { get; set; }
    }

    internal sealed class OpenAIParameter
    {
        public string Type { get; set; } = "object";

        public string? Description { get; set; }
    }
}
