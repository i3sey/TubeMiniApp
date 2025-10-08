using System.Text;
using System.Text.Json;
using TubeMiniApp.API.Models;

namespace TubeMiniApp.API.Services;

/// <summary>
/// Сервис для отправки сообщений через Telegram Bot API
/// </summary>
public interface ITelegramNotificationService
{
    Task SendOrderConfirmationAsync(Order order);
    Task SendMessageAsync(long chatId, string message);
}

public class TelegramNotificationService : ITelegramNotificationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramNotificationService> _logger;

    public TelegramNotificationService(
        HttpClient httpClient, 
        IConfiguration configuration,
        ILogger<TelegramNotificationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendOrderConfirmationAsync(Order order)
    {
        try
        {
            var botToken = _configuration["Telegram:BotToken"];
            if (string.IsNullOrEmpty(botToken))
            {
                _logger.LogWarning("Telegram Bot Token не настроен");
                return;
            }

            var message = FormatOrderMessage(order);
            await SendMessageAsync(order.TelegramUserId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Исключение при отправке уведомления о заказе {order.OrderNumber}");
        }
    }

    public async Task SendMessageAsync(long chatId, string message)
    {
        try
        {
            var botToken = _configuration["Telegram:BotToken"];
            if (string.IsNullOrEmpty(botToken))
            {
                _logger.LogWarning("Telegram Bot Token не настроен");
                return;
            }

            var url = $"https://api.telegram.org/bot{botToken}/sendMessage";

            var payload = new
            {
                chat_id = chatId,
                text = message,
                parse_mode = "HTML"
            };

            var jsonContent = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Сообщение успешно отправлено пользователю {chatId}");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Ошибка отправки сообщения пользователю {chatId}: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Исключение при отправке сообщения пользователю {chatId}");
        }
    }

    private string FormatOrderMessage(Order order)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("🎉 <b>Ваш заказ успешно оформлен!</b>");
        sb.AppendLine();
        sb.AppendLine($"📋 <b>Номер заказа:</b> {order.OrderNumber}");
        sb.AppendLine($"📅 <b>Дата:</b> {order.CreatedAt:dd.MM.yyyy HH:mm}");
        sb.AppendLine($"👤 <b>Клиент:</b> {order.CustomerName}");
        sb.AppendLine($"📞 <b>Телефон:</b> {order.CustomerPhone}");
        
        if (!string.IsNullOrEmpty(order.CustomerEmail))
            sb.AppendLine($"📧 <b>Email:</b> {order.CustomerEmail}");
        
        if (!string.IsNullOrEmpty(order.INN))
            sb.AppendLine($"🏢 <b>ИНН:</b> {order.INN}");
            
        if (!string.IsNullOrEmpty(order.DeliveryAddress))
            sb.AppendLine($"🚚 <b>Адрес доставки:</b> {order.DeliveryAddress}");
        
        sb.AppendLine();
        sb.AppendLine("📦 <b>Состав заказа:</b>");
        
        foreach (var item in order.Items)
        {
            sb.AppendLine($"• {item.Product?.ProductType ?? "Товар"} ({item.Product?.Diameter ?? 0}мм)");
            
            if (item.QuantityMeters > 0)
                sb.AppendLine($"  └ {item.QuantityMeters:F1} м");
            
            if (item.QuantityTons > 0)
                sb.AppendLine($"  └ {item.QuantityTons:F2} т");
                
            sb.AppendLine($"  └ {item.TotalPrice:C0}");
        }
        
        sb.AppendLine();
        
        if (order.TotalDiscount > 0)
        {
            sb.AppendLine($"💰 <b>Скидка:</b> {order.TotalDiscount:C0}");
        }
        
        sb.AppendLine($"💳 <b>Итого:</b> {order.TotalAmount:C0}");
        
        if (!string.IsNullOrEmpty(order.Comment))
        {
            sb.AppendLine();
            sb.AppendLine($"💬 <b>Комментарий:</b> {order.Comment}");
        }
        
        sb.AppendLine();
        sb.AppendLine("📞 Наш менеджер свяжется с вами для уточнения деталей заказа.");
        sb.AppendLine("Спасибо за ваш заказ! 🙏");

        return sb.ToString();
    }
}