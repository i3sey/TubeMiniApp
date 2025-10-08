namespace TubeMiniApp.API.Models;

/// <summary>
/// Корзина покупателя
/// </summary>
public class Cart
{
    public int Id { get; set; }
    
    /// <summary>
    /// ID пользователя Telegram
    /// </summary>
    public long TelegramUserId { get; set; }
    
    public List<CartItem> Items { get; set; } = new();
    
    /// <summary>
    /// Общая сумма корзины
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Общая скидка
    /// </summary>
    public decimal TotalDiscount { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}
