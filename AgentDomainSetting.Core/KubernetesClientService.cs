using k8s;
using k8s.Models;

namespace Tgs.AgentDomainSettings;

internal class KubernetesClientService : IKubernetesClientService
{
    private readonly Kubernetes _kubernetesClient;
    private readonly KubernetesClientConfiguration _kubernetesConfig;

    public KubernetesClientService(
        Kubernetes kubernetesClient,
        KubernetesClientConfiguration kubernetesConfig)
    {
        _kubernetesClient = kubernetesClient ?? throw new ArgumentNullException(nameof(kubernetesClient));
        _kubernetesConfig = kubernetesConfig ?? throw new ArgumentNullException(nameof(kubernetesConfig));
    }

    public async ValueTask ApplyIngressAsync(V1Ingress ingress)
    {
        var deployedIngresses = await _kubernetesClient.ListNamespacedIngressAsync(
            _kubernetesConfig.Namespace,
            fieldSelector: $"metadata.name={ingress.Name()}").ConfigureAwait(false);

        if (deployedIngresses.Items.Count() > 0)
            await _kubernetesClient
                .ReplaceNamespacedIngressAsync(ingress, ingress.Name(), _kubernetesConfig.Namespace)
                .ConfigureAwait(false);
        else
            await _kubernetesClient
                .CreateNamespacedIngressAsync(ingress, _kubernetesConfig.Namespace)
                .ConfigureAwait(false);
    }

    public async ValueTask DeleteIngressAsync(string ingressName)
        => await _kubernetesClient
            .DeleteNamespacedIngressAsync(ingressName, _kubernetesConfig.Namespace)
            .ConfigureAwait(false);
}
