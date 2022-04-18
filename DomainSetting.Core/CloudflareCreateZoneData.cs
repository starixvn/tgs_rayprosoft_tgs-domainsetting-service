using System.Text.Json.Serialization;

namespace Tgs.DomainSettings;

internal class CloudflareCreateZoneData
{
    [JsonPropertyName("name")]
    public string DomainName { get; init; } = default!;

    [JsonPropertyName("account")]
    public CloudflareAccount Accounts { get; init; } = default!;

    public string Type { get; init; } = "full";

    [JsonPropertyName("jump_start")]
    public bool JumpStart { get; init; } = false;
}
