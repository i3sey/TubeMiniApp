using Microsoft.AspNetCore.Mvc;
using TubeMiniApp.API.Services;

namespace TubeMiniApp.API.Controllers;

/// <summary>
/// Контроллер для работы со скидками
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DiscountsController : ControllerBase
{
    private readonly IDiscountService _discountService;

    public DiscountsController(IDiscountService discountService)
    {
        _discountService = discountService;
    }

    /// <summary>
    /// Получить список активных скидок
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDiscounts()
    {
        var discounts = await _discountService.GetActiveDiscountsAsync();
        return Ok(discounts);
    }
}
