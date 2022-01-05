using Tgs.AgentDomainSettings.Repositories.Entities;
using Tgs.DomainSettings;

namespace Tgs.AgentDomainSettings.Repositories;

internal class TgsRepository : ITgsRepository
{
    private readonly TgsContext _dbContext;

    public TgsRepository(TgsContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public IAsyncEnumerable<string> QueryAgentDomainsAsync(Guid agentId)
        => _dbContext.AgentDomains.AsQueryable()
            .Where(ad => ad.AgentId == agentId)
            .Select(ad => ad.DomainName)
            .ToAsyncEnumerable();

    public async ValueTask<Agent?> GetAgentByIdAsync(Guid agentId)
    {
        var row = await _dbContext.AgentRows
            .FindAsync(agentId)
            .ConfigureAwait(false);

        return row is null
            ? null
            : new Agent
            {
                Id = row.Id,
                BrandId = row.BrandId,
                InvitationCode = row.InvitationCode
            };
    }
}
