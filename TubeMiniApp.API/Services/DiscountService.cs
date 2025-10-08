using Microsoft.EntityFrameworkCore;
using TubeMiniApp.API.Data;
using TubeMiniApp.API.Models;

namespace TubeMiniApp.API.Services;

/// <summary>
/// Сервис для работы со скидками
/// </summary>
public interface IDiscountService
{
    Task<decimal> GetDiscountForItemAsync(decimal quantityTons, string productType, string warehouse);
    Task<List<Discount>> GetActiveDiscountsAsync();
}

public class DiscountService : IDiscountService
{
    private readonly ApplicationDbContext _context;

    public DiscountService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetDiscountForItemAsync(decimal quantityTons, string productType, string warehouse)
    {
        var applicableDiscounts = await _context.Discounts
            .Where(d => d.IsActive && d.MinQuantityTons <= quantityTons)
            .Where(d => (d.ProductType == null || d.ProductType == productType) &&
                       (d.Warehouse == null || d.Warehouse == warehouse))
            .OrderByDescending(d => d.DiscountPercent)
            .ToListAsync();

        return applicableDiscounts.FirstOrDefault()?.DiscountPercent ?? 0;
    }

    public async Task<List<Discount>> GetActiveDiscountsAsync()
    {
        return await _context.Discounts
            .Where(d => d.IsActive)
            .OrderBy(d => d.MinQuantityTons)
            .ToListAsync();
    }
}
