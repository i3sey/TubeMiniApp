using Xunit;
using Microsoft.EntityFrameworkCore;
using TubeMiniApp.API.Data;
using TubeMiniApp.API.Services;
using TubeMiniApp.API.Models;
using TubeMiniApp.API.DTOs;
using FluentAssertions;

namespace TubeMiniApp.Tests.Services;

/// <summary>
/// Тесты для CartService
/// </summary>
public class CartServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CartService _cartService;
    private readonly DiscountService _discountService;

    public CartServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _discountService = new DiscountService(_context);
        _cartService = new CartService(_context, _discountService);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var product = new Product
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
        };

        _context.Products.Add(product);

        // Add discount rules
        var discounts = new List<Discount>
        {
            new Discount { Id = 1, MinQuantityTons = 10, DiscountPercent = 5, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Discount { Id = 2, MinQuantityTons = 50, DiscountPercent = 10, IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        _context.Discounts.AddRange(discounts);

        _context.SaveChanges();
    }

    [Fact]
    public async Task AddToCartAsync_ValidProduct_AddsItemToCart()
    {
        // Arrange
        var dto = new AddToCartDto
        {
            TelegramUserId = 12345,
            ProductId = 1,
            QuantityMeters = 100
        };

        // Act
        var cart = await _cartService.AddToCartAsync(dto);

        // Assert
        cart.Should().NotBeNull();
        cart.Items.Should().HaveCount(1);
        cart.Items.First().QuantityMeters.Should().Be(100);
    }

    [Fact]
    public async Task AddToCartAsync_CalculatesQuantityTons_Correctly()
    {
        // Arrange
        var dto = new AddToCartDto
        {
            TelegramUserId = 12345,
            ProductId = 1,
            QuantityMeters = 1000 // 1000 метров
        };

        // Act
        var cart = await _cartService.AddToCartAsync(dto);

        // Assert
        var item = cart.Items.First();
        // 1000 метров * 4.74 кг/метр / 1000 = 4.74 тонны
        item.QuantityTons.Should().BeApproximately(4.74m, 0.01m);
    }

    [Fact]
    public async Task AddToCartAsync_CalculatesQuantityMeters_FromTons()
    {
        // Arrange
        var dto = new AddToCartDto
        {
            TelegramUserId = 12345,
            ProductId = 1,
            QuantityTons = 10 // 10 тонн
        };

        // Act
        var cart = await _cartService.AddToCartAsync(dto);

        // Assert
        var item = cart.Items.First();
        // 10 тонн * 1000 / 4.74 кг/метр ≈ 2110 метров
        item.QuantityMeters.Should().BeApproximately(2110m, 10m);
    }

    [Fact]
    public async Task AddToCartAsync_UsesStockRatio_WhenWeightPerMeterMissing()
    {
        // Arrange
        var product = await _context.Products.FirstAsync();
        product.WeightPerMeter = 0;
        product.AvailableStockMeters = 338.4m;
        product.AvailableStockTons = 34.01m;
        await _context.SaveChangesAsync();

        var dto = new AddToCartDto
        {
            TelegramUserId = 54321,
            ProductId = product.Id,
            QuantityMeters = 338.4m
        };

        // Act
        var cart = await _cartService.AddToCartAsync(dto);

        // Assert
        var item = cart.Items.First();
        item.QuantityTons.Should().BeApproximately(34.01m, 0.05m);
        item.QuantityMeters.Should().Be(338.4m);
    }

    [Fact]
    public async Task AddToCartAsync_AppliesDiscount_WhenThresholdMet()
    {
        // Arrange
        var dto = new AddToCartDto
        {
            TelegramUserId = 12345,
            ProductId = 1,
            QuantityTons = 15 // 15 тонн - должна применяться скидка 5%
        };

        // Act
        var cart = await _cartService.AddToCartAsync(dto);

        // Assert
        var item = cart.Items.First();
        item.DiscountPercent.Should().Be(5);
    }

    [Fact]
    public async Task AddToCartAsync_InsufficientStock_ThrowsException()
    {
        // Arrange
        var dto = new AddToCartDto
        {
            TelegramUserId = 12345,
            ProductId = 1,
            QuantityTons = 200 // Больше, чем доступно (150 тонн)
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _cartService.AddToCartAsync(dto)
        );
    }

    [Fact]
    public async Task RemoveFromCartAsync_ExistingItem_RemovesFromCart()
    {
        // Arrange
        var dto = new AddToCartDto
        {
            TelegramUserId = 12345,
            ProductId = 1,
            QuantityMeters = 100
        };
        var cart = await _cartService.AddToCartAsync(dto);
        var itemId = cart.Items.First().Id;

        // Act
        var updatedCart = await _cartService.RemoveFromCartAsync(itemId);

        // Assert
        updatedCart.Items.Should().BeEmpty();
        updatedCart.TotalAmount.Should().Be(0);
    }

    [Fact]
    public async Task UpdateCartItemQuantityAsync_ValidQuantity_UpdatesItem()
    {
        // Arrange
        var dto = new AddToCartDto
        {
            TelegramUserId = 12345,
            ProductId = 1,
            QuantityMeters = 100
        };
        var cart = await _cartService.AddToCartAsync(dto);
        var itemId = cart.Items.First().Id;

        // Act
        var updatedCart = await _cartService.UpdateCartItemQuantityAsync(itemId, 200, null);

        // Assert
        var item = updatedCart.Items.First();
        item.QuantityMeters.Should().Be(200);
    }

    [Fact]
    public async Task ClearCartAsync_RemovesAllItems()
    {
        // Arrange
        var telegramUserId = 12345L;
        var dto = new AddToCartDto
        {
            TelegramUserId = telegramUserId,
            ProductId = 1,
            QuantityMeters = 100
        };
        await _cartService.AddToCartAsync(dto);

        // Act
        await _cartService.ClearCartAsync(telegramUserId);

        // Assert
        var cart = await _cartService.GetCartAsync(telegramUserId);
        cart!.Items.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
