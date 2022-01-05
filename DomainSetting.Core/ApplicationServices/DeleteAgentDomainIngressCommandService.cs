using Tgs.ServiceUtility;

namespace Tgs.DomainSettings.ApplicationServices;

internal class DeleteAgentDomainIngressCommandService : ICommandService<DeleteAgentDomainIngressCommand>
{
    private readonly IKubernetesClientService _kubernetesClientService;

    public DeleteAgentDomainIngressCommandService(IKubernetesClientService kubernetesClientService)
    {
        _kubernetesClientService = kubernetesClientService ?? throw new ArgumentNullException(nameof(kubernetesClientService));
    }

    public ValueTask ExecuteAsync(DeleteAgentDomainIngressCommand command, CancellationToken cancellationToken = default)
    {
        var ingressName = $"ing-agent-{command.AgentId:D}";

        return _kubernetesClientService.DeleteIngressAsync(ingressName);
    }
}
