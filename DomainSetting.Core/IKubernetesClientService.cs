using k8s.Models;

namespace Tgs.DomainSettings;

public interface IKubernetesClientService
{
    ValueTask ApplyIngressAsync(V1Ingress ingress);

    ValueTask DeleteIngressAsync(string ingressName);

    ValueTask<V1Ingress> GetIngressAsync(string ingressName);
}
