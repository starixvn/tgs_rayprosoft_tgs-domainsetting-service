using k8s.Models;

namespace Tgs.AgentDomainSettings;

public interface IKubernetesClientService
{
    ValueTask ApplyIngressAsync(V1Ingress ingress);

    ValueTask DeleteIngressAsync(string ingressName);
}
