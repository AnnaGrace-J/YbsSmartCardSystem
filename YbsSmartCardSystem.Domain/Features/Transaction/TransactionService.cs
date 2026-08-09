using YbsSmartCardSystem.Domain.Common;
using Microsoft.EntityFrameworkCore;
using YbsSmartCardSystem.Database.AppDbContextModels;
using YbsSmartCardSystem.Contracts.Features.Transaction;
using YbsSmartCardSystem.Infrastructure.AuditLog;
using YbsSmartCardSystem.Infrastructure.Services;
using YbsSmartCardSystem.Shared.Constants;

namespace YbsSmartCardSystem.Domain.Features.Transaction;

public class TransactionService
{
    private const decimal FixedFareAmount = 400m;
    private readonly AppDbContext _db;
    private readonly IAuditLogWriter _audit;
    private readonly ICurrentUserService _currentUser;

    public TransactionService(AppDbContext db, IAuditLogWriter audit, ICurrentUserService currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public Result<TransactionCreateResponseModel> Create(TransactionCreateRequestModel request)
    {
        try
        {
            if (request is null)
            {
                return Fail("Request data is required.", 400);
            }

            if (string.IsNullOrWhiteSpace(request.CardNum))
            {
                return Fail("Card number is required.", 400);
            }

            if (string.IsNullOrWhiteSpace(request.TerminalSerialNo))
            {
                return Fail("Terminal serial number is required.", 400);
            }

            var cardNum = request.CardNum.Trim();
            var terminalSerialNo = request.TerminalSerialNo.Trim();

            if (cardNum.Length > 50)
            {
                return Fail("Card number cannot exceed 50 characters.", 400);
            }

            if (terminalSerialNo.Length > 100)
            {
                return Fail("Terminal serial number cannot exceed 100 characters.", 400);
            }

            var card = _db.TblCards
                .FirstOrDefault(x => x.CardNum == cardNum && x.DeleteFlag == false);

            if (card is null)
            {
                return Fail("Card not found.", 404);
            }

            var terminal = _db.TblTerminals
                .AsNoTracking()
                .Include(x => x.Bus)
                .FirstOrDefault(x => x.TerminalSerialNo == terminalSerialNo && x.DeleteFlag == false);

            if (terminal is null)
            {
                return Fail("Terminal not found.", 404);
            }

            if (terminal.Bus.DeleteFlag)
            {
                return Fail("Bus not found.", 404);
            }

            if (!terminal.IsActive)
            {
                return Fail("Terminal is inactive.", 409);
            }

            if (card.Balance < FixedFareAmount)
            {
                return Fail("Insufficient balance.", 409);
            }

            using var tx = _db.Database.BeginTransaction();

            try
            {
                var transaction = new TblTransaction
                {
                    CardId = card.CardId,
                    TerminalId = terminal.TerminalId,
                    Amount = FixedFareAmount,
                    TransactionDate = DateTime.Now,
                    DeleteFlag = false
                };

                _db.TblTransactions.Add(transaction);

                card.Balance -= FixedFareAmount;
                card.UpdatedDate = DateTime.Now;

                _db.SaveChanges();
                tx.Commit();

                _ = _audit.WriteAsync(new AuditLogWriteModel
                {
                    UserId      = _currentUser.UserId,
                    Action      = AuditActions.BusTap,
                    FeatureName = "Transaction",
                    EntityName  = "TblTransaction",
                    EntityId    = transaction.TransactionId.ToString(),
                    NewValue    = new { transaction.TransactionNo, card.CardNum, terminal.TerminalSerialNo, transaction.Amount, RemainingBalance = card.Balance },
                    IpAddress   = _currentUser.IpAddress,
                    UserAgent   = _currentUser.UserAgent
                });

                return new Result<TransactionCreateResponseModel>
                {
                    IsSuccess = true,
                    StatusCode = 200,
                    Message = "Transaction completed successfully.",
                    Data = new TransactionCreateResponseModel
                    {
                        TransactionId = transaction.TransactionId,
                        TransactionNo = transaction.TransactionNo,
                        CardId = card.CardId,
                        CardNum = card.CardNum,
                        OwnerName = card.OwnerName,
                        TerminalId = terminal.TerminalId,
                        TerminalSerialNo = terminal.TerminalSerialNo,
                        Amount = transaction.Amount,
                        RemainingBalance = card.Balance,
                        TransactionDate = transaction.TransactionDate
                    }
                };
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
        catch (Exception)
        {
            return new Result<TransactionCreateResponseModel>
            {
                IsSuccess = false,
                StatusCode = 500,
                Message = "An unexpected error occurred."
            };
        }
    }

    public Result<TransactionListResponseModel> GetList(TransactionListRequestModel request)
    {
        try
        {
            request ??= new TransactionListRequestModel();

            if (request.PageNo <= 0)
            {
                return new Result<TransactionListResponseModel>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Page number must be greater than 0."
                };
            }

            if (request.PageSize <= 0)
            {
                return new Result<TransactionListResponseModel>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Page size must be greater than 0."
                };
            }

            if (request.PageSize > 100)
            {
                return new Result<TransactionListResponseModel>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Page size cannot exceed 100.",
                };
            }

            var query = _db.TblTransactions
                .AsNoTracking()
                .Include(x => x.Card)
                .Include(x => x.Terminal)
                    .ThenInclude(x => x.Bus)
                .Where(x => x.DeleteFlag == false
                    && x.Card.DeleteFlag == false
                    && x.Terminal.DeleteFlag == false
                    && x.Terminal.Bus.DeleteFlag == false);

            // Viewer users can only see transactions for their own card
            if (_currentUser.IsViewer)
            {
                var viewerPhone = _currentUser.PhoneNumber;
                if (string.IsNullOrEmpty(viewerPhone))
                {
                    return new Result<TransactionListResponseModel>
                    {
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = "No transactions found.",
                        Data = new TransactionListResponseModel { TotalCount = 0, Transactions = [] }
                    };
                }
                query = query.Where(x => x.Card.MobileNo == viewerPhone);
            }

            if (!string.IsNullOrWhiteSpace(request.CardNum))
            {
                var cardNum = request.CardNum.Trim();
                query = query.Where(x => x.Card.CardNum.Contains(cardNum));
            }

            if (!string.IsNullOrWhiteSpace(request.TerminalSerialNo))
            {
                var terminalSerialNo = request.TerminalSerialNo.Trim();
                query = query.Where(x => x.Terminal.TerminalSerialNo.Contains(terminalSerialNo));
            }

            var totalCount = query.Count();

            var transactions = query
                .OrderByDescending(x => x.TransactionDate)
                .ThenByDescending(x => x.TransactionId)
                .Skip((request.PageNo - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new TransactionModel
                {
                    TransactionId = x.TransactionId,
                    TransactionNo = x.TransactionNo,
                    CardId = x.CardId,
                    CardNum = x.Card.CardNum,
                    OwnerName = x.Card.OwnerName,
                    TerminalId = x.TerminalId,
                    TerminalSerialNo = x.Terminal.TerminalSerialNo,
                    BusNo = x.Terminal.Bus.BusNo,
                    BusLicense = x.Terminal.Bus.BusLicense,
                    Amount = x.Amount,
                    TransactionDate = x.TransactionDate
                })
                .ToList();

            return new Result<TransactionListResponseModel>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Transactions retrieved successfully.",
                Data = new TransactionListResponseModel
                {
                    Transactions = transactions,
                    TotalCount = totalCount
                }
            };
        }
        catch (Exception)
        {
            return new Result<TransactionListResponseModel>
            {
                IsSuccess = false,
                StatusCode = 500,
                Message = "An unexpected error occurred."
            };
        }
    }

    private static Result<TransactionCreateResponseModel> Fail(string message, int statusCode)
    {
        return new Result<TransactionCreateResponseModel>
        {
            IsSuccess = false,
            StatusCode = statusCode,
            Message = message
        };
    }
}
