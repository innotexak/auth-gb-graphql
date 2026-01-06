using System.Text.Json.Serialization;

namespace Auth.API.Services;

public sealed class GraphQLResponse<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("errors")]
    public List<GraphQLError>? Errors { get; set; }
}

public sealed class GraphQLError
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("extensions")]
    public Dictionary<string, object>? Extensions { get; set; }
}


