using Microsoft.AspNetCore.Mvc;

namespace IoT.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SensorsController(ILogger<SensorsController> logger) : ControllerBase
    {
        private readonly ILogger<SensorsController> _logger = logger;
    }
}
