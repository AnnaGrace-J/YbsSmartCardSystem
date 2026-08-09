using YbsSmartCardSystem.Domain.Common;
using Microsoft.EntityFrameworkCore;
using YbsSmartCardSystem.Database.AppDbContextModels;
using YbsSmartCardSystem.Contracts.Features.TopUp;
using YbsSmartCardSystem.Infrastructure.AuditLog;
using YbsSmartCardSystem.Infrastructure.Services;
using YbsSmartCardSystem.Shared.Constants;

namespace YbsSmartCardSystem.Domain.Features.TopUp;

public class TopUpService
{
    private readonly AppDbContext _db;
    private readonly IAuditLogWriter _audit;
    private readonly ICurrentUserService _currentUser;

    public TopUpService(AppDbContext db, IAuditLogWriter audit, ICurrentUserService currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
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
                    DeleteFlag = false,
                    CreatedBy  = _currentUser.UserId
                };
                _db.TblTopUps.Add(topUp);

                card.Balance    += request.Amount;
                card.UpdatedDate = DateTime.Now;
                _db.SaveChanges();
                tx.Commit();

                _ = _audit.WriteAsync(new AuditLogWriteModel
                {
                    UserId      = _currentUser.UserId,
                    Action      = AuditActions.TopUp,
                    FeatureName = "TopUp",
                    EntityName  = "TblTopUp",
                    EntityId    = topUp.TopUpId.ToString(),
                    NewValue    = new { topUp.TopUpNo, request.CardId, topUp.Amount, NewBalance = card.Balance },
                    IpAddress   = _currentUser.IpAddress,
                    UserAgent   = _currentUser.UserAgent
                });

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

            // Viewer users can only see top-ups for their own card
            if (_currentUser.IsViewer)
            {
                var viewerPhone = _currentUser.PhoneNumber;
                if (string.IsNullOrEmpty(viewerPhone))
                {
                    return new Result<TopUpListResponseModel>
                    {
                        IsSuccess = true,
                        Message = "No top-ups found.",
                        Data = new TopUpListResponseModel { TotalCount = 0, TopUps = [] }
                    };
                }
                query = query.Where(x => x.Card.MobileNo == viewerPhone);
            }

            if (request.CardId > 0)
            {
                query = query.Where(x => x.CardId == request.CardId);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(x => 
                    x.Card.CardNum.Contains(search) || 
                    x.Card.OwnerName.Contains(search));
            }

            if (request.FilterDate.HasValue)
            {
                var filterDate = request.FilterDate.Value.Date;
                query = query.Where(x => x.TopUpDate.Date == filterDate);
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
                    Remark    = t.Remark,
                    CreatedByName = t.CreatedUser != null ? t.CreatedUser.UserName : null,
                    CreatedByRole = t.CreatedUser != null ? _db.TblUserRoles.Where(ur => ur.UserId == t.CreatedBy && !ur.DeleteFlag).Select(ur => ur.Role.RoleName).FirstOrDefault() : null
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
