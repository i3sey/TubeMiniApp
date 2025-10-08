using System;

namespace TubeMiniApp.API.DTOs;

/// <summary>
/// DTO с данными профиля пользователя на основе последнего заказа
/// </summary>
public class UserProfileDto
{
    public long TelegramUserId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerInn { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? CompanyName { get; set; }
    public DateTime? LastOrderAt { get; set; }
    public bool HasOrders { get; set; }
}
