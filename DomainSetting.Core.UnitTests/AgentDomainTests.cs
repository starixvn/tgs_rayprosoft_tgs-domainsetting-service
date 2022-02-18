using k8s.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tgs.BrandCertificate.Clients;
using Tgs.DomainSettings;
using Tgs.DomainSettings.ApplicationServices;

namespace AgentDomainSetting.Core.UnitTests;

[TestClass]
public class AgentDomainTests
{
    [TestMethod]
    public async Task Apply_AgentDomain_Ingress_TestAsync()
    {
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakeBrandCertClient = Substitute.For<IBrandCertificateClient>();
        var fakeKubernetestClient = Substitute.For<IKubernetesClientService>();

        var sut = new ApplyAgentDomainIngressCommandService(
            fakeTgsRepository,
            fakeBrandCertClient,
            fakeKubernetestClient);

        var command = new ApplyAgentDomainIngressCommand(
            Guid.NewGuid());

        _ = fakeTgsRepository.QueryAgentDomainsAsync(Arg.Is(command.AgentId))
            .Returns(new[] { "test.com" }.ToAsyncEnumerable());

        _ = fakeTgsRepository.GetAgentByIdAsync(Arg.Is(command.AgentId))
            .Returns(new Agent
            {
                Id = command.AgentId,
                Name = "agent1",
                BrandId = Guid.NewGuid(),
                BrandCode = "test",
                InvitationCode = "invitation"
            });

        _ = fakeBrandCertClient.MakeJsonWebTokenAsync(Arg.Any<Guid>())
            .Returns("token");

        fakeKubernetestClient
            .WhenForAnyArgs(fake => fake.ApplyIngressAsync(Arg.Any<V1Ingress>()))
            .Do(callInfo =>
            {
                var ing = callInfo.Arg<V1Ingress>();

                Assert.IsNotNull(ing.Spec.Tls);
                Assert.IsNotNull(ing.Spec.Rules);

                Assert.AreEqual(1, ing.Spec.Tls.Count);
                Assert.AreEqual(1, ing.Spec.Tls[0].Hosts.Count);
                Assert.AreEqual("test.com", ing.Spec.Tls[0].Hosts[0]);

                Assert.AreEqual(1, ing.Spec.Rules.Count);
                Assert.AreEqual("test.com", ing.Spec.Rules[0].Host);
            });

        await sut.ExecuteAsync(command);

        _ = fakeKubernetestClient.Received(1)
            .ApplyIngressAsync(Arg.Is<V1Ingress>(ing => ValidateIngress(ing)));
    }

    [TestMethod]
    public async Task Delete_AgentDomain_Ingress_Test()
    {
        var fakeTgsRepository = Substitute.For<ITgsRepository>();
        var fakeKubernetesClient = Substitute.For<IKubernetesClientService>();

        var sut = new DeleteAgentDomainIngressCommandService(fakeTgsRepository, fakeKubernetesClient);

        var command = new DeleteAgentDomainIngressCommand(Guid.NewGuid());

        _ = fakeTgsRepository.GetAgentByIdAsync(Arg.Is(command.AgentId))
            .Returns(new Agent
            {
                Id = command.AgentId,
                Name = "agent1",
                BrandId = Guid.NewGuid(),
                BrandCode = "test",
                InvitationCode = "invitation"
            });

        var expectedIngressName = $"ing-agent-test-agent1";

        await sut.ExecuteAsync(command);

        _ = fakeKubernetesClient.Received(1)
            .DeleteIngressAsync(Arg.Is(expectedIngressName));
    }

    private static bool ValidateIngress(V1Ingress ing)
    {
        try
        {
            ing.Validate();

            return true;
        }
        catch
        {
            return false;
        }
    }
}
