namespace Tgs.DomainSettings;

public class CloudflareApiException : Exception
{
    public CloudflareApiException(string? message) : base(message)
    {
    }
}
