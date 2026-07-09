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
}
