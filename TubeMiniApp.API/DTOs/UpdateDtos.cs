namespace TubeMiniApp.API.DTOs;

/// <summary>
/// DTO для обновления цен (задача со звездочкой)
/// </summary>
public class PriceUpdateDto
{
    public string SKU { get; set; } = string.Empty;
    public decimal PricePerTon { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// DTO для обновления остатков (задача со звездочкой)
/// </summary>
public class StockUpdateDto
{
    public string SKU { get; set; } = string.Empty;
    public decimal DeltaTons { get; set; }
    public decimal DeltaMeters { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// DTO для пакетного обновления цен
/// </summary>
public class PricesBatchUpdateDto
{
    public List<PriceUpdateDto> Prices { get; set; } = new();
}

/// <summary>
/// DTO для пакетного обновления остатков
/// </summary>
public class StocksBatchUpdateDto
{
    public List<StockUpdateDto> Stocks { get; set; } = new();
}
