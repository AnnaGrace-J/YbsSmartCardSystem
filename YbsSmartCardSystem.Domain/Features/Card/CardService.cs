using Microsoft.EntityFrameworkCore;
using YbsSmartCardSystem.Database.AppDbContextModels;
using YbsSmartCardSystem.Domain.Features.Card.Models;

namespace YbsSmartCardSystem.Domain.Features.Card;

public class CardService
{
    private readonly AppDbContext _db;

    public CardService(AppDbContext db)
    {
        _db = db;
    }

    public Result<CardListResponseModel> GetList(CardListRequestModel request)
    {
        try
        {
            var query = _db.TblCards
                .AsNoTracking()
                .Where(x => x.DeleteFlag == false);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(x =>
                    x.CardNum.Contains(search) ||
                    x.OwnerName.Contains(search));
            }

            var totalCount = query.Count();

            var cards = query
                .OrderByDescending(x => x.CardId)
                .Skip((request.PageNo - 1) * request.PageSize)
                .Take(request.PageSize)
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
                        CardId    = c.CardId,
                        CardNum   = c.CardNum,
                        OwnerName = c.OwnerName,
                        MobileNo  = c.MobileNo,
                        Balance   = c.Balance
                    }).ToList()
                }
            };
        }
        catch (Exception ex)
        {
            return new Result<CardListResponseModel>
            {
                IsSuccess = false,
                Message   = ex.ToString()
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
                    Message = "CardId is required."
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
                    Message = "Card not found."
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
        catch (Exception ex)
        {
            return new Result<CardModel>
            {
                IsSuccess = false,
                Message = ex.ToString()
            };
        }
    }


    public Result<CardCreateResponseModel> Create(CardCreateRequestModel request)
    {
        try
        {
            if (request is null)
            {
                return new Result<CardCreateResponseModel>
                {
                    IsSuccess = false,
                    Message = "Request data is required."
                };
            }

            if (string.IsNullOrWhiteSpace(request.CardNum))
            {
                return new Result<CardCreateResponseModel>
                {
                    IsSuccess = false,
                    Message = "Card number is required."
                };
            }

            if (string.IsNullOrWhiteSpace(request.OwnerName))
            {
                return new Result<CardCreateResponseModel>
                {
                    IsSuccess = false,
                    Message = "Owner name is required."
                };
            }

            if (!string.IsNullOrWhiteSpace(request.MobileNo) && request.MobileNo.Length > 20)
            {
                return new Result<CardCreateResponseModel>
                {
                    IsSuccess = false,
                    Message = "Mobile number cannot exceed 20 characters."
                };
            }

            var isDuplicateCardNum = _db.TblCards
                .AsNoTracking()
                .Any(x => x.CardNum == request.CardNum && x.DeleteFlag == false);

            if (isDuplicateCardNum)
            {
                return new Result<CardCreateResponseModel>
                {
                    IsSuccess = false,
                    Message = "Card number already exists."
                };
            }

            var card = new TblCard
            {
                CardNum = request.CardNum.Trim(),
                OwnerName = request.OwnerName.Trim(),
                MobileNo = string.IsNullOrWhiteSpace(request.MobileNo) ? null : request.MobileNo.Trim(),
                Balance = 0,
                CreatedDate = DateTime.Now,
                DeleteFlag = false
            };
            _db.TblCards.Add(card);
            _db.SaveChanges();

            return new Result<CardCreateResponseModel>
            {
                IsSuccess = true,
                Message = "Card Created Successfully",
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
            return new Result<CardCreateResponseModel>
            {
                IsSuccess = false,
                Message = ex.ToString()
            };
        }
    }

    public Result<CardModel> Patch(int id, CardPatchRequestModel request)
    {
        try
        {
            if (id <= 0)
            {
                return new Result<CardModel>
                {
                    IsSuccess = false,
                    Message = "CardId is required."
                };
            }

            if (request is null)
            {
                return new Result<CardModel>
                {
                    IsSuccess = false,
                    Message = "Request data is required."
                };
            }

            if (request.CardNum is null &&
                request.OwnerName is null &&
                request.MobileNo is null)
            {
                return new Result<CardModel>
                {
                    IsSuccess = false,
                    Message = "At least one field is required to update."
                };
            }

            if (request.CardNum is not null && string.IsNullOrWhiteSpace(request.CardNum))
            {
                return new Result<CardModel>
                {
                    IsSuccess = false,
                    Message = "Card number cannot be empty."
                };
            }

            if (request.OwnerName is not null && string.IsNullOrWhiteSpace(request.OwnerName))
            {
                return new Result<CardModel>
                {
                    IsSuccess = false,
                    Message = "Owner name cannot be empty."
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
                    Message = "Card not found."
                };
            }

            if (!string.IsNullOrWhiteSpace(request.CardNum))
            {
                var isDuplicateCardNum = _db.TblCards
                    .AsNoTracking()
                    .Any(x => x.CardNum == request.CardNum
                        && x.CardId != id
                        && x.DeleteFlag == false);

                if (isDuplicateCardNum)
                {
                    return new Result<CardModel>
                    {
                        IsSuccess = false,
                        Message = "Card number already exists."
                    };
                }

                item.CardNum = request.CardNum;
            }

            if (!string.IsNullOrWhiteSpace(request.OwnerName))
            {
                item.OwnerName = request.OwnerName;
            }

            if (request.MobileNo is not null)
            {
                item.MobileNo = request.MobileNo;
            }


            item.UpdatedDate = DateTime.Now;

            _db.Entry(item).State = EntityState.Modified;
            _db.SaveChanges();

            return new Result<CardModel>
            {
                IsSuccess = true,
                Message = "Card updated successfully.",
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
        catch (Exception ex)
        {
            return new Result<CardModel>
            {
                IsSuccess = false,
                Message = ex.ToString()
            };
        }
    }

    public Result<CardModel> Delete(int id)
    {
        try
        {
            if (id <= 0)
            {
                return new Result<CardModel>
                {
                    IsSuccess = false,
                    Message = "CardId is required."
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
                    Message = "Card not found."
                };
            }

            item.DeleteFlag = true;
            item.UpdatedDate = DateTime.Now;

            _db.Entry(item).State = EntityState.Modified;
            _db.SaveChanges();

            return new Result<CardModel>
            {
                IsSuccess = true,
                Message = "Card deleted successfully.",
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
        catch (Exception ex)
        {
            return new Result<CardModel>
            {
                IsSuccess = false,
                Message = ex.ToString()
            };
        }
    }

}
