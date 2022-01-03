using System.Reflection;
using k8s;
using Tgs.AgentDomainSettings;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentDomainSetting(
        this IServiceCollection services,
        Func<IServiceProvider, KubernetesClientConfiguration> k8sConfigFactory)
        => services
            .RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
            .AddTransient<IKubernetesClientService, KubernetesClientService>()
            .AddSingleton(k8sConfigFactory)
            .AddSingleton(sp => new Kubernetes(sp.GetRequiredService<KubernetesClientConfiguration>()));
}
