namespace Tgs.AgentDomainSettings;

public class Agent
{
    public Guid Id { get; init; }

    public Guid BrandId { get; init; }

    public string InvitationCode { get; init; } = default!;
}
