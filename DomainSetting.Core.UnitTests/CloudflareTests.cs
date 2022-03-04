using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tgs.DomainSettings;
using Tgs.DomainSettings.ApplicationServices;

namespace AgentDomainSetting.Core.UnitTests;

[TestClass]
public class CloudflareTests
{
    [TestMethod]
    public async Task Create_AgentZoneInfo()
    {
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakeCloudflareClient = Substitute.For<ICloudflareClientService>();
        var options = new CloudflareOptions();

        var sut = new CreateDnsZoneInfoCommandService(
            fakeTgsRepository,
            fakeCloudflareClient,
            options);

        var command = new CreateDnsZoneInfoCommand(Guid.NewGuid());

        _ = fakeTgsRepository.QueryAgentDomainsAsync(Arg.Is(command.AgentId))
            .Returns(new[] { "test.com" }.ToAsyncEnumerable());

        _ = fakeCloudflareClient.CreateZoneAsync(Arg.Is("test.com"))
            .Returns(new CloudflareDnsZone
            {
                Id = "testId",
                DomainName = "test.com",
                Status = "active"
            });

        await sut.ExecuteAsync(command);

        _ = fakeCloudflareClient.Received(1)
            .CreateZoneAsync(Arg.Is("test.com"));

        _ = fakeCloudflareClient.Received(2)
            .CreateCNameInZoneAsync(Arg.Is("testId"), Arg.Any<string>(), Arg.Any<string>());
        _ = fakeCloudflareClient.Received(1)
            .SetZoneAutomaticHttpsRewritesAsync(Arg.Is("testId"));
        _ = fakeCloudflareClient.Received(1)
            .SetZoneAlwaysUseHttpsAsync(Arg.Is("testId"));
        _ = fakeCloudflareClient.Received(1)
            .SetZoneResultMinifyAsync(Arg.Is("testId"));
        _ = fakeCloudflareClient.Received(1)
            .SetZoneBrotliAsync(Arg.Is("testId"));
    }

    [TestMethod]
    public async Task Delete_AgentZoneInfo()
    {
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakeCloudflareClient = Substitute.For<ICloudflareClientService>();

        var sut = new DeleteDnsZoneInfoCommandService(
            fakeTgsRepository,
            fakeCloudflareClient);

        var command = new DeleteDnsZoneInfoCommand(Guid.NewGuid());

        _ = fakeTgsRepository.QueryAgentDomainsAsync(Arg.Is(command.AgentId))
            .Returns(new[] { "test.com" }.ToAsyncEnumerable());

        _ = fakeCloudflareClient.ListZonesAsync(Arg.Is("test.com"))
            .Returns(new[] {
                new CloudflareDnsZone {
                    Id = "testId",
                    DomainName = "test.com",
                    Status = "active"
                }
            }.ToAsyncEnumerable());

        await sut.ExecuteAsync(command);

        _ = fakeCloudflareClient.Received(1)
            .DeleteZoneAsync(Arg.Is("testId"));
    }
}
