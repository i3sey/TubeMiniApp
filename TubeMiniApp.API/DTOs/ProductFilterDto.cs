namespace TubeMiniApp.API.DTOs;

/// <summary>
/// DTO для фильтрации продукции
/// </summary>
public class ProductFilterDto
{
    public string? Warehouse { get; set; }
    public string? ProductType { get; set; }
    public decimal? DiameterMin { get; set; }
    public decimal? DiameterMax { get; set; }
    public decimal? WallThicknessMin { get; set; }
    public decimal? WallThicknessMax { get; set; }
    public string? GOST { get; set; }
    public string? SteelGrade { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
