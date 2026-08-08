using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YbsSmartCardSystem.Contracts.Features.Card;
using YbsSmartCardSystem.Domain.Features.Card;
using YbsSmartCardSystem.Infrastructure.Authorization.DynamicRbac;
using YbsSmartCardSystem.Shared.Constants;
using CardListRequestModel = YbsSmartCardSystem.Contracts.Features.Card.CardListRequestModel;

namespace YbsSmartCardSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardController : BaseController
    {
        private readonly CardService _cardService;

        public CardController(CardService cardService)
        {
            _cardService = cardService;
        }

        [HttpGet]
        [RequirePermission(PermissionCodes.CardView)]
        public IActionResult CardList([FromQuery] CardListRequestModel request)
        {
            var result = _cardService.GetList(request);
            return Execute(result);
        }

        [HttpGet("{id}")]
        [RequirePermission(PermissionCodes.CardView)]
        public IActionResult CardGetById(int id)
        {
            var result = _cardService.GetById(id);
            return Execute(result);
        }

        [HttpPost("Registration/SendOtp")]
        [RequirePermission(PermissionCodes.CardRegister)]
        public async Task<IActionResult> SendRegistrationOtp([FromBody] CardRegistrationSendOtpRequestModel request)
        {
            var result = await _cardService.SendRegistrationOtpAsync(request);
            return Execute(result);
        }

        [HttpPost("Registration/VerifyOtp")]
        [RequirePermission(PermissionCodes.CardRegister)]
        public async Task<IActionResult> VerifyRegistrationOtp([FromBody] CardRegistrationVerifyOtpRequestModel request)
        {
            var result = await _cardService.VerifyRegistrationOtpAsync(request);
            return Execute(result);
        }

        [HttpPost]
        [RequirePermission(PermissionCodes.CardRegister)]
        public async Task<IActionResult> CardCreate([FromBody] CardCreateRequestModel request)
        {
            var result = await _cardService.CreateAsync(request);
            return Execute(result);
        }

        [HttpPatch("{id}")]
        [RequirePermission(PermissionCodes.CardUpdate)]
        public IActionResult CardPatch(int id, [FromBody] CardPatchRequestModel request)
        {
            var result = _cardService.Patch(id, request);
            return Execute(result);
        }

        [HttpDelete("{id}")]
        [RequirePermission(PermissionCodes.CardDelete)]
        public IActionResult CardDelete(int id)
        {
            var result = _cardService.Delete(id);
            return Execute(result);
        }
    }
}
