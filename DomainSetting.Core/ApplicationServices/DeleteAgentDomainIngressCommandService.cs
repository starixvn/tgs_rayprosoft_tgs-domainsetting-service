using Tgs.ServiceUtility;

namespace Tgs.DomainSettings.ApplicationServices;

internal class DeleteAgentDomainIngressCommandService : ICommandService<DeleteAgentDomainIngressCommand>
{
    private readonly ITgsRepository _tgsRepository;
    private readonly IKubernetesClientService _kubernetesClientService;

    public DeleteAgentDomainIngressCommandService(ITgsRepository tgsRepository, IKubernetesClientService kubernetesClientService)
    {
        _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
        _kubernetesClientService = kubernetesClientService ?? throw new ArgumentNullException(nameof(kubernetesClientService));
    }

    public async ValueTask ExecuteAsync(DeleteAgentDomainIngressCommand command, CancellationToken cancellationToken = default)
    {
        var agent = await _tgsRepository
            .GetAgentByIdAsync(command.AgentId)
            .ConfigureAwait(false);

        if (agent is null)
            return;

        var ingressName = $"ing-agent-{agent.BrandCode.ToLower()}-{agent.Name.Trim().ToLower()}";

        await _kubernetesClientService.DeleteIngressAsync(ingressName).ConfigureAwait(false);
    }
}
