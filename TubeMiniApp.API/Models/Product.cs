namespace TubeMiniApp.API.Models;

/// <summary>
/// Модель продукции (трубы)
/// </summary>
public class Product
{
    public int Id { get; set; }
    
    /// <summary>
    /// Название склада
    /// </summary>
    public string Warehouse { get; set; } = string.Empty;
    
    /// <summary>
    /// Вид продукции (например: труба электросварная, бесшовная и т.д.)
    /// </summary>
    public string ProductType { get; set; } = string.Empty;
    
    /// <summary>
    /// Диаметр в мм
    /// </summary>
    public decimal Diameter { get; set; }
    
    /// <summary>
    /// Толщина стенки в мм
    /// </summary>
    public decimal WallThickness { get; set; }
    
    /// <summary>
    /// ГОСТ
    /// </summary>
    public string GOST { get; set; } = string.Empty;
    
    /// <summary>
    /// Марка стали
    /// </summary>
    public string SteelGrade { get; set; } = string.Empty;
    
    /// <summary>
    /// Текущая цена за тонну
    /// </summary>
    public decimal PricePerTon { get; set; }
    
    /// <summary>
    /// Вес метра трубы в кг
    /// </summary>
    public decimal WeightPerMeter { get; set; }
    
    /// <summary>
    /// Доступное количество в тоннах
    /// </summary>
    public decimal AvailableStockTons { get; set; }
    
    /// <summary>
    /// Доступное количество в метрах
    /// </summary>
    public decimal AvailableStockMeters { get; set; }
    
    /// <summary>
    /// Дата последнего обновления цены
    /// </summary>
    public DateTime LastPriceUpdate { get; set; }
    
    /// <summary>
    /// Артикул
    /// </summary>
    public string? SKU { get; set; }
}
