using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using TubeMiniApp.API.DTOs;
using FluentAssertions;
using System.Net;

namespace TubeMiniApp.Tests.Integration;

/// <summary>
/// Интеграционные тесты для API эндпоинтов
/// </summary>
public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetProducts_WithFilters_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/products?warehouse=Екатеринбург");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetWarehouses_ReturnsListOfWarehouses()
    {
        // Act
        var response = await _client.GetAsync("/api/products/warehouses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var warehouses = await response.Content.ReadFromJsonAsync<List<string>>();
        warehouses.Should().NotBeNull();
        warehouses.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AddToCart_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var dto = new AddToCartDto
        {
            TelegramUserId = 123456789,
            ProductId = 1,
            QuantityMeters = 100
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/cart/add", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCart_ReturnsCart()
    {
        // Arrange
        var telegramUserId = 123456789L;
        
        // Add item first
        await _client.PostAsJsonAsync("/api/cart/add", new AddToCartDto
        {
            TelegramUserId = telegramUserId,
            ProductId = 1,
            QuantityMeters = 50
        });

        // Act
        var response = await _client.GetAsync($"/api/cart/{telegramUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var telegramUserId = 987654321L;
        
        // Add items to cart first
        await _client.PostAsJsonAsync("/api/cart/add", new AddToCartDto
        {
            TelegramUserId = telegramUserId,
            ProductId = 1,
            QuantityMeters = 100
        });

        var orderDto = new CreateOrderDto
        {
            TelegramUserId = telegramUserId,
            CustomerName = "Тестовый Клиент",
            CustomerPhone = "+79991234567",
            CustomerEmail = "test@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", orderDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetDiscounts_ReturnsActiveDiscounts()
    {
        // Act
        var response = await _client.GetAsync("/api/discounts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SyncPrices_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var dto = new PricesBatchUpdateDto
        {
            Prices = new List<PriceUpdateDto>
            {
                new PriceUpdateDto
                {
                    SKU = "TUBE-EW-57-3.5-ST3",
                    PricePerTon = 66000,
                    Timestamp = DateTime.UtcNow
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sync/prices", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SyncStocks_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var dto = new StocksBatchUpdateDto
        {
            Stocks = new List<StockUpdateDto>
            {
                new StockUpdateDto
                {
                    SKU = "TUBE-EW-57-3.5-ST3",
                    DeltaTons = -5,
                    DeltaMeters = -1054,
                    Timestamp = DateTime.UtcNow
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sync/stocks", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
