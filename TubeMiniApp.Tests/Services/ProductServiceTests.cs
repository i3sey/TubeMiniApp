using Xunit;
using Microsoft.EntityFrameworkCore;
using TubeMiniApp.API.Data;
using TubeMiniApp.API.Services;
using TubeMiniApp.API.Models;
using TubeMiniApp.API.DTOs;
using FluentAssertions;

namespace TubeMiniApp.Tests.Services;

/// <summary>
/// Тесты для ProductService
/// </summary>
public class ProductServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new ProductService(_context);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var products = new List<Product>
        {
            new Product
            {
                Id = 1,
                Warehouse = "Склад Екатеринбург",
                ProductType = "Труба электросварная",
                Diameter = 57,
                WallThickness = 3.5m,
                GOST = "ГОСТ 10704-91",
                SteelGrade = "Ст3сп",
                PricePerTon = 65000,
                WeightPerMeter = 4.74m,
                AvailableStockTons = 150,
                AvailableStockMeters = 31646,
                LastPriceUpdate = DateTime.UtcNow,
                SKU = "TUBE-1"
            },
            new Product
            {
                Id = 2,
                Warehouse = "Склад Москва",
                ProductType = "Труба бесшовная",
                Diameter = 76,
                WallThickness = 5,
                GOST = "ГОСТ 8732-78",
                SteelGrade = "20",
                PricePerTon = 78000,
                WeightPerMeter = 8.86m,
                AvailableStockTons = 200,
                AvailableStockMeters = 22574,
                LastPriceUpdate = DateTime.UtcNow,
                SKU = "TUBE-2"
            },
            new Product
            {
                Id = 3,
                Warehouse = "Склад Екатеринбург",
                ProductType = "Труба электросварная",
                Diameter = 108,
                WallThickness = 4,
                GOST = "ГОСТ 10704-91",
                SteelGrade = "Ст3сп",
                PricePerTon = 67000,
                WeightPerMeter = 10.42m,
                AvailableStockTons = 0, // Нет в наличии
                AvailableStockMeters = 0,
                LastPriceUpdate = DateTime.UtcNow,
                SKU = "TUBE-3"
            }
        };

        _context.Products.AddRange(products);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetFilteredProductsAsync_NoFilters_ReturnsAllProductsInStock()
    {
        // Arrange
        var filter = new ProductFilterDto();

        // Act
        var (products, totalCount) = await _service.GetFilteredProductsAsync(filter);

        // Assert
        products.Should().HaveCount(2); // Только товары с остатками
        totalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetFilteredProductsAsync_FilterByWarehouse_ReturnsMatchingProducts()
    {
        // Arrange
        var filter = new ProductFilterDto { Warehouse = "Екатеринбург" };

        // Act
        var (products, totalCount) = await _service.GetFilteredProductsAsync(filter);

        // Assert
        products.Should().HaveCount(1);
        products.First().Warehouse.Should().Contain("Екатеринбург");
    }

    [Fact]
    public async Task GetFilteredProductsAsync_FilterByDiameter_ReturnsMatchingProducts()
    {
        // Arrange
        var filter = new ProductFilterDto { DiameterMin = 70, DiameterMax = 80 };

        // Act
        var (products, totalCount) = await _service.GetFilteredProductsAsync(filter);

        // Assert
        products.Should().HaveCount(1);
        products.First().Diameter.Should().Be(76);
    }

    [Fact]
    public async Task GetFilteredProductsAsync_FilterByProductType_ReturnsMatchingProducts()
    {
        // Arrange
        var filter = new ProductFilterDto { ProductType = "бесшовная" };

        // Act
        var (products, totalCount) = await _service.GetFilteredProductsAsync(filter);

        // Assert
        products.Should().HaveCount(1);
        products.First().ProductType.Should().Contain("бесшовная");
    }

    [Fact]
    public async Task GetProductByIdAsync_ExistingProduct_ReturnsProduct()
    {
        // Arrange
        var productId = 1;

        // Act
        var product = await _service.GetProductByIdAsync(productId);

        // Assert
        product.Should().NotBeNull();
        product!.Id.Should().Be(productId);
    }

    [Fact]
    public async Task GetProductByIdAsync_NonExistingProduct_ReturnsNull()
    {
        // Arrange
        var productId = 999;

        // Act
        var product = await _service.GetProductByIdAsync(productId);

        // Assert
        product.Should().BeNull();
    }

    [Fact]
    public async Task GetWarehousesAsync_ReturnsDistinctWarehouses()
    {
        // Act
        var warehouses = await _service.GetWarehousesAsync();

        // Assert
        warehouses.Should().HaveCount(2);
        warehouses.Should().Contain("Склад Екатеринбург");
        warehouses.Should().Contain("Склад Москва");
    }

    [Fact]
    public async Task GetProductTypesAsync_ReturnsDistinctTypes()
    {
        // Act
        var types = await _service.GetProductTypesAsync();

        // Assert
        types.Should().HaveCount(2);
        types.Should().Contain("Труба электросварная");
        types.Should().Contain("Труба бесшовная");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
