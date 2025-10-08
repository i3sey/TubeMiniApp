using Microsoft.EntityFrameworkCore;
using TubeMiniApp.API.Data;
using TubeMiniApp.API.Models;
using TubeMiniApp.API.DTOs;

namespace TubeMiniApp.API.Services;

/// <summary>
/// Сервис для работы с продукцией
/// </summary>
public interface IProductService
{
    Task<(List<Product> Products, int TotalCount)> GetFilteredProductsAsync(ProductFilterDto filter);
    Task<Product?> GetProductByIdAsync(int id);
    Task<List<string>> GetWarehousesAsync();
    Task<List<string>> GetProductTypesAsync();
    Task<List<string>> GetGOSTsAsync();
    Task<List<string>> GetSteelGradesAsync();
}

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Product> Products, int TotalCount)> GetFilteredProductsAsync(ProductFilterDto filter)
    {
        var query = _context.Products.AsQueryable();

        // Применение фильтров
        if (!string.IsNullOrWhiteSpace(filter.Warehouse))
        {
            query = query.Where(p => p.Warehouse.Contains(filter.Warehouse));
        }

        if (!string.IsNullOrWhiteSpace(filter.ProductType))
        {
            query = query.Where(p => p.ProductType.Contains(filter.ProductType));
        }

        if (filter.DiameterMin.HasValue)
        {
            query = query.Where(p => p.Diameter >= filter.DiameterMin.Value);
        }

        if (filter.DiameterMax.HasValue)
        {
            query = query.Where(p => p.Diameter <= filter.DiameterMax.Value);
        }

        if (filter.WallThicknessMin.HasValue)
        {
            query = query.Where(p => p.WallThickness >= filter.WallThicknessMin.Value);
        }

        if (filter.WallThicknessMax.HasValue)
        {
            query = query.Where(p => p.WallThickness <= filter.WallThicknessMax.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.GOST))
        {
            query = query.Where(p => p.GOST.Contains(filter.GOST));
        }

        if (!string.IsNullOrWhiteSpace(filter.SteelGrade))
        {
            query = query.Where(p => p.SteelGrade.Contains(filter.SteelGrade));
        }

        // Только товары с остатками
        query = query.Where(p => p.AvailableStockTons > 0);

        var totalCount = await query.CountAsync();

        var products = await query
            .OrderBy(p => p.Warehouse)
            .ThenBy(p => p.ProductType)
            .ThenBy(p => p.Diameter)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (products, totalCount);
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<List<string>> GetWarehousesAsync()
    {
        return await _context.Products
            .Select(p => p.Warehouse)
            .Distinct()
            .OrderBy(w => w)
            .ToListAsync();
    }

    public async Task<List<string>> GetProductTypesAsync()
    {
        return await _context.Products
            .Select(p => p.ProductType)
            .Distinct()
            .OrderBy(pt => pt)
            .ToListAsync();
    }

    public async Task<List<string>> GetGOSTsAsync()
    {
        return await _context.Products
            .Select(p => p.GOST)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync();
    }

    public async Task<List<string>> GetSteelGradesAsync()
    {
        return await _context.Products
            .Select(p => p.SteelGrade)
            .Distinct()
            .OrderBy(sg => sg)
            .ToListAsync();
    }
}
