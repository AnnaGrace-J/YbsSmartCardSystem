using YbsSmartCardSystem.Domain.Common;
using Microsoft.EntityFrameworkCore;
using YbsSmartCardSystem.Database.AppDbContextModels;
using YbsSmartCardSystem.Contracts.Features.TopUp;

namespace YbsSmartCardSystem.Domain.Features.TopUp;

public class TopUpService
{
    private readonly AppDbContext _db;

    public TopUpService(AppDbContext db)
    {
        _db = db;
    }

    public Result<TopUpCreateResponseModel> Create(TopUpCreateRequestModel request)
    {
        try
        {
            // --- Validation ---
            if (request is null)
            {
                return new Result<TopUpCreateResponseModel>
                {
                    IsSuccess = false,
                    Message   = "Request data is required.",
                    StatusCode = 400
                };
            }

            if (request.CardId <= 0)
            {
                return new Result<TopUpCreateResponseModel>
                {
                    IsSuccess = false,
                    Message   = "Card is required.",
                    StatusCode = 400
                };
            }

            if (request.Amount < 1000)
            {
                return new Result<TopUpCreateResponseModel>
                {
                    IsSuccess = false,
                    Message   = "Minimum top-up amount is 1,000 MMK."
                };
            }

            if (request.Amount > 100000)
            {
                return new Result<TopUpCreateResponseModel>
                {
                    IsSuccess = false,
                    Message   = "Maximum top-up amount is 100,000 MMK."
                };
            }

            if (!string.IsNullOrWhiteSpace(request.Remark) && request.Remark.Length > 250)
            {
                return new Result<TopUpCreateResponseModel>
                {
                    IsSuccess = false,
                    Message   = "Remark cannot exceed 250 characters.",
                    StatusCode = 400
                };
            }

            var card = _db.TblCards
                .FirstOrDefault(x => x.CardId == request.CardId && x.DeleteFlag == false);

            if (card is null)
            {
                return new Result<TopUpCreateResponseModel>
                {
                    IsSuccess = false,
                    Message   = "Card not found.",
                    StatusCode = 404
                };
            }

            // --- Atomic write ---
            using var tx = _db.Database.BeginTransaction();
            try
            {
                var topUp = new TblTopUp
                {
                    CardId     = request.CardId,
                    Amount     = request.Amount,
                    TopUpDate  = DateTime.Now,
                    Remark     = string.IsNullOrWhiteSpace(request.Remark) ? null : request.Remark.Trim(),
                    DeleteFlag = false
                };
                _db.TblTopUps.Add(topUp);

                card.Balance    += request.Amount;
                card.UpdatedDate = DateTime.Now;
                _db.SaveChanges();
                tx.Commit();

                return new Result<TopUpCreateResponseModel>
                {
                    IsSuccess = true,
                    Message   = "Top-up successful.",
                    Data      = new TopUpCreateResponseModel
                    {
                        TopUpId    = topUp.TopUpId,
                        TopUpNo    = topUp.TopUpNo,
                        CardId     = card.CardId,
                        CardNum    = card.CardNum,
                        OwnerName  = card.OwnerName,
                        Amount     = topUp.Amount,
                        NewBalance = card.Balance,
                        TopUpDate  = topUp.TopUpDate,
                        Remark     = topUp.Remark
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
            return new Result<TopUpCreateResponseModel>
            {
                IsSuccess = false,
                Message = "An unexpected error occurred.",
                StatusCode = 500
                // TODO: Log exception after Infrastructure logging is added.
            };
        }
    }

    public Result<TopUpListResponseModel> GetList(TopUpListRequestModel request)
    {
        try
        {
            var query = _db.TblTopUps
                .AsNoTracking()
                .Where(x => x.DeleteFlag == false);

            if (request.CardId > 0)
            {
                query = query.Where(x => x.CardId == request.CardId);
            }

            var totalCount = query.Count();

            var topUps = query
                .OrderByDescending(x => x.TopUpDate)
                .Skip((request.PageNo - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => new TopUpModel
                {
                    TopUpId   = t.TopUpId,
                    TopUpNo   = t.TopUpNo,
                    CardId    = t.CardId,
                    CardNum   = t.Card.CardNum,
                    OwnerName = t.Card.OwnerName,
                    Amount    = t.Amount,
                    TopUpDate = t.TopUpDate,
                    Remark    = t.Remark
                })
                .ToList();

            return new Result<TopUpListResponseModel>
            {
                IsSuccess = true,
                Message   = "Top-ups retrieved successfully.",
                Data      = new TopUpListResponseModel
                {
                    TotalCount = totalCount,
                    TopUps     = topUps
                }
            };
        }
        catch (Exception)
        {
            return new Result<TopUpListResponseModel>
            {
                IsSuccess = false,
                Message = "An unexpected error occurred.",
                StatusCode = 500
                // TODO: Log exception after Infrastructure logging is added.
            };
        }
    }
}
