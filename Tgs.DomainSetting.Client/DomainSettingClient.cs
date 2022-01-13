namespace Tgs.DomainSettings.Clients;

internal class DomainSettingClient : IDomainSettingClient
{
    private readonly HttpClient _httpClient;

    public DomainSettingClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async ValueTask ApplyAgentDomainIngressAsync(Guid agentId)
    {
        var response = await _httpClient
            .PutAsync($"/agent/{agentId}", null)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
    }

    public async ValueTask DeleteAgentDomainIngressAsync(Guid agentId)
    {
        var response = await _httpClient
            .DeleteAsync($"/agent/{agentId}")
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
    }

    public async ValueTask CreateZoneAsync(Guid agentId)
    {
        var response = await _httpClient
            .PutAsync($"/cf/{agentId}", null)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
    }

    public async ValueTask DeleteZoneAsync(Guid agentId)
    {
        var response = await _httpClient
            .DeleteAsync($"/cf/{agentId}")
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
    }
}
