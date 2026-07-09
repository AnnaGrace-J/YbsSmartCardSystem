using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YbsSmartCardSystem.Database.AppDbContextModels;
using YbsSmartCardSystem.Domain.Features.Card;
using YbsSmartCardSystem.Domain.Features.Card.Models;
using CardListRequestModel = YbsSmartCardSystem.Domain.Features.Card.Models.CardListRequestModel;

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
        public IActionResult CardList([FromQuery] CardListRequestModel request)
        {
            var result = _cardService.GetList(request);
            return Execute(result);
        }
        [HttpPost]
        public IActionResult CardCreate([FromBody] CardCreateRequestModel request)
        {
            var result = _cardService.Create(request);
            return Execute(result);
        }
        [HttpPut("{id}")]
        public IActionResult CardUpdate(int id, [FromBody] CardUpdateRequestModel request)
        {
            var result = _cardService.Update(id, request);
            return Execute(result);
        }
        [HttpPatch("{id}")]
        public IActionResult CardPatch(int id, [FromBody] CardPatchRequestModel request)
        {
            var result = _cardService.Patch(id, request);
            return Execute(result);
        }
        [HttpDelete("{id}")]
        public IActionResult CardDelete(int id)
        {
            var result = _cardService.Delete(id);
            return Execute(result);
        }
    }
}
