using YbsSmartCardSystem.Domain.Common;
using Microsoft.EntityFrameworkCore;
using YbsSmartCardSystem.Database.AppDbContextModels;
using YbsSmartCardSystem.Contracts.Features.Card;
using YbsSmartCardSystem.Infrastructure.AuditLog;
using YbsSmartCardSystem.Infrastructure.Services;
using YbsSmartCardSystem.Shared.Constants;

namespace YbsSmartCardSystem.Domain.Features.Card;

public class CardService
{
    private readonly AppDbContext _db;
    private readonly IAuditLogWriter _audit;
    private readonly ICurrentUserService _currentUser;
    private readonly IOtpService _otpService;
    private readonly CardNumberGenerator _cardNumberGenerator;

    public CardService(
        AppDbContext db, 
        IAuditLogWriter audit, 
        ICurrentUserService currentUser,
        IOtpService otpService,
        CardNumberGenerator cardNumberGenerator)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
        _otpService = otpService;
        _cardNumberGenerator = cardNumberGenerator;
    }

    public Result<CardListResponseModel> GetList(CardListRequestModel request)
    {
        try
        {
            var query = _db.TblCards
                .AsNoTracking()
                .Include(x => x.CreatedUser)
                .AsQueryable();

            // Viewer users can only see their own card
            if (_currentUser.IsViewer)
            {
                var viewerPhone = _currentUser.PhoneNumber;
                if (string.IsNullOrEmpty(viewerPhone))
                {
                    return new Result<CardListResponseModel>
                    {
                        IsSuccess = true,
                        Message = "No card found.",
                        Data = new CardListResponseModel { TotalCount = 0, Cards = [] }
                    };
                }
                query = query.Where(x => x.MobileNo == viewerPhone);
            }

            if (request.IsDeleted.HasValue)
            {
                query = query.Where(x => x.DeleteFlag == request.IsDeleted.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(x =>
                    x.CardNum.Contains(search) ||
                    x.OwnerName.Contains(search));
            }

            if (request.FilterDate.HasValue)
            {
                var dateStr = request.FilterDate.Value.ToString("ddMMyyyy");
                query = query.Where(x => x.CardNum.Contains(dateStr));
            }

            var totalCount = query.Count();

            if (request.PageNo < 1) request.PageNo = 1;
            if (request.PageSize < 1) request.PageSize = 10;
            if (request.PageSize > 100) request.PageSize = 100;

            var cards = query
                .OrderByDescending(x => x.CardId)
                .Skip((request.PageNo - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new
                {
                    Card = c,
                    CreatorName = c.CreatedUser != null ? c.CreatedUser.UserName : null,
                    CreatorRole = c.CreatedUser != null ? _db.TblUserRoles.Where(ur => ur.UserId == c.CreatedBy && !ur.DeleteFlag).Select(ur => ur.Role.RoleName).FirstOrDefault() : null
                })
                .ToList();

            return new Result<CardListResponseModel>
            {
                IsSuccess = true,
                Message   = "Card retrieved successfully.",
                Data      = new CardListResponseModel
                {
                    TotalCount = totalCount,
                    Cards      = cards.Select(c => new CardModel
                    {
                        CardId    = c.Card.CardId,
                        CardNum   = c.Card.CardNum,
                        OwnerName = c.Card.OwnerName,
                        MobileNo  = c.Card.MobileNo,
                        Balance   = c.Card.Balance,
                        CreatedByName = c.CreatorName,
                        CreatedByRole = c.CreatorRole,
                        DeleteFlag = c.Card.DeleteFlag
                    }).ToList()
                }
            };
        }
        catch (Exception)
        {
            return new Result<CardListResponseModel>
            {
                IsSuccess = false,
                Message = "An unexpected error occurred.",
                StatusCode = 500
            };
        }
    }

    public Result<CardModel> GetById(int id)
    {
        try
        {
            if (id <= 0)
            {
                return new Result<CardModel>
                {
                    IsSuccess = false,
                    Message = "CardId is required.",
                    StatusCode = 400
                };
            }

            var item = _db.TblCards
                .AsNoTracking()
                .FirstOrDefault(x => x.CardId == id && x.DeleteFlag == false);

            if (item is null)
            {
                return new Result<CardModel>
                {
                    IsSuccess = false,
                    Message = "Card not found.",
                    StatusCode = 404
                };
            }

            return new Result<CardModel>
            {
                IsSuccess = true,
                Message = "Card retrieved successfully.",
                Data = new CardModel
                {
                    CardId = item.CardId,
                    CardNum = item.CardNum,
                    OwnerName = item.OwnerName,
                    MobileNo = item.MobileNo,
                    Balance = item.Balance
                }
            };
        }
        catch (Exception)
        {
            return new Result<CardModel>
            {
                IsSuccess = false,
                Message = "An unexpected error occurred.",
                StatusCode = 500
            };
        }
    }

    public async Task<Result<CardRegistrationSendOtpResponseModel>> SendRegistrationOtpAsync(CardRegistrationSendOtpRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            return new Result<CardRegistrationSendOtpResponseModel> { IsSuccess = false, Message = "Phone number is required.", StatusCode = 400 };

        // Check if an active card already exists for this phone number
        var existingCard = await _db.TblCards
            .AnyAsync(x => x.MobileNo == request.PhoneNumber && !x.DeleteFlag);

        if (existingCard)
        {
            return new Result<CardRegistrationSendOtpResponseModel>
            {
                IsSuccess = false,
                Message = "A card already exists for this phone number.",
                StatusCode = 409
            };
        }

        var result = await _otpService.SendOtpAsync(request.PhoneNumber, "CardRegistration");
        
        return new Result<CardRegistrationSendOtpResponseModel>
        {
            IsSuccess = true,
            Message = "OTP sent successfully.",
            Data = new CardRegistrationSendOtpResponseModel
            {
                PhoneNumber = result.PhoneNumber,
                ExpiresAt = result.ExpiresAt
            }
        };
    }

    public async Task<Result<CardRegistrationVerifyOtpResponseModel>> VerifyRegistrationOtpAsync(CardRegistrationVerifyOtpRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber) || string.IsNullOrWhiteSpace(request.OtpCode))
            return new Result<CardRegistrationVerifyOtpResponseModel> { IsSuccess = false, Message = "Phone number and OTP code are required.", StatusCode = 400 };

        var isVerified = await _otpService.VerifyOtpAsync(request.PhoneNumber, request.OtpCode, "CardRegistration");

        if (!isVerified)
            return new Result<CardRegistrationVerifyOtpResponseModel> { IsSuccess = false, Message = "Invalid or expired OTP.", StatusCode = 400 };

        return new Result<CardRegistrationVerifyOtpResponseModel>
        {
            IsSuccess = true,
            Message = "OTP verified successfully.",
            Data = new CardRegistrationVerifyOtpResponseModel
            {
                PhoneNumber = request.PhoneNumber,
                IsVerified = true
            }
        };
    }

    public async Task<Result<CardCreateResponseModel>> CreateAsync(CardCreateRequestModel request)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            if (request is null)
                return new Result<CardCreateResponseModel> { IsSuccess = false, Message = "Request data is required.", StatusCode = 400 };
            if (string.IsNullOrWhiteSpace(request.OwnerName))
                return new Result<CardCreateResponseModel> { IsSuccess = false, Message = "Owner name is required.", StatusCode = 400 };
            if (string.IsNullOrWhiteSpace(request.MobileNo))
                return new Result<CardCreateResponseModel> { IsSuccess = false, Message = "Mobile number is required.", StatusCode = 400 };
            if (request.MobileNo.Length > 20)
                return new Result<CardCreateResponseModel> { IsSuccess = false, Message = "Mobile number cannot exceed 20 characters.", StatusCode = 400 };

            // Check OTP verification
            var verifiedOtp = await _db.TblCardRegistrationOtps
                .Where(x => x.PhoneNumber == request.MobileNo && x.Purpose == "CardRegistration" && !x.DeleteFlag && x.VerifiedAt != null)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync();

            if (verifiedOtp == null)
            {
                return new Result<CardCreateResponseModel> { IsSuccess = false, Message = "Phone number is not verified.", StatusCode = 400 };
            }

            // Check if card already exists for this phone number
            var existingCard = await _db.TblCards
                .Where(x => x.MobileNo == request.MobileNo && !x.DeleteFlag)
                .FirstOrDefaultAsync();

            if (existingCard != null)
            {
                return new Result<CardCreateResponseModel> { IsSuccess = false, Message = "A card already exists for this phone number.", StatusCode = 409 };
            }

            var cardNum = await _cardNumberGenerator.GenerateCardNumberAsync();

            var card = new TblCard
            {
                CardNum = cardNum,
                OwnerName = request.OwnerName.Trim(),
                MobileNo = request.MobileNo.Trim(),
                Balance = 0,
                CreatedDate = DateTime.Now,
                DeleteFlag = false,
                CreatedBy = _currentUser.UserId
            };
            _db.TblCards.Add(card);
            
            verifiedOtp.DeleteFlag = true; // Consume OTP
            
            await _db.SaveChangesAsync();

            _ = _audit.WriteAsync(new AuditLogWriteModel
            {
                UserId      = _currentUser.UserId,
                Action      = AuditActions.CreateCard,
                FeatureName = "Card",
                EntityName  = "TblCard",
                EntityId    = card.CardId.ToString(),
                NewValue    = new { card.CardNum, card.OwnerName, card.MobileNo },
                IpAddress   = _currentUser.IpAddress,
                UserAgent   = _currentUser.UserAgent
            });

            await transaction.CommitAsync();

            return new Result<CardCreateResponseModel>
            {
                IsSuccess = true,
                Message = "Card Created Successfully",
                StatusCode = 201,
                Data = new CardCreateResponseModel
                {
                    CardId = card.CardId,
                    CardNum = card.CardNum,
                    OwnerName = card.OwnerName,
                    MobileNo = card.MobileNo
                }
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            if (ex.Message.Contains("Maximum card number limit reached"))
            {
                return new Result<CardCreateResponseModel> { IsSuccess = false, Message = ex.Message, StatusCode = 400 };
            }
            return new Result<CardCreateResponseModel> { IsSuccess = false, Message = "An unexpected error occurred.", StatusCode = 500 };
        }
    }

    public Result<CardModel> Patch(int id, CardPatchRequestModel request)
    {
        try
        {
            if (id <= 0)
                return new Result<CardModel> { IsSuccess = false, Message = "CardId is required.", StatusCode = 400 };
            if (request is null)
                return new Result<CardModel> { IsSuccess = false, Message = "Request data is required.", StatusCode = 400 };
            if (request.CardNum is null && request.OwnerName is null && request.MobileNo is null)
                return new Result<CardModel> { IsSuccess = false, Message = "At least one field is required to update.", StatusCode = 400 };
            if (request.CardNum is not null && string.IsNullOrWhiteSpace(request.CardNum))
                return new Result<CardModel> { IsSuccess = false, Message = "Card number cannot be empty." };
            if (request.OwnerName is not null && string.IsNullOrWhiteSpace(request.OwnerName))
                return new Result<CardModel> { IsSuccess = false, Message = "Owner name cannot be empty." };

            var item = _db.TblCards.FirstOrDefault(x => x.CardId == id && x.DeleteFlag == false);
            if (item is null)
                return new Result<CardModel> { IsSuccess = false, Message = "Card not found.", StatusCode = 404 };

            var oldValue = new { item.CardNum, item.OwnerName, item.MobileNo };

            if (!string.IsNullOrWhiteSpace(request.CardNum))
            {
                var isDuplicateCardNum = _db.TblCards.AsNoTracking()
                    .Any(x => x.CardNum == request.CardNum && x.CardId != id && x.DeleteFlag == false);
                if (isDuplicateCardNum)
                    return new Result<CardModel> { IsSuccess = false, Message = "Card number already exists.", StatusCode = 409 };
                item.CardNum = request.CardNum;
            }
            if (!string.IsNullOrWhiteSpace(request.OwnerName))
                item.OwnerName = request.OwnerName;
            if (request.MobileNo is not null)
            {
                if (string.IsNullOrWhiteSpace(request.MobileNo))
                {
                    return new Result<CardModel> { IsSuccess = false, Message = "Mobile number cannot be empty.", StatusCode = 400 };
                }

                var isDuplicatePhone = _db.TblCards.AsNoTracking()
                    .Any(x => x.MobileNo == request.MobileNo && x.CardId != id && x.DeleteFlag == false);
                if (isDuplicatePhone)
                    return new Result<CardModel> { IsSuccess = false, Message = "A card already exists for this phone number.", StatusCode = 409 };

                item.MobileNo = request.MobileNo;
            }

            item.UpdatedDate = DateTime.Now;
            _db.SaveChanges();

            _ = _audit.WriteAsync(new AuditLogWriteModel
            {
                UserId      = _currentUser.UserId,
                Action      = AuditActions.UpdateCard,
                FeatureName = "Card",
                EntityName  = "TblCard",
                EntityId    = item.CardId.ToString(),
                OldValue    = oldValue,
                NewValue    = new { item.CardNum, item.OwnerName, item.MobileNo },
                IpAddress   = _currentUser.IpAddress,
                UserAgent   = _currentUser.UserAgent
            });

            return new Result<CardModel>
            {
                IsSuccess = true,
                Message = "Card updated successfully.",
                Data = new CardModel { CardId = item.CardId, CardNum = item.CardNum, OwnerName = item.OwnerName, MobileNo = item.MobileNo, Balance = item.Balance }
            };
        }
        catch (Exception)
        {
            return new Result<CardModel> { IsSuccess = false, Message = "An unexpected error occurred.", StatusCode = 500 };
        }
    }

    public Result<CardModel> Delete(int id)
    {
        try
        {
            if (id <= 0)
                return new Result<CardModel> { IsSuccess = false, Message = "CardId is required.", StatusCode = 400 };

            var item = _db.TblCards.FirstOrDefault(x => x.CardId == id && x.DeleteFlag == false);
            if (item is null)
                return new Result<CardModel> { IsSuccess = false, Message = "Card not found.", StatusCode = 404 };

            item.DeleteFlag = true;
            item.UpdatedDate = DateTime.Now;
            _db.SaveChanges();

            _ = _audit.WriteAsync(new AuditLogWriteModel
            {
                UserId      = _currentUser.UserId,
                Action      = AuditActions.DeleteCard,
                FeatureName = "Card",
                EntityName  = "TblCard",
                EntityId    = item.CardId.ToString(),
                OldValue    = new { item.CardNum, item.OwnerName },
                IpAddress   = _currentUser.IpAddress,
                UserAgent   = _currentUser.UserAgent
            });

            return new Result<CardModel>
            {
                IsSuccess = true,
                Message = "Card deleted successfully.",
                Data = new CardModel { CardId = item.CardId, CardNum = item.CardNum, OwnerName = item.OwnerName, MobileNo = item.MobileNo, Balance = item.Balance }
            };
        }
        catch (Exception)
        {
            return new Result<CardModel> { IsSuccess = false, Message = "An unexpected error occurred.", StatusCode = 500 };
        }
    }

    public Result<CardModel?> GetMyCard()
    {
        try
        {
            var phone = _currentUser.PhoneNumber;
            if (string.IsNullOrEmpty(phone))
            {
                return new Result<CardModel?> { IsSuccess = true, Data = null, Message = "User has no phone number registered." };
            }

            var card = _db.TblCards
                .AsNoTracking()
                .FirstOrDefault(x => x.MobileNo == phone && x.DeleteFlag == false);

            if (card is null)
            {
                return new Result<CardModel?> { IsSuccess = true, Data = null, Message = "No card registered for this phone number." };
            }

            return new Result<CardModel?>
            {
                IsSuccess = true,
                Data = new CardModel
                {
                    CardId = card.CardId,
                    CardNum = card.CardNum,
                    OwnerName = card.OwnerName,
                    MobileNo = card.MobileNo,
                    Balance = card.Balance,
                    DeleteFlag = card.DeleteFlag
                }
            };
        }
        catch (Exception ex)
        {
            return new Result<CardModel?> { IsSuccess = false, Message = ex.Message, StatusCode = 500 };
        }
    }
}
