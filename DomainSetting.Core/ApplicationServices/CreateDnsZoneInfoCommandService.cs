using Tgs.ServiceUtility;

namespace Tgs.DomainSettings.ApplicationServices;

internal class CreateDnsZoneInfoCommandService : ICommandService<CreateDnsZoneInfoCommand>
{
    private readonly ITgsRepository _tgsRepository;
    private readonly ICloudflareClientService _cloudflareClient;
    private readonly CloudflareOptions _options;

    public CreateDnsZoneInfoCommandService(
        ITgsRepository tgsRepository,
        ICloudflareClientService cloudflareClient,
        CloudflareOptions options)
    {
        _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
        _cloudflareClient = cloudflareClient ?? throw new ArgumentNullException(nameof(cloudflareClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async ValueTask ExecuteAsync(CreateDnsZoneInfoCommand command, CancellationToken cancellationToken = default)
    {
        var agentDomains = await _tgsRepository
            .QueryAgentDomainsAsync(command.AgentId)
            .ToArrayAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (!agentDomains.Any())
            return;

        foreach (var domain in agentDomains)
        {
            var zone = await _cloudflareClient.CreateZoneAsync(domain).ConfigureAwait(false);

            await _cloudflareClient
                .CreateCNameInZoneAsync(zone.Id, domain, _options.TgsDomain)
                .ConfigureAwait(false);
            await _cloudflareClient
                .CreateCNameInZoneAsync(zone.Id, $"www.{domain}", _options.TgsDomain)
                .ConfigureAwait(false);

            await _cloudflareClient
                .SetZoneAutomaticHttpsRewritesAsync(zone.Id)
                .ConfigureAwait(false);
            await _cloudflareClient
                .SetZoneAlwaysUseHttpsAsync(zone.Id)
                .ConfigureAwait(false);
            await _cloudflareClient
                .SetZoneResultMinifyAsync(zone.Id)
                .ConfigureAwait(false);
            await _cloudflareClient
                .SetZoneBrotliAsync(zone.Id)
                .ConfigureAwait(false);
        }
    }
}
