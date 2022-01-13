using Microsoft.AspNetCore.Mvc;
using Tgs.DomainSettings.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.AgentDomainSettingService.Controllers;

[Route("[controller]")]
[ApiController]
public class AgentController : ControllerBase
{
    [HttpPut("{agentId}", Name = "ApplyAgentDomainIngress")]
    public ValueTask ApplyAgentDomainIngressAsync(
        [FromServices] ICommandService<ApplyAgentDomainIngressCommand> applyIngressService,
        Guid agentId)
    {
        var command = new ApplyAgentDomainIngressCommand(agentId);

        return applyIngressService.ExecuteAsync(command);
    }

    [HttpDelete("{agentId}", Name = "DeleteAgentDomainIngress")]
    public ValueTask DeleteAgentDomainIngressAsync(
        [FromServices] ICommandService<DeleteAgentDomainIngressCommand> deleteIngressServicie,
        Guid agentId)
    {
        var command = new DeleteAgentDomainIngressCommand(agentId);

        return deleteIngressServicie.ExecuteAsync(command);
    }
}
