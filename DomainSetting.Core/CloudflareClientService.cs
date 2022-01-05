using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.DomainSettings;

internal class CloudflareClientService : ICloudflareClientService
{
    private static readonly JsonSerializerOptions _JsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    private readonly HttpClient _httpClient;
    private readonly CloudflareOptions _options;

    public CloudflareClientService(HttpClient httpClient, CloudflareOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async IAsyncEnumerable<CloudflareDnsZone> ListZonesAsync(string domainName)
    {
        var response = await _httpClient
            .GetAsync($"/client/v4/zones?name={domainName}")
            .ConfigureAwait(false);

        var responseData = await response.Content
            .ReadFromJsonAsync<CloudflareResponse<CloudflareDnsZone[]>>(_JsonSerializerOptions)
            .ConfigureAwait(false);

        if (responseData?.Success != true)
            throw new Exception(string.Join(
                Environment.NewLine,
                responseData?.Errors.Select(error => $"{error.Code}: {error.Message}") ?? Array.Empty<string>()));

        foreach (var zone in responseData.Result!)
            yield return zone;
    }

    public async ValueTask<CloudflareDnsZone> CreateZoneAsync(string domainName)
    {
        var createInfo = new CloudflareCreateZoneData
        {
            DomainName = domainName,
            Accounts = new CloudflareAccount
            {
                Id = _options.AccountId
            }
        };

        var response = await _httpClient
            .PostAsJsonAsync("/client/v4/zones", createInfo, _JsonSerializerOptions)
            .ConfigureAwait(false);

        var responseData = await response.Content
            .ReadFromJsonAsync<CloudflareResponse<CloudflareDnsZone>>(_JsonSerializerOptions)
            .ConfigureAwait(false);

        if (responseData?.Success != true)
            throw new Exception(string.Join(
                Environment.NewLine,
                responseData?.Errors.Select(error => $"{error.Code}: {error.Message}") ?? Array.Empty<string>()));

        return responseData.Result!;
    }

    public async ValueTask DeleteZoneAsync(string zoneId)
    {
        var response = await _httpClient
            .DeleteAsync($"/client/v4/zones/{zoneId}")
            .ConfigureAwait(false);

        var responseData = await response.Content
            .ReadFromJsonAsync<CloudflareResponse<object>>(_JsonSerializerOptions)
            .ConfigureAwait(false);

        if (responseData?.Success != true)
            throw new Exception(string.Join(
                Environment.NewLine,
                responseData?.Errors.Select(error => $"{error.Code}: {error.Message}") ?? Array.Empty<string>()));
    }

    public async ValueTask CreateCNameInZoneAsync(string zoneId, string cname, string targetSite)
    {
        var createInfo = new CloudfllareCreateDnsRecordData
        {
            Type = "CNAME",
            Name = cname,
            Content = targetSite,
            TimeToLive = 1,
            Proxied = true
        };

        var response = await _httpClient
            .PostAsJsonAsync($"/client/v4/zones/{zoneId}/dns_records", createInfo, _JsonSerializerOptions)
            .ConfigureAwait(false);

        var responseData = await response.Content
            .ReadFromJsonAsync<CloudflareResponse<object>>(_JsonSerializerOptions)
            .ConfigureAwait(false);

        if (responseData?.Success != true)
            throw new Exception(string.Join(
                Environment.NewLine,
                responseData?.Errors.Select(error => $"{error.Code}: {error.Message}") ?? Array.Empty<string>()));
    }
}
