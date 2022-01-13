using System.Text;
using k8s;
using k8s.Models;
using Tgs.BrandCertificate.Clients;
using Tgs.DomainSettings.Properties;
using Tgs.ServiceUtility;

namespace Tgs.DomainSettings.ApplicationServices;

internal class ApplyAgentDomainIngressCommandService : ICommandService<ApplyAgentDomainIngressCommand>
{
    private readonly ITgsRepository _tgsRepository;
    private readonly IBrandCertificateClient _brandCertificateClient;
    private readonly IKubernetesClientService _kubernetesClientService;

    public ApplyAgentDomainIngressCommandService(
        ITgsRepository tgsRepository,
        IBrandCertificateClient brandCertificateClient,
        IKubernetesClientService kubernetesClientService)
    {
        _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
        _brandCertificateClient = brandCertificateClient ?? throw new ArgumentNullException(nameof(brandCertificateClient));
        _kubernetesClientService = kubernetesClientService ?? throw new ArgumentNullException(nameof(kubernetesClientService));
    }

    public async ValueTask ExecuteAsync(
        ApplyAgentDomainIngressCommand command,
        CancellationToken cancellationToken = default)
    {
        var agentDomains = await _tgsRepository
            .QueryAgentDomainsAsync(command.AgentId)
            .ToArrayAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (!agentDomains.Any())
            return;

        var agent = await _tgsRepository
            .GetAgentByIdAsync(command.AgentId)
            .ConfigureAwait(false);

        if (agent is null)
            return;

        var brandToken = await _brandCertificateClient
            .MakeJsonWebTokenAsync(agent.BrandId)
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(brandToken))
            return;

        using var templateStream = new MemoryStream(Resources.ingress);

        var ingress = await Yaml.LoadFromStreamAsync<V1Ingress>(templateStream).ConfigureAwait(false);

        ingress.Metadata.Name = $"ing-agent-{command.AgentId:D}";
        ingress.SetAnnotation(
            "nginx.ingress.kubernetes.io/configuration-snippet",
            BuildNginxConfigurationSnippet(brandToken, agent.InvitationCode));

        var ingressTls = new V1IngressTLS(new List<string>(), $"agent-{command.AgentId:D}");

        ingress.Spec.Tls = new List<V1IngressTLS> { ingressTls };
        ingress.Spec.Rules = new List<V1IngressRule>();

        foreach (var domainName in agentDomains)
        {
            var ingressRule = CreateIngressRule(domainName);

            ingress.Spec.Rules.Add(ingressRule);
            ingressTls.Hosts.Add(domainName);
        }

        var yaml = Yaml.SaveToString(ingress);
        await _kubernetesClientService.ApplyIngressAsync(ingress).ConfigureAwait(false);
    }

    private string BuildNginxConfigurationSnippet(string brandToken, string invitationCode)
    {
        var sb = new StringBuilder()
            .AppendLine($"proxy_set_header Brand-Token {brandToken};")
            .AppendLine($"proxy_set_header Agent-Invitation-Code {invitationCode};");

        return sb.ToString();
    }

    private static V1IngressRule CreateIngressRule(string domainName)
    {
        var ingressRule = new V1IngressRule(domainName, new V1HTTPIngressRuleValue(new List<V1HTTPIngressPath>()));

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

        return ingressRule;
    }
}
