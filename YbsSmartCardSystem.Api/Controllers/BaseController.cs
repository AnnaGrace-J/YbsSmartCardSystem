using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using YbsSmartCardSystem.Domain;

namespace YbsSmartCardSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        [NonAction]
        public IActionResult Execute(object data)
        {
            string json = JsonConvert.SerializeObject(data);//object to json string
            Result<object> result = JsonConvert.DeserializeObject<Result<object>>(json)!;//json string to object
            if (result.IsSuccess)
            {
                return Ok(data);
            }
            else
            {
                return StatusCode(400, result);
            }

        }
    }
}
