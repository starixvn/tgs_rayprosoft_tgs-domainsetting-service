namespace Tgs.DomainSettings;

public interface ITgsRepository
{
    IAsyncEnumerable<string> QueryAgentDomainsAsync(Guid agentId);

    ValueTask<Agent?> GetAgentByIdAsync(Guid agentId);
}
