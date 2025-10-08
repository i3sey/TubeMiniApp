using Microsoft.EntityFrameworkCore;
using TubeMiniApp.API.Models;

namespace TubeMiniApp.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Discount> Discounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Конфигурация Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Warehouse).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ProductType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.GOST).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SteelGrade).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SKU).HasMaxLength(100);
            entity.Property(e => e.PricePerTon).HasPrecision(18, 2);
            entity.Property(e => e.WeightPerMeter).HasPrecision(18, 3);
            entity.Property(e => e.AvailableStockTons).HasPrecision(18, 2);
            entity.Property(e => e.AvailableStockMeters).HasPrecision(18, 2);
            entity.HasIndex(e => e.SKU).IsUnique();
        });

        // Конфигурация Cart
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Items)
                .WithOne()
                .HasForeignKey(e => e.CartId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalDiscount).HasPrecision(18, 2);
            entity.HasIndex(e => e.TelegramUserId);
        });

        // Конфигурация CartItem
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.QuantityMeters).HasPrecision(18, 2);
            entity.Property(e => e.QuantityTons).HasPrecision(18, 3);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
        });

        // Конфигурация Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CustomerPhone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CustomerEmail).HasMaxLength(100);
            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.INN).HasMaxLength(12);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalDiscount).HasPrecision(18, 2);
            entity.HasMany(e => e.Items)
                .WithOne()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => e.TelegramUserId);
        });

        // Конфигурация OrderItem
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.QuantityMeters).HasPrecision(18, 2);
            entity.Property(e => e.QuantityTons).HasPrecision(18, 3);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
        });

        // Конфигурация Discount
        modelBuilder.Entity<Discount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MinQuantityTons).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);
            entity.Property(e => e.ProductType).HasMaxLength(100);
            entity.Property(e => e.Warehouse).HasMaxLength(100);
        });

        // Seed данные - скидки по умолчанию
        modelBuilder.Entity<Discount>().HasData(
            new Discount { Id = 1, MinQuantityTons = 10, DiscountPercent = 5, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Discount { Id = 2, MinQuantityTons = 50, DiscountPercent = 10, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Discount { Id = 3, MinQuantityTons = 100, DiscountPercent = 15, IsActive = true, CreatedAt = DateTime.UtcNow }
        );
    }
}
