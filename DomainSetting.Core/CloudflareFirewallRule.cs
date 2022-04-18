using System.Text.Json.Serialization;

namespace Tgs.DomainSettings;

public class CloudflareFirewallRule
{
    [JsonPropertyName("action")]
    public string Action { get; init; } = default!;

    [JsonPropertyName("paused")]
    public bool Paused { get; init; } = false;

    [JsonPropertyName("description")]
    public string Description { get; init; } = default!;

    [JsonPropertyName("filter")]
    public CloudflareFirewallRuleFilter Filter { get; init; } = default!;
}
