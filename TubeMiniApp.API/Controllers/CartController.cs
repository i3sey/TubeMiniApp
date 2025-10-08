using Microsoft.AspNetCore.Mvc;
using TubeMiniApp.API.Services;
using TubeMiniApp.API.DTOs;

namespace TubeMiniApp.API.Controllers;

/// <summary>
/// Контроллер для работы с корзиной
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Получить корзину пользователя
    /// </summary>
    [HttpGet("{telegramUserId}")]
    public async Task<IActionResult> GetCart(long telegramUserId)
    {
        var cart = await _cartService.GetCartAsync(telegramUserId);

        if (cart == null)
        {
            return NotFound(new { message = "Корзина не найдена" });
        }

        return Ok(cart);
    }

    /// <summary>
    /// Добавить товар в корзину
    /// </summary>
    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
    {
        try
        {
            var cart = await _cartService.AddToCartAsync(dto);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Обновить количество товара в корзине
    /// </summary>
    [HttpPut("item/{cartItemId}")]
    public async Task<IActionResult> UpdateCartItem(
        int cartItemId,
        [FromBody] UpdateCartItemDto dto)
    {
        try
        {
            var cart = await _cartService.UpdateCartItemQuantityAsync(
                cartItemId,
                dto.QuantityMeters,
                dto.QuantityTons
            );
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Удалить товар из корзины
    /// </summary>
    [HttpDelete("item/{cartItemId}")]
    public async Task<IActionResult> RemoveFromCart(int cartItemId)
    {
        try
        {
            var cart = await _cartService.RemoveFromCartAsync(cartItemId);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Очистить корзину
    /// </summary>
    [HttpDelete("{telegramUserId}")]
    public async Task<IActionResult> ClearCart(long telegramUserId)
    {
        await _cartService.ClearCartAsync(telegramUserId);
        return Ok(new { message = "Корзина очищена" });
    }
}

public class UpdateCartItemDto
{
    public decimal? QuantityMeters { get; set; }
    public decimal? QuantityTons { get; set; }
}
