namespace TubeMiniApp.API.DTOs;

/// <summary>
/// DTO для создания заказа
/// </summary>
public class CreateOrderDto
{
    public long TelegramUserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? CustomerInn { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? CompanyName { get; set; }
    public string? Comment { get; set; }
}
