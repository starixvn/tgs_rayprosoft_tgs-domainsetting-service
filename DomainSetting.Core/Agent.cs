namespace Tgs.DomainSettings;

public class Agent
{
    public Guid Id { get; init; }

    public string Name { get; init; } = default!;

    public Guid BrandId { get; init; }

    public string BrandCode { get; init; } = default!;

    public string InvitationCode { get; init; } = default!;
}
