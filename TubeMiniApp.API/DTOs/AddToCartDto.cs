namespace TubeMiniApp.API.DTOs;

/// <summary>
/// DTO для добавления товара в корзину
/// </summary>
public class AddToCartDto
{
    public long TelegramUserId { get; set; }
    public int ProductId { get; set; }
    
    /// <summary>
    /// Количество в метрах (опционально, если указано QuantityTons)
    /// </summary>
    public decimal? QuantityMeters { get; set; }
    
    /// <summary>
    /// Количество в тоннах (опционально, если указано QuantityMeters)
    /// </summary>
    public decimal? QuantityTons { get; set; }
    
    /// <summary>
    /// Предпочтительная единица измерения (meters/tons)
    /// </summary>
    public string? PreferredUnit { get; set; }
}
