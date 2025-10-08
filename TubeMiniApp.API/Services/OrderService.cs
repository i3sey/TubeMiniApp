using Microsoft.EntityFrameworkCore;
using TubeMiniApp.API.Data;
using TubeMiniApp.API.Models;
using TubeMiniApp.API.DTOs;

namespace TubeMiniApp.API.Services;

/// <summary>
/// Сервис для работы с заказами
/// </summary>
public interface IOrderService
{
    Task<Order> CreateOrderFromCartAsync(CreateOrderDto dto);
    Task<Order?> GetOrderByIdAsync(int orderId);
    Task<Order?> GetOrderByNumberAsync(string orderNumber);
    Task<List<Order>> GetUserOrdersAsync(long telegramUserId);
    Task<Order> UpdateOrderStatusAsync(int orderId, OrderStatus status);
    Task<UserProfileDto?> GetUserProfileAsync(long telegramUserId);
}

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ICartService _cartService;
    private readonly ITelegramNotificationService _telegramNotificationService;

    public OrderService(
        ApplicationDbContext context, 
        ICartService cartService,
        ITelegramNotificationService telegramNotificationService)
    {
        _context = context;
        _cartService = cartService;
        _telegramNotificationService = telegramNotificationService;
    }

    public async Task<Order> CreateOrderFromCartAsync(CreateOrderDto dto)
    {
        var cart = await _cartService.GetCartAsync(dto.TelegramUserId);

        if (cart == null || !cart.Items.Any())
        {
            throw new InvalidOperationException("Корзина пуста");
        }

        // Проверка доступности товаров
        foreach (var item in cart.Items)
        {
            if (item.Product == null) continue;

            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null || product.AvailableStockTons < item.QuantityTons)
            {
                throw new InvalidOperationException(
                    $"Недостаточно товара '{product?.ProductType}' на складе. Требуется: {item.QuantityTons} тонн, доступно: {product?.AvailableStockTons ?? 0} тонн"
                );
            }
        }

        // Создание заказа
        var order = new Order
        {
            TelegramUserId = dto.TelegramUserId,
            OrderNumber = GenerateOrderNumber(),
            CustomerName = dto.CustomerName,
            CustomerPhone = dto.CustomerPhone,
            CustomerEmail = dto.CustomerEmail,
            DeliveryAddress = dto.DeliveryAddress,
            CompanyName = dto.CompanyName,
            INN = dto.CustomerInn,
            Comment = dto.Comment,
            Status = OrderStatus.New,
            CreatedAt = DateTime.UtcNow,
            TotalAmount = cart.TotalAmount,
            TotalDiscount = cart.TotalDiscount
        };

        // Копирование товаров из корзины в заказ
        foreach (var cartItem in cart.Items)
        {
            var orderItem = new OrderItem
            {
                ProductId = cartItem.ProductId,
                QuantityMeters = cartItem.QuantityMeters,
                QuantityTons = cartItem.QuantityTons,
                UnitPrice = cartItem.UnitPrice,
                DiscountPercent = cartItem.DiscountPercent,
                TotalPrice = cartItem.TotalPrice
            };
            order.Items.Add(orderItem);

            // Уменьшение остатков
            var product = await _context.Products.FindAsync(cartItem.ProductId);
            if (product != null)
            {
                product.AvailableStockTons -= cartItem.QuantityTons;
                product.AvailableStockMeters -= cartItem.QuantityMeters;
            }
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Очистка корзины
        await _cartService.ClearCartAsync(dto.TelegramUserId);

        // Отправка уведомления пользователю (в фоновом режиме)
        _ = Task.Run(async () =>
        {
            try
            {
                await _telegramNotificationService.SendOrderConfirmationAsync(order);
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не прерываем основной процесс
                Console.WriteLine($"Ошибка отправки уведомления: {ex.Message}");
            }
        });

        return order;
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<Order?> GetOrderByNumberAsync(string orderNumber)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }

    public async Task<List<Order>> GetUserOrdersAsync(long telegramUserId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.TelegramUserId == telegramUserId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order> UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = await GetOrderByIdAsync(orderId);

        if (order == null)
        {
            throw new InvalidOperationException("Заказ не найден");
        }

        order.Status = status;

        if (status == OrderStatus.Processing || status == OrderStatus.Confirmed)
        {
            order.ProcessedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return order;
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(long telegramUserId)
    {
        var lastOrder = await _context.Orders
            .Where(o => o.TelegramUserId == telegramUserId)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (lastOrder == null)
        {
            return null;
        }

        return new UserProfileDto
        {
            TelegramUserId = telegramUserId,
            CustomerName = lastOrder.CustomerName,
            CustomerPhone = lastOrder.CustomerPhone,
            CustomerEmail = lastOrder.CustomerEmail,
            CustomerInn = lastOrder.INN,
            DeliveryAddress = lastOrder.DeliveryAddress,
            CompanyName = lastOrder.CompanyName,
            LastOrderAt = lastOrder.CreatedAt,
            HasOrders = true
        };
    }

    private string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"ORD-{timestamp}-{random}";
    }
}
