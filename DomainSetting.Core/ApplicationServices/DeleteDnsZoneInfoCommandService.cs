using Tgs.ServiceUtility;

namespace Tgs.DomainSettings.ApplicationServices;

internal class DeleteDnsZoneInfoCommandService : ICommandService<DeleteDnsZoneInfoCommand>
{
    private readonly ITgsRepository _tgsRepository;
    private readonly ICloudflareClientService _cloudflareClient;

    public DeleteDnsZoneInfoCommandService(ITgsRepository tgsRepository, ICloudflareClientService cloudflareClient)
    {
        _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
        _cloudflareClient = cloudflareClient ?? throw new ArgumentNullException(nameof(cloudflareClient));
    }

    public async ValueTask ExecuteAsync(DeleteDnsZoneInfoCommand command, CancellationToken cancellationToken = default)
    {
        var agentDomains = await _tgsRepository
            .QueryAgentDomainsAsync(command.AgentId)
            .ToArrayAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (!agentDomains.Any())
            return;

        foreach (var domain in agentDomains)
        {
            var zone = await _cloudflareClient.ListZonesAsync(domain)
                .Where(z => z.DomainName == domain)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (zone is not null)
                await _cloudflareClient.DeleteZoneAsync(zone.Id).ConfigureAwait(false);
        }
    }
}
