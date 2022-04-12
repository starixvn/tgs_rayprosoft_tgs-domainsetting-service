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
using Tgs.DomainSettings;
using Tgs.DomainSettings.ApplicationServices;
using Tgs.DomainSettings.Clients;
using Tgs.ServiceUtility;

namespace Tgs.DomainSettingService.IntegrationTests;

[TestClass]
public class CloudflareAgentZoneTests
{
    [TestMethod]
    public async Task Create_Agent_Zone()
    {
        var fakeCreateService = Substitute.For<ICommandService<CreateDnsZoneInfoCommand>>();
        var fakeCloudflareService = Substitute.For<ICloudflareClientService>();

        var web = CreateTestDomainSettingSite(services => services
                .AddSingleton(fakeCreateService)
                .AddSingleton(fakeCloudflareService));

        var httpClient = web.CreateClient();

        var sut = new DomainSettingClient(httpClient);

        var agentId = Guid.NewGuid();

        fakeCreateService
            .WhenForAnyArgs(fake => fake.ExecuteAsync(Arg.Any<CreateDnsZoneInfoCommand>()))
            .Do(Callback.Always(callInfo =>
            {
                var command = callInfo.Arg<CreateDnsZoneInfoCommand>();

                Assert.AreEqual(agentId, command.AgentId);
            }));

        await sut.CreateZoneAsync(agentId);

        _ = fakeCreateService.Received(1)
            .ExecuteAsync(Arg.Any<CreateDnsZoneInfoCommand>());
    }

    [TestMethod]
    public async Task Delete_Agent_Zone()
    {
        var fakeCreateService = Substitute.For<ICommandService<DeleteDnsZoneInfoCommand>>();
        var fakeCloudflareService = Substitute.For<ICloudflareClientService>();

        var web = CreateTestDomainSettingSite(services => services
            .AddSingleton(fakeCreateService)
            .AddSingleton(fakeCloudflareService));

        var httpClient = web.CreateClient();

        var sut = new DomainSettingClient(httpClient);

        var agentId = Guid.NewGuid();

        fakeCreateService
            .WhenForAnyArgs(fake => fake.ExecuteAsync(Arg.Any<DeleteDnsZoneInfoCommand>()))
            .Do(Callback.Always(callInfo =>
            {
                var command = callInfo.Arg<DeleteDnsZoneInfoCommand>();

                Assert.AreEqual(agentId, command.AgentId);
            }));

        await sut.DeleteZoneAsync(agentId);

        _ = fakeCreateService.Received(1)
            .ExecuteAsync(Arg.Any<DeleteDnsZoneInfoCommand>());
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
