using Azure.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var cards = _db.TblCards
                .AsNoTracking()
                .Where(x => x.DeleteFlag == false)
                .OrderByDescending(x=>x.CardId)
                .Skip((request.PageNo - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            Result<CardListResponseModel> result = new Result<CardListResponseModel>
            {
                IsSuccess = true,
                Message = "Card retrieved successfully.",
                Data = new CardListResponseModel
                {
                    Cards = cards.Select(c => new CardModel
                    {
                        CardId = c.CardId,
                        CardNum = c.CardNum,
                        OwnerName = c.OwnerName,
                        MobileNo = c.MobileNo,
                        Balance = c.Balance
                    }).ToList()
                }
            };
            return result;
        }
        catch (Exception ex)
        {
            Result<CardListResponseModel> result = new Result<CardListResponseModel>
            {
                IsSuccess = false,
                Message = ex.ToString()
            };
            return result;//400
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
            var card = new TblCard
            {
                CardNum = request.CardNum,
                OwnerName = request.OwnerName,
                MobileNo = request.MobileNo,
                CreatedDate = DateTime.Now,
                DeleteFlag = false
            };
            _db.TblCards.Add(card);
            _db.SaveChanges();
            Result<CardCreateResponseModel> result = new Result<CardCreateResponseModel>
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
            return result;
        }
        catch(Exception ex)
        {
            Result<CardCreateResponseModel> result = new Result<CardCreateResponseModel>
            {
                IsSuccess = false,
                Message = ex.ToString()
            };
            return result;
        }
    }

    public Result<CardModel> Update(int id, CardUpdateRequestModel request)
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

            if (string.IsNullOrWhiteSpace(request.CardNum))
            {
                return new Result<CardModel>
                {
                    IsSuccess = false,
                    Message = "Card number is required."
                };
            }

            if (string.IsNullOrWhiteSpace(request.OwnerName))
            {
                return new Result<CardModel>
                {
                    IsSuccess = false,
                    Message = "Owner name is required."
                };
            }

            if (request.Balance < 0)
            {
                return new Result<CardModel>
                {
                    IsSuccess = false,
                    Message = "Balance cannot be negative."
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
            item.OwnerName = request.OwnerName;
            item.MobileNo = request.MobileNo;
            item.Balance = request.Balance;
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
                request.MobileNo is null &&
                request.Balance is null)
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

            if (request.Balance is not null && request.Balance < 0)
            {
                return new Result<CardModel>
                {
                    IsSuccess = false,
                    Message = "Balance cannot be negative."
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

            if (request.Balance is not null)
            {
                item.Balance = request.Balance.Value;
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
