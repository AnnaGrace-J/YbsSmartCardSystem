using Microsoft.AspNetCore.Mvc;
using YbsSmartCardSystem.Domain.Features.TopUp;
using YbsSmartCardSystem.Domain.Features.TopUp.Models;

namespace YbsSmartCardSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TopUpController : BaseController
    {
        private readonly TopUpService _topUpService;

        public TopUpController(TopUpService topUpService)
        {
            _topUpService = topUpService;
        }

        [HttpPost]
        public IActionResult TopUpCreate([FromBody] TopUpCreateRequestModel request)
        {
            var result = _topUpService.Create(request);
            return Execute(result);
        }

        [HttpGet]
        public IActionResult TopUpList([FromQuery] TopUpListRequestModel request)
        {
            var result = _topUpService.GetList(request);
            return Execute(result);
        }
    }
}
