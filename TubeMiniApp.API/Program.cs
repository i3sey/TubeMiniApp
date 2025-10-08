using Microsoft.EntityFrameworkCore;
using TubeMiniApp.API.Data;
using TubeMiniApp.API.Middleware;
using TubeMiniApp.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Tube Mini App API",
        Version = "v1",
        Description = "API для Telegram Mini App по заказу трубной продукции - РадиоХак 2.0",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Трубная металлургическая компания"
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString) || connectionString == "UseInMemory")
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("TubeMiniAppDb"));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// Register services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<IDataSyncService, DataSyncService>();
builder.Services.AddScoped<IDataImportService, DataImportService>();
builder.Services.AddScoped<ITelegramNotificationService, TelegramNotificationService>();

// Register HttpClient for Telegram notifications
builder.Services.AddHttpClient<ITelegramNotificationService, TelegramNotificationService>();

// CORS configuration for Telegram Mini App
builder.Services.AddCors(options =>
{
    options.AddPolicy("TelegramPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Rate limiting for DDoS protection
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<Microsoft.AspNetCore.Http.HttpContext, string>(context =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Request.Headers.Host.ToString(),
            factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();

// Seed database with sample data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    
    // Импорт данных из testData если база пустая
    var importService = scope.ServiceProvider.GetRequiredService<IDataImportService>();
    await importService.ImportInitialDataAsync();
    
    // Если данных всё еще нет (папка testData не найдена), добавляем демо-данные
    if (!context.Products.Any())
    {
        context.Products.AddRange(
            new TubeMiniApp.API.Models.Product
            {
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
                SKU = "TUBE-EW-57-3.5-ST3"
            },
            new TubeMiniApp.API.Models.Product
            {
                Warehouse = "Склад Екатеринбург",
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
                SKU = "TUBE-SM-76-5-20"
            },
            new TubeMiniApp.API.Models.Product
            {
                Warehouse = "Склад Москва",
                ProductType = "Труба электросварная",
                Diameter = 108,
                WallThickness = 4,
                GOST = "ГОСТ 10704-91",
                SteelGrade = "Ст3сп",
                PricePerTon = 67000,
                WeightPerMeter = 10.42m,
                AvailableStockTons = 300,
                AvailableStockMeters = 28792,
                LastPriceUpdate = DateTime.UtcNow,
                SKU = "TUBE-EW-108-4-ST3"
            }
        );
        context.SaveChanges();
    }
    
    // Добавляем демо-скидки если их нет
    if (!context.Discounts.Any())
    {
        context.Discounts.AddRange(
            new TubeMiniApp.API.Models.Discount
            {
                MinQuantityTons = 0.01m, // Очень низкий порог - от 10 кг
                DiscountPercent = 3,
                ProductType = null, // Для всех типов
                Warehouse = null, // Для всех складов
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new TubeMiniApp.API.Models.Discount
            {
                MinQuantityTons = 0.05m, // От 50 кг
                DiscountPercent = 5,
                ProductType = null,
                Warehouse = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new TubeMiniApp.API.Models.Discount
            {
                MinQuantityTons = 0.1m, // От 100 кг
                DiscountPercent = 7,
                ProductType = null,
                Warehouse = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new TubeMiniApp.API.Models.Discount
            {
                MinQuantityTons = 0.5m, // От 500 кг
                DiscountPercent = 10,
                ProductType = null,
                Warehouse = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new TubeMiniApp.API.Models.Discount
            {
                MinQuantityTons = 1m, // От 1 тонны
                DiscountPercent = 15,
                ProductType = null,
                Warehouse = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
        context.SaveChanges();
    }
}
// Configure the HTTP request pipeline
// Swagger доступен всегда для демонстрации
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tube Mini App API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCors("TelegramPolicy");
app.UseRateLimiter();
app.UseMiddleware<TelegramAuthorizationMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make the implicit Program class public for testing
public partial class Program { }
