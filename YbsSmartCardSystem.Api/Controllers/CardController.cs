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
            //if (result.IsSuccess)
            //{
            //    return Ok(result.Data);
            //}
            //return BadRequest(result.Message);
            return Execute(result);
        }
    }
}
