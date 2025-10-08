using Microsoft.AspNetCore.Mvc;
using TubeMiniApp.API.DTOs;
using TubeMiniApp.API.Services;

namespace TubeMiniApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelegramController : ControllerBase
{
    private readonly ILogger<TelegramController> _logger;
    private readonly ITelegramNotificationService _telegramService;

    public TelegramController(
        ILogger<TelegramController> logger,
        ITelegramNotificationService telegramService)
    {
        _logger = logger;
        _telegramService = telegramService;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook([FromBody] TelegramUpdateDto update)
    {
        try
        {
            _logger.LogInformation("Получено обновление от Telegram: {UpdateType}", 
                update.Message?.Text ?? "Non-text update");

            // Обрабатываем текстовые сообщения
            if (update.Message?.Text != null)
            {
                await HandleTextMessage(update);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке webhook от Telegram");
            return StatusCode(500);
        }
    }

    private async Task HandleTextMessage(TelegramUpdateDto update)
    {
        var message = update.Message;
        var chatId = message.Chat.Id;
        var text = message.Text.Trim();

        _logger.LogInformation("Обработка сообщения от пользователя {UserId}: {Text}", 
            message.From.Id, text);

        switch (text.ToLower())
        {
            case "/start":
                await HandleStartCommand(chatId, message.From);
                break;
            
            case "/help":
                await HandleHelpCommand(chatId);
                break;
            
            default:
                await HandleUnknownCommand(chatId);
                break;
        }
    }

    private async Task HandleStartCommand(long chatId, TelegramUserDto user)
    {
        var welcomeMessage = $"""
            🛒 Добро пожаловать в ТМК!
            
            👋 Привет, {user.FirstName}!
            
            Это мини-приложение для заказа металлопроката. 
            Здесь вы можете:
            
            🔍 Просматривать каталог продукции
            📦 Добавлять товары в корзину  
            📋 Оформлять заказы
            📊 Отслеживать историю заказов
            
            Для начала работы нажмите кнопку "Открыть магазин" ниже или используйте команду /help для получения справки.
            """;

        await _telegramService.SendMessageAsync(chatId, welcomeMessage);
        
        _logger.LogInformation("Отправлено приветственное сообщение пользователю {UserId}", user.Id);
    }

    private async Task HandleHelpCommand(long chatId)
    {
        var helpMessage = """
            📚 Справка по боту ТМК
            
            🤖 Доступные команды:
            /start - Начать работу с ботом
            /help - Показать эту справку
            
            🛍️ Как сделать заказ:
            1. Откройте мини-приложение
            2. Выберите нужные товары
            3. Добавьте их в корзину
            4. Оформите заказ
            
            📞 Техподдержка: @support_username
            🌐 Сайт: https://sa05.me
            """;

        await _telegramService.SendMessageAsync(chatId, helpMessage);
    }

    private async Task HandleUnknownCommand(long chatId)
    {
        var unknownMessage = """
            ❓ Команда не распознана
            
            Используйте:
            /start - Начать работу
            /help - Показать справку
            
            Или откройте мини-приложение для работы с каталогом! 🛒
            """;

        await _telegramService.SendMessageAsync(chatId, unknownMessage);
    }
}