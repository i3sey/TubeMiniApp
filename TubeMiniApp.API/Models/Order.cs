namespace TubeMiniApp.API.Models;

/// <summary>
/// Заказ
/// </summary>
public class Order
{
    public int Id { get; set; }
    
    /// <summary>
    /// ID пользователя Telegram
    /// </summary>
    public long TelegramUserId { get; set; }
    
    /// <summary>
    /// Номер заказа
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Имя клиента
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;
    
    /// <summary>
    /// Телефон клиента
    /// </summary>
    public string CustomerPhone { get; set; } = string.Empty;
    
    /// <summary>
    /// Email клиента
    /// </summary>
    public string? CustomerEmail { get; set; }
    
    /// <summary>
    /// Адрес доставки
    /// </summary>
    public string? DeliveryAddress { get; set; }
    
    /// <summary>
    /// Название компании
    /// </summary>
    public string? CompanyName { get; set; }
    
    /// <summary>
    /// ИНН компании
    /// </summary>
    public string? INN { get; set; }
    
    public List<OrderItem> Items { get; set; } = new();
    
    /// <summary>
    /// Общая сумма заказа
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Общая скидка
    /// </summary>
    public decimal TotalDiscount { get; set; }
    
    /// <summary>
    /// Статус заказа
    /// </summary>
    public OrderStatus Status { get; set; }
    
    /// <summary>
    /// Комментарий к заказу
    /// </summary>
    public string? Comment { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
}

public enum OrderStatus
{
    New = 0,
    Processing = 1,
    Confirmed = 2,
    Shipped = 3,
    Completed = 4,
    Cancelled = 5
}
