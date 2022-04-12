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

        var brandIngress = await _kubernetesClientService
            .GetIngressAsync($"lobby-{agent.BrandCode.ToLower()}")
            .ConfigureAwait(false);

        using var templateStream = new MemoryStream(Resources.ingress);

        var ingress = await KubernetesYaml.LoadFromStreamAsync<V1Ingress>(templateStream).ConfigureAwait(false);

        ingress.Metadata.Name = $"ing-agent-{agent.BrandCode.ToLower()}-{agent.Name.Trim().ToLower()}";
        ingress.SetAnnotation(
            "nginx.ingress.kubernetes.io/configuration-snippet",
            BuildNginxConfigurationSnippet(brandToken, agent.InvitationCode));
        ingress.SetLabel("tgs.brandcode", agent.BrandCode);
        ingress.SetLabel("tgs.invitation", agent.InvitationCode);
        ingress.SetLabel("tgs.agent.username", agent.Name);

        var ingressTls = new V1IngressTLS(new List<string>(), $"agent-{agent.BrandCode.ToLower()}-{agent.Name.Trim().ToLower()}");

        ingress.Spec.Tls = new List<V1IngressTLS> { ingressTls };
        ingress.Spec.Rules = new List<V1IngressRule>();

        foreach (var domainName in agentDomains)
        {
            ingress.Spec.Rules.Add(new V1IngressRule(
                domainName,
                new V1HTTPIngressRuleValue(brandIngress.Spec.Rules[0].Http.Paths)));
            ingress.Spec.Rules.Add(new V1IngressRule(
                $"www.{domainName}",
                new V1HTTPIngressRuleValue(brandIngress.Spec.Rules[0].Http.Paths)));
            ingressTls.Hosts.Add(domainName);
            ingressTls.Hosts.Add($"www.{domainName}");
        }

        await _kubernetesClientService.ApplyIngressAsync(ingress).ConfigureAwait(false);
    }

    private static string BuildNginxConfigurationSnippet(string brandToken, string invitationCode)
    {
        var sb = new StringBuilder()
            .AppendLine($"proxy_set_header Brand-Token {brandToken};")
            .AppendLine($"proxy_set_header Agent-Invitation-Code {invitationCode};");

        return sb.ToString();
    }
}
