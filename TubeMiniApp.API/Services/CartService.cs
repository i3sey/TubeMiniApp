using Microsoft.EntityFrameworkCore;
using TubeMiniApp.API.Data;
using TubeMiniApp.API.Models;
using TubeMiniApp.API.DTOs;

namespace TubeMiniApp.API.Services;

/// <summary>
/// Сервис для работы с корзиной
/// </summary>
public interface ICartService
{
    Task<Cart> GetOrCreateCartAsync(long telegramUserId);
    Task<Cart> AddToCartAsync(AddToCartDto dto);
    Task<Cart> UpdateCartItemQuantityAsync(int cartItemId, decimal? quantityMeters, decimal? quantityTons);
    Task<Cart> RemoveFromCartAsync(int cartItemId);
    Task ClearCartAsync(long telegramUserId);
    Task<Cart?> GetCartAsync(long telegramUserId);
}

public class CartService : ICartService
{
    private readonly ApplicationDbContext _context;
    private readonly IDiscountService _discountService;

    public CartService(ApplicationDbContext context, IDiscountService discountService)
    {
        _context = context;
        _discountService = discountService;
    }

    public async Task<Cart> GetOrCreateCartAsync(long telegramUserId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.TelegramUserId == telegramUserId);

        if (cart == null)
        {
            cart = new Cart
            {
                TelegramUserId = telegramUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        return cart;
    }

    public async Task<Cart?> GetCartAsync(long telegramUserId)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.TelegramUserId == telegramUserId);
    }

    public async Task<Cart> AddToCartAsync(AddToCartDto dto)
    {
        var cart = await GetOrCreateCartAsync(dto.TelegramUserId);
        var product = await _context.Products.FindAsync(dto.ProductId);

        if (product == null)
        {
            throw new InvalidOperationException("Продукт не найден");
        }

        // Расчет количества в метрах и тоннах
        var (quantityMeters, quantityTons, preferredUnit) = CalculateQuantities(product, dto.QuantityMeters, dto.QuantityTons);
        
        // Переопределить если передан явно
        if (!string.IsNullOrEmpty(dto.PreferredUnit))
        {
            preferredUnit = dto.PreferredUnit;
        }

        // Проверка доступности
        if (quantityTons > product.AvailableStockTons)
        {
            throw new InvalidOperationException($"Недостаточно товара на складе. Доступно: {product.AvailableStockTons} тонн");
        }

        // Проверка, есть ли уже этот товар в корзине
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);

        if (existingItem != null)
        {
            existingItem.QuantityMeters += quantityMeters;
            existingItem.QuantityTons += quantityTons;
            // Сохраняем предпочтительную единицу последнего добавления
            existingItem.PreferredUnit = preferredUnit;
        }
        else
        {
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                QuantityMeters = quantityMeters,
                QuantityTons = quantityTons,
                PreferredUnit = preferredUnit,
                UnitPrice = product.PricePerTon,
                AddedAt = DateTime.UtcNow
            };
            cart.Items.Add(cartItem);
            _context.CartItems.Add(cartItem);
        }

        await RecalculateCartAsync(cart);
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return cart;
    }

    public async Task<Cart> UpdateCartItemQuantityAsync(int cartItemId, decimal? quantityMeters, decimal? quantityTons)
    {
        var cartItem = await _context.CartItems
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

        if (cartItem == null)
        {
            throw new InvalidOperationException("Элемент корзины не найден");
        }

        var product = cartItem.Product!;

        // Расчет нового количества
        if (quantityMeters.HasValue && quantityMeters > 0)
        {
            var normalizedMeters = decimal.Round(quantityMeters.Value, 2, MidpointRounding.AwayFromZero);
            cartItem.QuantityMeters = normalizedMeters;
            cartItem.QuantityTons = CalculateTonsFromMeters(product, normalizedMeters);
        }
        else if (quantityTons.HasValue && quantityTons > 0)
        {
            var normalizedTons = decimal.Round(quantityTons.Value, 3, MidpointRounding.AwayFromZero);
            cartItem.QuantityTons = normalizedTons;
            cartItem.QuantityMeters = CalculateMetersFromTons(product, normalizedTons);
        }
        else
        {
            throw new InvalidOperationException("Необходимо указать количество в метрах или тоннах");
        }

        // Проверка доступности
        if (cartItem.QuantityTons > product.AvailableStockTons)
        {
            throw new InvalidOperationException($"Недостаточно товара на складе. Доступно: {product.AvailableStockTons} тонн");
        }

        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstAsync(c => c.Id == cartItem.CartId);

        await RecalculateCartAsync(cart);
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return cart;
    }

    public async Task<Cart> RemoveFromCartAsync(int cartItemId)
    {
        var cartItem = await _context.CartItems.FindAsync(cartItemId);

        if (cartItem == null)
        {
            throw new InvalidOperationException("Элемент корзины не найден");
        }

        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstAsync(c => c.Id == cartItem.CartId);

        _context.CartItems.Remove(cartItem);
        cart.Items.Remove(cartItem);

        await RecalculateCartAsync(cart);
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return cart;
    }

    public async Task ClearCartAsync(long telegramUserId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.TelegramUserId == telegramUserId);

        if (cart != null)
        {
            _context.CartItems.RemoveRange(cart.Items);
            cart.Items.Clear();
            cart.TotalAmount = 0;
            cart.TotalDiscount = 0;
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private async Task RecalculateCartAsync(Cart cart)
    {
        decimal totalAmount = 0;
        decimal totalDiscount = 0;

        foreach (var item in cart.Items)
        {
            if (item.Product == null) continue;

            // Получение актуальной цены
            item.UnitPrice = item.Product.PricePerTon;

            // Расчет скидки для позиции
            var discount = await _discountService.GetDiscountForItemAsync(
                item.QuantityTons,
                item.Product.ProductType,
                item.Product.Warehouse
            );

            item.DiscountPercent = discount;

            // Расчет стоимости
            var basePrice = item.QuantityTons * item.UnitPrice;
            var discountAmount = basePrice * (discount / 100);
            item.TotalPrice = basePrice - discountAmount;

            totalAmount += item.TotalPrice;
            totalDiscount += discountAmount;
        }

        cart.TotalAmount = totalAmount;
        cart.TotalDiscount = totalDiscount;
    }

    private static (decimal quantityMeters, decimal quantityTons, string preferredUnit) CalculateQuantities(
        Product product,
        decimal? requestedMeters,
        decimal? requestedTons)
    {
        if (requestedMeters.HasValue && requestedMeters.Value > 0)
        {
            var normalizedMeters = decimal.Round(requestedMeters.Value, 2, MidpointRounding.AwayFromZero);
            var tons = CalculateTonsFromMeters(product, normalizedMeters);
            return (normalizedMeters, tons, "meters");
        }

        if (requestedTons.HasValue && requestedTons.Value > 0)
        {
            var normalizedTons = decimal.Round(requestedTons.Value, 3, MidpointRounding.AwayFromZero);
            var meters = CalculateMetersFromTons(product, normalizedTons);
            return (meters, normalizedTons, "tons");
        }

        throw new InvalidOperationException("Необходимо указать количество в метрах или тоннах");
    }

    private static decimal CalculateTonsFromMeters(Product product, decimal meters)
    {
        if (meters <= 0)
        {
            return 0;
        }

        var tonsPerMeter = GetTonsPerMeter(product);
        return decimal.Round(meters * tonsPerMeter, 3, MidpointRounding.AwayFromZero);
    }

    private static decimal CalculateMetersFromTons(Product product, decimal tons)
    {
        if (tons <= 0)
        {
            return 0;
        }

        var tonsPerMeter = GetTonsPerMeter(product);

        if (tonsPerMeter <= 0)
        {
            throw new InvalidOperationException("Нет данных для расчета количества в метрах");
        }

        return decimal.Round(tons / tonsPerMeter, 2, MidpointRounding.AwayFromZero);
    }

    private static decimal GetTonsPerMeter(Product product)
    {
        if (product.WeightPerMeter > 0)
        {
            return product.WeightPerMeter / 1000m;
        }

        if (product.AvailableStockMeters > 0)
        {
            return product.AvailableStockTons / product.AvailableStockMeters;
        }

        throw new InvalidOperationException("Нет данных для расчета веса на метр");
    }
}
