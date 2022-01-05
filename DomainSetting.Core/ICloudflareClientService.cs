namespace Tgs.DomainSettings;

public interface ICloudflareClientService
{
    ValueTask CreateCNameInZoneAsync(string zoneId, string cname, string targetSite);

    ValueTask<CloudflareDnsZone> CreateZoneAsync(string domainName);

    ValueTask DeleteZoneAsync(string zoneId);

    IAsyncEnumerable<CloudflareDnsZone> ListZonesAsync(string domainName);
}
