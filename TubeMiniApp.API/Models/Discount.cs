namespace TubeMiniApp.API.Models;

/// <summary>
/// Правило скидки в зависимости от объема заказа
/// </summary>
public class Discount
{
    public int Id { get; set; }
    
    /// <summary>
    /// Минимальный объем заказа в тоннах для применения скидки
    /// </summary>
    public decimal MinQuantityTons { get; set; }
    
    /// <summary>
    /// Процент скидки
    /// </summary>
    public decimal DiscountPercent { get; set; }
    
    /// <summary>
    /// Применяется ли к конкретному типу продукции (null = для всех)
    /// </summary>
    public string? ProductType { get; set; }
    
    /// <summary>
    /// Применяется ли к конкретному складу (null = для всех)
    /// </summary>
    public string? Warehouse { get; set; }
    
    /// <summary>
    /// Активна ли скидка
    /// </summary>
    public bool IsActive { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
