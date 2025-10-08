using Microsoft.AspNetCore.Mvc;
using TubeMiniApp.API.Services;
using TubeMiniApp.API.DTOs;

namespace TubeMiniApp.API.Controllers;

/// <summary>
/// Контроллер для синхронизации данных (задача со звездочкой)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly IDataSyncService _dataSyncService;

    public SyncController(IDataSyncService dataSyncService)
    {
        _dataSyncService = dataSyncService;
    }

    /// <summary>
    /// Синхронизация цен (пакетное обновление)
    /// </summary>
    [HttpPost("prices")]
    public async Task<IActionResult> SyncPrices([FromBody] PricesBatchUpdateDto dto)
    {
        var updatedCount = await _dataSyncService.SyncPricesAsync(dto);
        return Ok(new
        {
            message = "Цены обновлены",
            updatedCount,
            totalSent = dto.Prices.Count
        });
    }

    /// <summary>
    /// Синхронизация остатков (пакетное обновление дельт)
    /// </summary>
    [HttpPost("stocks")]
    public async Task<IActionResult> SyncStocks([FromBody] StocksBatchUpdateDto dto)
    {
        var updatedCount = await _dataSyncService.SyncStocksAsync(dto);
        return Ok(new
        {
            message = "Остатки обновлены",
            updatedCount,
            totalSent = dto.Stocks.Count
        });
    }

    /// <summary>
    /// Обновление цены одного товара
    /// </summary>
    [HttpPost("price")]
    public async Task<IActionResult> UpdatePrice([FromBody] PriceUpdateDto dto)
    {
        var updated = await _dataSyncService.UpdatePriceAsync(dto);

        if (updated == 0)
        {
            return NotFound(new { message = $"Продукт с SKU '{dto.SKU}' не найден" });
        }

        return Ok(new { message = "Цена обновлена", sku = dto.SKU });
    }

    /// <summary>
    /// Обновление остатка одного товара
    /// </summary>
    [HttpPost("stock")]
    public async Task<IActionResult> UpdateStock([FromBody] StockUpdateDto dto)
    {
        var updated = await _dataSyncService.UpdateStockAsync(dto);

        if (updated == 0)
        {
            return NotFound(new { message = $"Продукт с SKU '{dto.SKU}' не найден" });
        }

        return Ok(new { message = "Остаток обновлен", sku = dto.SKU });
    }

    /// <summary>
    /// Обработать все файлы обновлений из папки testData/updates
    /// </summary>
    [HttpPost("process-updates")]
    public async Task<IActionResult> ProcessAllUpdates()
    {
        try
        {
            await _dataSyncService.ProcessAllUpdatesAsync();
            return Ok(new
            {
                message = "Все файлы обновлений обработаны успешно",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Ошибка при обработке обновлений",
                error = ex.Message
            });
        }
    }
}
