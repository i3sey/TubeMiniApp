namespace TubeMiniApp.API.Models;

/// <summary>
/// Элемент заказа
/// </summary>
public class OrderItem
{
    public int Id { get; set; }
    
    public int OrderId { get; set; }
    
    public int ProductId { get; set; }
    
    public Product? Product { get; set; }
    
    /// <summary>
    /// Количество в метрах
    /// </summary>
    public decimal QuantityMeters { get; set; }
    
    /// <summary>
    /// Количество в тоннах
    /// </summary>
    public decimal QuantityTons { get; set; }
    
    /// <summary>
    /// Цена за тонну на момент оформления заказа
    /// </summary>
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// Скидка в процентах
    /// </summary>
    public decimal DiscountPercent { get; set; }
    
    /// <summary>
    /// Итоговая цена позиции
    /// </summary>
    public decimal TotalPrice { get; set; }
}
