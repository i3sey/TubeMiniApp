using Xunit;
using Microsoft.EntityFrameworkCore;
using TubeMiniApp.API.Data;
using TubeMiniApp.API.Services;
using TubeMiniApp.API.Models;
using TubeMiniApp.API.DTOs;
using FluentAssertions;

namespace TubeMiniApp.Tests.Services;

/// <summary>
/// Тесты для OrderService
/// </summary>
public class OrderServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly OrderService _orderService;
    private readonly CartService _cartService;
    private readonly DiscountService _discountService;
    private readonly ITelegramNotificationService _telegramNotificationService;

    public OrderServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _discountService = new DiscountService(_context);
        _cartService = new CartService(_context, _discountService);
        _telegramNotificationService = new NoopTelegramNotificationService();
        _orderService = new OrderService(_context, _cartService, _telegramNotificationService);

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
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateOrderFromCartAsync_ValidCart_CreatesOrder()
    {
        // Arrange
        var telegramUserId = 12345L;
        
        // Add item to cart first
        await _cartService.AddToCartAsync(new AddToCartDto
        {
            TelegramUserId = telegramUserId,
            ProductId = 1,
            QuantityMeters = 100
        });

        var orderDto = new CreateOrderDto
        {
            TelegramUserId = telegramUserId,
            CustomerName = "Иван Иванов",
            CustomerPhone = "+79001234567",
            CustomerEmail = "ivan@example.com"
        };

        // Act
        var order = await _orderService.CreateOrderFromCartAsync(orderDto);

        // Assert
        order.Should().NotBeNull();
        order.OrderNumber.Should().NotBeNullOrEmpty();
        order.CustomerName.Should().Be("Иван Иванов");
        order.Items.Should().HaveCount(1);
        order.Status.Should().Be(OrderStatus.New);
    }

    [Fact]
    public async Task CreateOrderFromCartAsync_EmptyCart_ThrowsException()
    {
        // Arrange
        var orderDto = new CreateOrderDto
        {
            TelegramUserId = 99999,
            CustomerName = "Иван Иванов",
            CustomerPhone = "+79001234567"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _orderService.CreateOrderFromCartAsync(orderDto)
        );
    }

    [Fact]
    public async Task CreateOrderFromCartAsync_ReducesStock()
    {
        // Arrange
        var telegramUserId = 12345L;
        var initialStock = 150m;
        
        await _cartService.AddToCartAsync(new AddToCartDto
        {
            TelegramUserId = telegramUserId,
            ProductId = 1,
            QuantityTons = 10
        });

        var orderDto = new CreateOrderDto
        {
            TelegramUserId = telegramUserId,
            CustomerName = "Иван Иванов",
            CustomerPhone = "+79001234567"
        };

        // Act
        await _orderService.CreateOrderFromCartAsync(orderDto);

        // Assert
        var product = await _context.Products.FindAsync(1);
        product!.AvailableStockTons.Should().Be(initialStock - 10);
    }

    [Fact]
    public async Task CreateOrderFromCartAsync_ClearsCart()
    {
        // Arrange
        var telegramUserId = 12345L;
        
        await _cartService.AddToCartAsync(new AddToCartDto
        {
            TelegramUserId = telegramUserId,
            ProductId = 1,
            QuantityMeters = 100
        });

        var orderDto = new CreateOrderDto
        {
            TelegramUserId = telegramUserId,
            CustomerName = "Иван Иванов",
            CustomerPhone = "+79001234567"
        };

        // Act
        await _orderService.CreateOrderFromCartAsync(orderDto);

        // Assert
        var cart = await _cartService.GetCartAsync(telegramUserId);
        cart!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserOrdersAsync_ReturnsUserOrders()
    {
        // Arrange
        var telegramUserId = 12345L;
        
        await _cartService.AddToCartAsync(new AddToCartDto
        {
            TelegramUserId = telegramUserId,
            ProductId = 1,
            QuantityMeters = 100
        });

        await _orderService.CreateOrderFromCartAsync(new CreateOrderDto
        {
            TelegramUserId = telegramUserId,
            CustomerName = "Иван Иванов",
            CustomerPhone = "+79001234567"
        });

        // Act
        var orders = await _orderService.GetUserOrdersAsync(telegramUserId);

        // Assert
        orders.Should().HaveCount(1);
        orders.First().TelegramUserId.Should().Be(telegramUserId);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ChangesStatus()
    {
        // Arrange
        var telegramUserId = 12345L;
        
        await _cartService.AddToCartAsync(new AddToCartDto
        {
            TelegramUserId = telegramUserId,
            ProductId = 1,
            QuantityMeters = 100
        });

        var order = await _orderService.CreateOrderFromCartAsync(new CreateOrderDto
        {
            TelegramUserId = telegramUserId,
            CustomerName = "Иван Иванов",
            CustomerPhone = "+79001234567"
        });

        // Act
        var updatedOrder = await _orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Processing);

        // Assert
        updatedOrder.Status.Should().Be(OrderStatus.Processing);
        updatedOrder.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserProfileAsync_ReturnsLatestOrderData()
    {
        // Arrange
        var telegramUserId = 77777L;

        await _cartService.AddToCartAsync(new AddToCartDto
        {
            TelegramUserId = telegramUserId,
            ProductId = 1,
            QuantityMeters = 150
        });

        var dto = new CreateOrderDto
        {
            TelegramUserId = telegramUserId,
            CustomerName = "Петр Петров",
            CustomerPhone = "+79001112233",
            CustomerEmail = "peter@example.com",
            CustomerInn = "1234567890",
            DeliveryAddress = "Екатеринбург, ул. Тестовая 1"
        };

        await _orderService.CreateOrderFromCartAsync(dto);

        // Act
        var profile = await _orderService.GetUserProfileAsync(telegramUserId);

        // Assert
        profile.Should().NotBeNull();
        profile!.CustomerName.Should().Be(dto.CustomerName);
        profile.CustomerPhone.Should().Be(dto.CustomerPhone);
        profile.CustomerEmail.Should().Be(dto.CustomerEmail);
        profile.CustomerInn.Should().Be(dto.CustomerInn);
        profile.DeliveryAddress.Should().Be(dto.DeliveryAddress);
        profile.HasOrders.Should().BeTrue();
        profile.LastOrderAt.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

internal class NoopTelegramNotificationService : ITelegramNotificationService
{
    public Task SendOrderConfirmationAsync(Order order) => Task.CompletedTask;
}
