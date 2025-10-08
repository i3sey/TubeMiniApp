using Microsoft.AspNetCore.Mvc;
using TubeMiniApp.API.Services;
using TubeMiniApp.API.DTOs;

namespace TubeMiniApp.API.Controllers;

/// <summary>
/// Контроллер для работы с продукцией
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Получить список продукции с фильтрацией
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] ProductFilterDto filter)
    {
        var (products, totalCount) = await _productService.GetFilteredProductsAsync(filter);

        return Ok(new
        {
            data = products,
            totalCount,
            pageNumber = filter.PageNumber,
            pageSize = filter.PageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
        });
    }

    /// <summary>
    /// Получить продукт по ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound(new { message = "Продукт не найден" });
        }

        return Ok(product);
    }

    /// <summary>
    /// Получить список складов
    /// </summary>
    [HttpGet("warehouses")]
    public async Task<IActionResult> GetWarehouses()
    {
        var warehouses = await _productService.GetWarehousesAsync();
        return Ok(warehouses);
    }

    /// <summary>
    /// Получить список видов продукции
    /// </summary>
    [HttpGet("types")]
    public async Task<IActionResult> GetProductTypes()
    {
        var types = await _productService.GetProductTypesAsync();
        return Ok(types);
    }

    /// <summary>
    /// Получить список ГОСТов
    /// </summary>
    [HttpGet("gosts")]
    public async Task<IActionResult> GetGOSTs()
    {
        var gosts = await _productService.GetGOSTsAsync();
        return Ok(gosts);
    }

    /// <summary>
    /// Получить список марок стали
    /// </summary>
    [HttpGet("steel-grades")]
    public async Task<IActionResult> GetSteelGrades()
    {
        var grades = await _productService.GetSteelGradesAsync();
        return Ok(grades);
    }

    /// <summary>
    /// Получить все опции для фильтров
    /// </summary>
    [HttpGet("filter-options")]
    public async Task<IActionResult> GetFilterOptions()
    {
        var warehouses = await _productService.GetWarehousesAsync();
        var types = await _productService.GetProductTypesAsync();
        var gosts = await _productService.GetGOSTsAsync();
        var grades = await _productService.GetSteelGradesAsync();

        return Ok(new
        {
            warehouses,
            productTypes = types,
            gosts,
            steelGrades = grades
        });
    }
}
