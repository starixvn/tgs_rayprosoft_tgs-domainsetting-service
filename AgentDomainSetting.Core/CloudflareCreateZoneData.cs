using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tgs.AgentDomainSettings;

internal class CloudflareCreateZoneData
{
    [JsonPropertyName("name")]
    public string DomainName { get; init; } = default!;

    [JsonPropertyName("account")]
    public CloudflareAccount Accounts { get; init; } = default!;

    public string Type { get; init; } = "full";
}
