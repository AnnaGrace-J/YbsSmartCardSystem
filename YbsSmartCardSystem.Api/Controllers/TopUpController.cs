using Microsoft.AspNetCore.Mvc;
using YbsSmartCardSystem.Contracts.Features.TopUp;
using YbsSmartCardSystem.Domain.Features.TopUp;
using YbsSmartCardSystem.Infrastructure.Authorization.DynamicRbac;
using YbsSmartCardSystem.Shared.Constants;

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
        [RequirePermission(PermissionCodes.TopUpCreate)]
        public IActionResult TopUpCreate([FromBody] TopUpCreateRequestModel request)
        {
            var result = _topUpService.Create(request);
            return Execute(result);
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.TopUpView)]
        public IActionResult TopUpList([FromQuery] TopUpListRequestModel request)
        {
            var result = _topUpService.GetList(request);
            return Execute(result);
        }
    }
}
