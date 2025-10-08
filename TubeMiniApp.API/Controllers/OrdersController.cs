using Microsoft.AspNetCore.Mvc;
using TubeMiniApp.API.Services;
using TubeMiniApp.API.DTOs;
using TubeMiniApp.API.Models;

namespace TubeMiniApp.API.Controllers;

/// <summary>
/// Контроллер для работы с заказами
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Создать заказ из корзины
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        try
        {
            var order = await _orderService.CreateOrderFromCartAsync(dto);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Получить заказ по ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);

        if (order == null)
        {
            return NotFound(new { message = "Заказ не найден" });
        }

        return Ok(order);
    }

    /// <summary>
    /// Получить заказ по номеру
    /// </summary>
    [HttpGet("number/{orderNumber}")]
    public async Task<IActionResult> GetOrderByNumber(string orderNumber)
    {
        var order = await _orderService.GetOrderByNumberAsync(orderNumber);

        if (order == null)
        {
            return NotFound(new { message = "Заказ не найден" });
        }

        return Ok(order);
    }

    /// <summary>
    /// Получить список заказов пользователя
    /// </summary>
    [HttpGet("user/{telegramUserId}")]
    public async Task<IActionResult> GetUserOrders(long telegramUserId)
    {
        var orders = await _orderService.GetUserOrdersAsync(telegramUserId);
        return Ok(orders);
    }

    /// <summary>
    /// Получить данные профиля пользователя на основе последних заказов
    /// </summary>
    [HttpGet("user/{telegramUserId}/profile")]
    public async Task<IActionResult> GetUserProfile(long telegramUserId)
    {
        var profile = await _orderService.GetUserProfileAsync(telegramUserId);

        if (profile == null)
        {
            return NotFound(new { message = "Данные профиля не найдены" });
        }

        return Ok(profile);
    }

    /// <summary>
    /// Обновить статус заказа
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        try
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, dto.Status);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
}
