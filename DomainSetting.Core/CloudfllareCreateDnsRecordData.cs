using System.Text.Json.Serialization;

namespace Tgs.AgentDomainSettings;

internal class CloudfllareCreateDnsRecordData
{
    public string Type { get; init; } = default!;

    public string Name { get; init; } = default!;

    public string Content { get; init; } = default!;

    [JsonPropertyName("ttl")]
    public int TimeToLive { get; init; }

    public int? Priority { get; init; }

    public bool? Proxied { get; init; }
}
