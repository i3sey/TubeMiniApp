using System.Text.Json;
using TubeMiniApp.API.Data;
using TubeMiniApp.API.Models;

namespace TubeMiniApp.API.Services;

public interface IDataImportService
{
    Task ImportInitialDataAsync();
    Task<int> ProcessPriceUpdatesAsync(string filePath);
    Task<int> ProcessStockUpdatesAsync(string filePath);
}

public class DataImportService : IDataImportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataImportService> _logger;
    private readonly string _dataPath;

    public DataImportService(
        ApplicationDbContext context,
        ILogger<DataImportService> logger,
        IWebHostEnvironment env)
    {
        _context = context;
        _logger = logger;
        _dataPath = Path.Combine(env.ContentRootPath, "testData");
    }

    public async Task ImportInitialDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting initial data import from testData folder...");

            // Проверяем есть ли уже данные
            if (_context.Products.Any())
            {
                _logger.LogInformation("Database already contains products. Skipping initial import.");
                return;
            }

            // 1. Импорт складов
            var warehouses = await ImportWarehousesAsync();
            _logger.LogInformation($"Imported {warehouses.Count} warehouses");

            // 2. Импорт номенклатуры (продукции)
            var products = await ImportNomenclatureAsync();
            _logger.LogInformation($"Imported {products.Count} products");

            // 3. Обновление цен
            await UpdatePricesAsync();
            _logger.LogInformation("Updated prices from prices.json");

            // 4. Обновление остатков
            await UpdateStocksAsync();
            _logger.LogInformation("Updated stocks from remnants.json");

            _logger.LogInformation("Initial data import completed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during initial data import");
            throw;
        }
    }

    private async Task<List<string>> ImportWarehousesAsync()
    {
        var filePath = Path.Combine(_dataPath, "stocks.json");
        if (!File.Exists(filePath))
        {
            _logger.LogWarning($"File not found: {filePath}");
            return new List<string>();
        }

        var json = await File.ReadAllTextAsync(filePath);
        var data = JsonSerializer.Deserialize<StocksImport>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (data?.ArrayOfStockEl == null) return new List<string>();

        var warehouses = data.ArrayOfStockEl
            .Select(s => s.StockName)
            .Distinct()
            .ToList();

        return warehouses;
    }

    private async Task<List<Product>> ImportNomenclatureAsync()
    {
        var filePath = Path.Combine(_dataPath, "nomenclature.json");
        if (!File.Exists(filePath))
        {
            _logger.LogWarning($"File not found: {filePath}");
            return new List<Product>();
        }

        var json = await File.ReadAllTextAsync(filePath);
        var data = JsonSerializer.Deserialize<NomenclatureImport>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (data?.ArrayOfNomenclatureEl == null) return new List<Product>();

        var products = new List<Product>();

        foreach (var item in data.ArrayOfNomenclatureEl.Where(n => n.Status == 1))
        {
            var product = new Product
            {
                SKU = $"PROD-{item.ID}",
                Warehouse = "Екатеринбург", // По умолчанию, обновим при импорте остатков
                ProductType = item.ProductionType,
                Diameter = (int)item.Diameter,
                WallThickness = item.PipeWallThickness,
                GOST = item.Gost,
                SteelGrade = item.SteelGrade,
                PricePerTon = 0, // Обновим из prices.json
                WeightPerMeter = 0, // Рассчитаем из фактических остатков в remnants.json
                AvailableStockTons = 0, // Обновим из remnants.json
                AvailableStockMeters = 0,
                LastPriceUpdate = DateTime.UtcNow
            };

            // Сохраним ID из исходных данных в дополнительное поле для связи
            product.Id = int.Parse(item.ID);

            products.Add(product);
        }

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        return products;
    }

    private async Task UpdatePricesAsync()
    {
        var filePath = Path.Combine(_dataPath, "prices.json");
        if (!File.Exists(filePath))
        {
            _logger.LogWarning($"File not found: {filePath}");
            return;
        }

        var json = await File.ReadAllTextAsync(filePath);
        var data = JsonSerializer.Deserialize<PricesImport>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (data?.ArrayOfPricesEl == null) return;

        foreach (var priceItem in data.ArrayOfPricesEl)
        {
            var productId = int.Parse(priceItem.ID);
            var product = await _context.Products.FindAsync(productId);

            if (product != null)
            {
                product.PricePerTon = priceItem.PriceT;
                product.LastPriceUpdate = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task UpdateStocksAsync()
    {
        var filePath = Path.Combine(_dataPath, "remnants.json");
        if (!File.Exists(filePath))
        {
            _logger.LogWarning($"File not found: {filePath}");
            return;
        }

        var json = await File.ReadAllTextAsync(filePath);
        var data = JsonSerializer.Deserialize<RemnantsImport>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (data?.ArrayOfRemnantsEl == null) return;

        // Загружаем склады для маппинга IDStock -> название
        var stocksFilePath = Path.Combine(_dataPath, "stocks.json");
        var stocksJson = await File.ReadAllTextAsync(stocksFilePath);
        var stocksData = JsonSerializer.Deserialize<StocksImport>(stocksJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var stockMapping = stocksData?.ArrayOfStockEl?.ToDictionary(s => s.IDStock, s => s.StockName)
            ?? new Dictionary<string, string>();

        foreach (var remnant in data.ArrayOfRemnantsEl)
        {
            var productId = int.Parse(remnant.ID);
            var product = await _context.Products.FindAsync(productId);

            if (product != null)
            {
                // Данные корректные, используем как есть
                product.AvailableStockMeters = remnant.InStockM;
                product.AvailableStockTons = remnant.InStockT;

                // Рассчитываем реальный вес метра из фактических данных остатков
                // Это более точно, чем Koef из номенклатуры
                if (remnant.InStockM > 0)
                {
                    product.WeightPerMeter = (remnant.InStockT / remnant.InStockM) * 1000; // кг/м
                }

                // Обновляем склад если есть маппинг
                if (stockMapping.TryGetValue(remnant.IDStock, out var warehouseName))
                {
                    product.Warehouse = warehouseName;
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> ProcessPriceUpdatesAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning($"Price update file not found: {filePath}");
                return 0;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var data = JsonSerializer.Deserialize<PricesImport>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data?.ArrayOfPricesEl == null) return 0;

            int updatedCount = 0;

            foreach (var priceItem in data.ArrayOfPricesEl)
            {
                var productId = int.Parse(priceItem.ID);
                var product = await _context.Products.FindAsync(productId);

                if (product != null)
                {
                    // Применяем дельту (изменение цены)
                    product.PricePerTon += priceItem.PriceT;
                    product.LastPriceUpdate = DateTime.UtcNow;
                    updatedCount++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Processed price updates: {updatedCount} products updated");
            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing price updates from {filePath}");
            throw;
        }
    }

    public async Task<int> ProcessStockUpdatesAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning($"Stock update file not found: {filePath}");
                return 0;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var data = JsonSerializer.Deserialize<RemnantsImport>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data?.ArrayOfRemnantsEl == null) return 0;

            int updatedCount = 0;

            foreach (var remnant in data.ArrayOfRemnantsEl)
            {
                var productId = int.Parse(remnant.ID);
                var product = await _context.Products.FindAsync(productId);

                if (product != null)
                {
                    // Данные корректные, используем как есть
                    product.AvailableStockMeters += remnant.InStockM;
                    product.AvailableStockTons += remnant.InStockT;
                    
                    // Не допускаем отрицательных остатков
                    if (product.AvailableStockTons < 0) product.AvailableStockTons = 0;
                    if (product.AvailableStockMeters < 0) product.AvailableStockMeters = 0;
                    
                    // Пересчитываем вес метра на основе актуальных остатков
                    if (product.AvailableStockMeters > 0)
                    {
                        product.WeightPerMeter = (product.AvailableStockTons / product.AvailableStockMeters) * 1000; // кг/м
                    }
                    
                    updatedCount++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Processed stock updates: {updatedCount} products updated");
            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing stock updates from {filePath}");
            throw;
        }
    }
}
