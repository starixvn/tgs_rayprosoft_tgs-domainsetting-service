namespace Tgs.AgentDomainSettings;

public class CloudflareOptions
{
    public string AccountId { get; set; } = default!;

    public string TgsDomain { get; set; } = default!;

    public Uri CloudflareBaseAddress { get; set; } = default!;

    public string Token { get; set; } = default!;
}
