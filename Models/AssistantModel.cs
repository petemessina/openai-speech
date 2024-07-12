namespace ConsoleApp6.Models
{
    internal sealed record AssistantModel
    {
        public string Id { get; init; } = string.Empty;

        public long CreatedAt { get; init; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string Model { get; set; } = string.Empty;

        public string Instructions { get; set; } = string.Empty;

        public List<ToolModel> Tools { get; init; } = [];

        public List<string> FileIds { get; init; } = [];

        public Dictionary<string, object> Metadata { get; init; } = [];

        public sealed class FileModel
        {
            public string AssistantId { get; set; } = string.Empty;

            public string Id { get; set; } = string.Empty;

            public long CreatedAt { get; init; }
        }
    }
}
