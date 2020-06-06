using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TestSingalR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AboutController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Get() => Ok(new { data = "This is About Details v1" });
    }
}