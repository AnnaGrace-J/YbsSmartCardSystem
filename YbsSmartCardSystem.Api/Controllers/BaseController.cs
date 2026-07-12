using Microsoft.AspNetCore.Mvc;
using YbsSmartCardSystem.Domain;

namespace YbsSmartCardSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        [NonAction]
        public IActionResult Execute<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return StatusCode(result.StatusCode, result);
        }
    }
}
