using System.Text.Json.Serialization;

namespace Tgs.DomainSettings;

public class CloudflareFirewallRuleFilter
{
    [JsonPropertyName("expression")]
    public string Expression { get; init; } = default!;

    [JsonPropertyName("paused")]
    public bool Paused { get; init; } = false;
}
