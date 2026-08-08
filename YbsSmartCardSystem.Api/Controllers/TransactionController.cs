using Microsoft.AspNetCore.Mvc;
using YbsSmartCardSystem.Contracts.Features.Transaction;
using YbsSmartCardSystem.Domain.Features.Transaction;
using YbsSmartCardSystem.Infrastructure.Authorization.DynamicRbac;
using YbsSmartCardSystem.Shared.Constants;

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
    [RequirePermission(PermissionCodes.BusPaymentCreate)]
    public IActionResult TransactionCreate([FromBody] TransactionCreateRequestModel request)
    {
        var result = _transactionService.Create(request);
        return Execute(result);
    }

    [HttpGet]
    [RequirePermission(PermissionCodes.TransactionView)]
    public IActionResult TransactionList([FromQuery] TransactionListRequestModel request)
    {
        var result = _transactionService.GetList(request);
        return Execute(result);
    }
}
