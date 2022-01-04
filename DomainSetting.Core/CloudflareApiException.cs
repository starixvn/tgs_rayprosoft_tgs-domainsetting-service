namespace Tgs.AgentDomainSettings;

public class CloudflareApiException : Exception
{
    public CloudflareApiException(string? message) : base(message)
    {
    }
}
