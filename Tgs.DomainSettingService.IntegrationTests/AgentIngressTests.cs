using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using OpenTelemetry.Exporter;
using Tgs.AgentDomainSettings.Repositories.Entities;
using Tgs.DomainSettings.ApplicationServices;
using Tgs.DomainSettings.Clients;
using Tgs.ServiceUtility;

namespace Tgs.DomainSettingService.IntegrationTests;

[TestClass]
public class AgentIngressTests
{
    [TestMethod]
    public async Task Apply_Agent_Ingress()
    {
        var fakeApplyService = Substitute.For<ICommandService<ApplyAgentDomainIngressCommand>>();

        var web = CreateTestDomainSettingSite(services => services.AddSingleton(fakeApplyService));

        var httpClient = web.CreateClient();

        var sut = new DomainSettingClient(httpClient);

        var agentId = Guid.NewGuid();

        fakeApplyService
            .WhenForAnyArgs(fake => fake.ExecuteAsync(Arg.Any<ApplyAgentDomainIngressCommand>()))
            .Do(Callback.Always(callInfo =>
            {
                var command = callInfo.Arg<ApplyAgentDomainIngressCommand>();

                Assert.AreEqual(agentId, command.AgentId);
            }));

        await sut.ApplyAgentDomainIngressAsync(agentId);

        _ = fakeApplyService.Received(1)
            .ExecuteAsync(Arg.Any<ApplyAgentDomainIngressCommand>());
    }

    [TestMethod]
    public async Task Delete_Agent_Ingress()
    {
        var fakeDeleteService = Substitute.For<ICommandService<DeleteAgentDomainIngressCommand>>();

        var web = CreateTestDomainSettingSite(services => services.AddSingleton(fakeDeleteService));

        var httpClient = web.CreateClient();

        var sut = new DomainSettingClient(httpClient);

        var agentId = Guid.NewGuid();

        fakeDeleteService
            .WhenForAnyArgs(fake => fake.ExecuteAsync(Arg.Any<DeleteAgentDomainIngressCommand>()))
            .Do(Callback.Always(callInfo =>
            {
                var command = callInfo.Arg<DeleteAgentDomainIngressCommand>();

                Assert.AreEqual(agentId, command.AgentId);
            }));

        await sut.DeleteAgentDomainIngressAsync(agentId);

        _ = fakeDeleteService.Received(1)
            .ExecuteAsync(Arg.Any<DeleteAgentDomainIngressCommand>());
    }

    private static WebApplicationFactory<Program> CreateTestDomainSettingSite(Action<IServiceCollection>? configureServices = null)
        => new WebApplicationFactory<Program>()
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureAppConfiguration((_, configBuilder) => configBuilder.Sources.Clear())
                .ConfigureLogging((_, logging) => logging
                    .ClearProviders()
                    .AddConsole())
                .ConfigureServices(services =>
                {
                    _ = services
                        .AddSingleton(sp =>
                        {
                            var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
                            _ = dbContextOptionsBuilder
                                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                .ConfigureWarnings(x => x.Ignore(
                                    InMemoryEventId.TransactionIgnoredWarning));

                            return new TgsContext(dbContextOptionsBuilder.Options);
                        });

                    _ = services
                        .AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
                            sp => new ConfigureNamedOptions<OtlpExporterOptions>(
                                Options.DefaultName,
                                exporterOptions => exporterOptions.Endpoint = null));

                    configureServices?.Invoke(services);
                }));
}
