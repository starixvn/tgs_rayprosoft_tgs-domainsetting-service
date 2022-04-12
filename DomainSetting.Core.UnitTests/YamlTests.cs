using k8s;
using k8s.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tgs.DomainSettings.Properties;

namespace AgentDomainSetting.Core.UnitTests;

[TestClass]
public class YamlTests
{
    [TestMethod]
    public async Task Create_Ingress_Test()
    {
        using var templateStream = new MemoryStream(Resources.ingress);

        var sut = await KubernetesYaml.LoadFromStreamAsync<V1Ingress>(templateStream);

        var agentId = Guid.NewGuid();
        var brandToken = "abcde";

        sut.Metadata.Name = $"ing-agent-{agentId:D}";
        sut.SetAnnotation(
            "nginx.ingress.kubernetes.io/configuration-snippet",
            $"\nproxy_set_header Brand-Token {brandToken};");

        var ingressTls = new V1IngressTLS(new List<string>(), $"agent-{agentId:D}");

        ingressTls.Hosts.Add("test.com");

        sut.Spec.Tls = new[] { ingressTls }.ToList();

        sut.Spec.Rules = new List<V1IngressRule>();

        var ingressRule = new V1IngressRule("test.com", new V1HTTPIngressRuleValue(new List<V1HTTPIngressPath>()));

        var brandCommunicationPath = new V1HTTPIngressPath(
            new V1IngressBackend(service: new V1IngressServiceBackend("brand-api", new V1ServiceBackendPort(number: 80))),
            "Prefix",
            "/api/brandcommunication");
        ingressRule.Http.Paths.Add(brandCommunicationPath);

        var brandLiveChatPath = new V1HTTPIngressPath(
            new V1IngressBackend(service: new V1IngressServiceBackend("brand-api", new V1ServiceBackendPort(number: 80))),
            "Prefix",
            "/api/brandlivechat");
        ingressRule.Http.Paths.Add(brandLiveChatPath);

        var opApiPath = new V1HTTPIngressPath(
            new V1IngressBackend(service: new V1IngressServiceBackend("operator-api", new V1ServiceBackendPort(number: 80))),
            "Prefix",
            "/api");
        ingressRule.Http.Paths.Add(opApiPath);

        var lobbyPath = new V1HTTPIngressPath(
            new V1IngressBackend(service: new V1IngressServiceBackend("lobby-t1", new V1ServiceBackendPort(number: 80))),
            "Prefix",
            "/");
        ingressRule.Http.Paths.Add(lobbyPath);

        sut.Spec.Rules.Add(ingressRule);

        var serializer = new YamlDotNet.Serialization.Serializer();
        var actual = serializer.Serialize(sut);

        sut.Validate();
    }
}
