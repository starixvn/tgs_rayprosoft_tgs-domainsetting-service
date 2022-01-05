using Microsoft.EntityFrameworkCore;
using Tgs.AgentDomainSettings.Repositories;
using Tgs.AgentDomainSettings.Repositories.Entities;
using Tgs.DomainSettings;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentDomainSettingRepositories(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> dbContextConfigure)
        => services
            .AddDbContext<TgsContext>(dbContextConfigure)
            .AddTransient<ITgsRepository, TgsRepository>();
}
