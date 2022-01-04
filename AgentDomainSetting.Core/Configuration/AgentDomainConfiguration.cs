using System.Net.Http.Headers;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Tgs.AgentDomainSettings.Configuration;

public class AgentDomainConfiguration
{
    public IServiceCollection Services { get; }

    public AgentDomainConfiguration(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public AgentDomainConfiguration ConfigureKubernetesClientConfiguration(
        Func<IServiceProvider, KubernetesClientConfiguration> k8sConfigFactory,
        Func<IServiceProvider, DelegatingHandler[]>? registerHttpClientHandlerFactory = null)
    {
        Services
            .AddSingleton(k8sConfigFactory)
            .AddSingleton(sp => new Kubernetes(
                sp.GetRequiredService<KubernetesClientConfiguration>(),
                (registerHttpClientHandlerFactory ?? new Func<IServiceProvider, DelegatingHandler[]>(_ => Array.Empty<DelegatingHandler>()))(sp)));

        return this;
    }

    public AgentDomainConfiguration ConfigureCloudflareOptions(Action<CloudflareOptions, IServiceProvider> cloudflareConfigure)
    {
        Services
            .AddTransient(sp => sp.GetRequiredService<IOptions<CloudflareOptions>>().Value)
            .AddOptions<CloudflareOptions>()
            .Configure(cloudflareConfigure);

        return this;
    }

    public AgentDomainConfiguration ConfigureCloudflareClient(Action<IHttpClientBuilder> httpClientConfigure)
    {
        httpClientConfigure(Services
            .AddHttpClient<ICloudflareClientService, CloudflareClientService>()
            .ConfigureHttpClient((sp, httpClient) =>
            {
                var options = sp.GetRequiredService<CloudflareOptions>();

                httpClient.BaseAddress = options.CloudflareBaseAddress;

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Token);
            }));

        return this;
    }
}
