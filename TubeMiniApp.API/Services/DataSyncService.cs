using Microsoft.EntityFrameworkCore;
using TubeMiniApp.API.Data;
using TubeMiniApp.API.Models;
using TubeMiniApp.API.DTOs;

namespace TubeMiniApp.API.Services;

/// <summary>
/// Сервис для синхронизации и обновления данных (задача со звездочкой)
/// </summary>
public interface IDataSyncService
{
    Task<int> SyncPricesAsync(PricesBatchUpdateDto dto);
    Task<int> SyncStocksAsync(StocksBatchUpdateDto dto);
    Task<int> UpdatePriceAsync(PriceUpdateDto dto);
    Task<int> UpdateStockAsync(StockUpdateDto dto);
    Task ProcessAllUpdatesAsync();
}

public class DataSyncService : IDataSyncService
{
    private readonly ApplicationDbContext _context;
    private readonly IDataImportService _importService;
    private readonly ILogger<DataSyncService> _logger;
    private readonly string _updatesPath;

    public DataSyncService(
        ApplicationDbContext context,
        IDataImportService importService,
        ILogger<DataSyncService> logger,
        IWebHostEnvironment env)
    {
        _context = context;
        _importService = importService;
        _logger = logger;
        _updatesPath = Path.Combine(env.ContentRootPath, "testData", "updates");
    }

    public async Task<int> SyncPricesAsync(PricesBatchUpdateDto dto)
    {
        int updatedCount = 0;

        foreach (var priceUpdate in dto.Prices)
        {
            var updated = await UpdatePriceAsync(priceUpdate);
            updatedCount += updated;
        }

        return updatedCount;
    }

    public async Task<int> SyncStocksAsync(StocksBatchUpdateDto dto)
    {
        int updatedCount = 0;

        foreach (var stockUpdate in dto.Stocks)
        {
            var updated = await UpdateStockAsync(stockUpdate);
            updatedCount += updated;
        }

        return updatedCount;
    }

    public async Task<int> UpdatePriceAsync(PriceUpdateDto dto)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.SKU == dto.SKU);

        if (product == null)
        {
            return 0;
        }

        product.PricePerTon = dto.PricePerTon;
        product.LastPriceUpdate = dto.Timestamp;

        await _context.SaveChangesAsync();
        return 1;
    }

    public async Task<int> UpdateStockAsync(StockUpdateDto dto)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.SKU == dto.SKU);

        if (product == null)
        {
            return 0;
        }

        // Применение дельты (изменения) к остаткам
        product.AvailableStockTons += dto.DeltaTons;
        product.AvailableStockMeters += dto.DeltaMeters;

        // Проверка на отрицательные значения
        if (product.AvailableStockTons < 0)
        {
            product.AvailableStockTons = 0;
        }

        if (product.AvailableStockMeters < 0)
        {
            product.AvailableStockMeters = 0;
        }

        await _context.SaveChangesAsync();
        return 1;
    }

    public async Task ProcessAllUpdatesAsync()
    {
        try
        {
            _logger.LogInformation("Processing all update files from testData/updates...");

            if (!Directory.Exists(_updatesPath))
            {
                _logger.LogWarning($"Updates directory not found: {_updatesPath}");
                return;
            }

            // Обработка обновлений цен
            var priceFiles = Directory.GetFiles(_updatesPath, "prices_update_*.json")
                .OrderBy(f => f)
                .ToList();

            foreach (var file in priceFiles)
            {
                _logger.LogInformation($"Processing price update: {Path.GetFileName(file)}");
                await _importService.ProcessPriceUpdatesAsync(file);
            }

            // Обработка обновлений остатков
            var stockFiles = Directory.GetFiles(_updatesPath, "remnants_update_*.json")
                .OrderBy(f => f)
                .ToList();

            foreach (var file in stockFiles)
            {
                _logger.LogInformation($"Processing stock update: {Path.GetFileName(file)}");
                await _importService.ProcessStockUpdatesAsync(file);
            }

            _logger.LogInformation("All updates processed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing update files");
            throw;
        }
    }
}
