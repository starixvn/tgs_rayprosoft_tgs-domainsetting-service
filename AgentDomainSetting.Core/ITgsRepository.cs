namespace Tgs.AgentDomainSettings;

public interface ITgsRepository
{
    IAsyncEnumerable<string> QueryAgentDomainsAsync(Guid agentId);

    ValueTask<Agent?> GetAgentByIdAsync(Guid agentId);
}
