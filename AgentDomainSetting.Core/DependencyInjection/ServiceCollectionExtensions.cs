using System.Reflection;
using Tgs.AgentDomainSettings;
using Tgs.AgentDomainSettings.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static AgentDomainConfiguration AddAgentDomainSetting(this IServiceCollection services)
    {
        services
            .RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
            .AddTransient<IKubernetesClientService, KubernetesClientService>();

        var configuration = new AgentDomainConfiguration(services);

        return configuration;
    }
}
