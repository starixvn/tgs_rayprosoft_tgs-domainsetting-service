namespace Tgs.AgentDomainSettings;

internal class CloudflareResponse<TResult>
{
    public bool Success { get; set; }

    public CloudflareError[] Errors { get; set; } = Array.Empty<CloudflareError>();

    public string[] Messages { get; set; } = Array.Empty<string>();

    public TResult? Result { get; set; }
}
