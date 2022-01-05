using System.Text.Json.Serialization;

namespace Tgs.DomainSettings;

public class CloudflareDnsZone
{
    public string Id { get; set; } = default!;

    [JsonPropertyName("name")]
    public string DomainName { get; set; } = default!;

    public string Status { get; set; } = default!;
}
