namespace TubeMiniApp.API.Models;

/// <summary>
/// Элемент корзины
/// </summary>
public class CartItem
{
    public int Id { get; set; }
    
    public int CartId { get; set; }
    
    public int ProductId { get; set; }
    
    public Product? Product { get; set; }
    
    /// <summary>
    /// Количество в метрах
    /// </summary>
    public decimal QuantityMeters { get; set; }
    
    /// <summary>
    /// Количество в тоннах (рассчитывается автоматически)
    /// </summary>
    public decimal QuantityTons { get; set; }
    
    /// <summary>
    /// Предпочтительная единица измерения, выбранная пользователем (meters/tons)
    /// </summary>
    public string PreferredUnit { get; set; } = "meters";
    
    /// <summary>
    /// Цена за единицу на момент добавления в корзину
    /// </summary>
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// Скидка в процентах
    /// </summary>
    public decimal DiscountPercent { get; set; }
    
    /// <summary>
    /// Итоговая цена с учетом скидки
    /// </summary>
    public decimal TotalPrice { get; set; }
    
    public DateTime AddedAt { get; set; }
}
