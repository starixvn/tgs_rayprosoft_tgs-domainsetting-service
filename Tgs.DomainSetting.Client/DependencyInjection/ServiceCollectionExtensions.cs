using Microsoft.Extensions.Options;
using Tgs.DomainSettings.Clients;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainSettingClient(
        IServiceCollection services,
        Action<DomainSettingClientOptions, IServiceProvider> configure)
        => services
            .AddOptions<DomainSettingClientOptions>()
            .Configure(configure)
            .Services
            .AddTransient(sp => sp.GetRequiredService<IOptions<DomainSettingClientOptions>>().Value)
            .AddHttpClient<IDomainSettingClient, DomainSettingClient>()
            .ConfigureHttpClient((sp, httpClient) =>
            {
                var options = sp.GetRequiredService<DomainSettingClientOptions>();

                httpClient.BaseAddress = options.DomainSettingBaseAddress;
            })
            .Services;
}
