using Microsoft.AspNetCore.Mvc;

namespace TNetAPI.API.Controllers;

[Route("api/ssh")]
public class SshController : ControllerBase
{
    private readonly Services.SshService _sshService;

    public SshController(Services.SshService sshService)
    {
        _sshService = sshService;
    }

    [HttpPost("custom")]
    public IActionResult RunCustomCommand([FromBody] string command)
    {
        try
        {
            var result = _sshService.ExecuteCustomCommand(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }


    [HttpGet("systemStatus")]
    public IActionResult SystemStatus()
    {
        return RunCustomCommand("show system utilization");
    }

    [HttpGet("MacTable")]
    public IActionResult MacTable()
    {
        return RunCustomCommand("show mac-address-table");
    }
}