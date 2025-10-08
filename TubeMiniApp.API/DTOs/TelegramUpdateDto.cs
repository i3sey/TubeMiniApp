namespace TubeMiniApp.API.DTOs;

public class TelegramUpdateDto
{
    public TelegramMessageDto? Message { get; set; }
}

public class TelegramMessageDto
{
    public long MessageId { get; set; }
    public TelegramUserDto From { get; set; } = new();
    public TelegramChatDto Chat { get; set; } = new();
    public string? Text { get; set; }
    public long Date { get; set; }
}

public class TelegramUserDto
{
    public long Id { get; set; }
    public bool IsBot { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public string? LanguageCode { get; set; }
}

public class TelegramChatDto
{
    public long Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
}