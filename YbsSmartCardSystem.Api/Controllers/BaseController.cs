using YbsSmartCardSystem.Domain.Common;
using Microsoft.AspNetCore.Mvc;

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

            if (result.StatusCode < 400)
            {
                return BadRequest(result);
            }

            return StatusCode(result.StatusCode, result);
        }
    }
}
