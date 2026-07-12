using Microsoft.AspNetCore.Mvc;
using YbsSmartCardSystem.Domain.Features.Transaction;
using YbsSmartCardSystem.Domain.Features.Transaction.Models;

namespace YbsSmartCardSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransactionController : BaseController
{
    private readonly TransactionService _transactionService;

    public TransactionController(TransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost]
    public IActionResult TransactionCreate([FromBody] TransactionCreateRequestModel request)
    {
        var result = _transactionService.Create(request);
        return Execute(result);
    }

    [HttpGet]
    public IActionResult TransactionList([FromQuery] TransactionListRequestModel request)
    {
        var result = _transactionService.GetList(request);
        return Execute(result);
    }
}
