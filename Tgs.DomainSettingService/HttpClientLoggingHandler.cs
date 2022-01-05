namespace Tgs.AgentDomainSettingService;

public class HttpClientLoggingHandler : DelegatingHandler
{
    private readonly ILogger<HttpClientLoggingHandler> _logger;

    public HttpClientLoggingHandler(ILogger<HttpClientLoggingHandler> logger)
        : base()
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Request: {request}");

        if (request.Content != null)
        {
            _logger.LogInformation($"Request Body: {await request.Content.ReadAsStringAsync().ConfigureAwait(false)}");
        }

        HttpResponseMessage response = await base
            .SendAsync(request, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation($"Response: {response}");
        if (response.Content != null)
        {
            _logger.LogInformation($"Response Body: {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
        }

        return response;
    }
}
