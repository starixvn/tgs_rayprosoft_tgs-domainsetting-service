
namespace Tgs.DomainSettings.Clients;

internal interface IDomainSettingClient
{
    ValueTask ApplyAgentDomainIngressAsync(Guid agentId);
    ValueTask CreateZoneAsync(Guid agentId);
    ValueTask DeleteAgentDomainIngressAsync(Guid agentId);
    ValueTask DeleteZoneAsync(Guid agentId);
}