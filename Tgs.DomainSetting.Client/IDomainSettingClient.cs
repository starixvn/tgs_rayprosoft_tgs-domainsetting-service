
namespace Tgs.DomainSettings.Clients;

public interface IDomainSettingClient
{
    ValueTask ApplyAgentDomainIngressAsync(Guid agentId);
    ValueTask CreateZoneAsync(Guid agentId);
    ValueTask DeleteAgentDomainIngressAsync(Guid agentId);
    ValueTask DeleteZoneAsync(Guid agentId);
}
