using Microsoft.AspNetCore.Mvc;
using Tgs.AgentDomainSettings.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.AgentDomainSettingService.Controllers;

[Route("cf")]
[ApiController]
public class CloudflareController : ControllerBase
{
    [HttpPut("{agentId}", Name = "CreateZone")]
    public ValueTask CreateZoneInfoAsync(
        [FromServices] ICommandService<CreateDnsZoneInfoCommand> createService,
        Guid agentId)
    {
        var command = new CreateDnsZoneInfoCommand(agentId);

        return createService.ExecuteAsync(command);
    }

    [HttpDelete("{agentId}", Name = "DeleteZone")]
    public ValueTask DeleteZoneInfoAsync(
        [FromServices] ICommandService<DeleteDnsZoneInfoCommand> deleteService,
        Guid agentId)
    {
        var command = new DeleteDnsZoneInfoCommand(agentId);

        return deleteService.ExecuteAsync(command);
    }
}
