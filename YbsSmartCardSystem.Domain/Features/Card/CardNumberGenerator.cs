using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YbsSmartCardSystem.Database.AppDbContextModels;

namespace YbsSmartCardSystem.Domain.Features.Card;

public class CardNumberGenerator
{
    private readonly AppDbContext _db;

    public CardNumberGenerator(AppDbContext db)
    {
        _db = db;
    }

    public async Task<string> GenerateCardNumberAsync()
    {
        var prefix = "crd";
        var datePart = DateTime.Now.ToString("ddMMyyyy");
        var prefixWithDate = prefix + datePart;

        // Find highest sequence for today
        var lastCard = await _db.TblCards
            .Where(x => x.CardNum.StartsWith(prefixWithDate))
            .OrderByDescending(x => x.CardNum)
            .FirstOrDefaultAsync();

        int nextSequence = 1;
        if (lastCard != null)
        {
            var lastSequenceString = lastCard.CardNum.Substring(prefixWithDate.Length);
            if (int.TryParse(lastSequenceString, out int lastSequence))
            {
                nextSequence = lastSequence + 1;
            }
        }

        if (nextSequence > 9999)
        {
            throw new Exception("Maximum card number limit reached for today.");
        }

        var sequencePart = nextSequence.ToString("D4");
        return prefixWithDate + sequencePart;
    }
}
