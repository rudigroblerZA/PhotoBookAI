using Microsoft.EntityFrameworkCore;
using PhotoBook.AILayoutService.Data;
using PhotoBook.AILayoutService.Services;
using PhotoBook.ServiceDefaults;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// Database for caching layout suggestions
builder.AddNpgsqlDbContext<LayoutDbContext>("photobook-db");

// Redis for caching
builder.AddRedisClient("redis");

// HTTP Clients
builder.Services.AddHttpClient("PhotoService", client =>
{
    client.BaseAddress = new Uri("http://photo-service");
});

builder.Services.AddHttpClient("ClaudeAPI", client =>
{
    client.BaseAddress = new Uri("https://api.anthropic.com");
    client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
});

// Services
builder.Services.AddScoped<ILayoutGeneratorService, ClaudeLayoutService>();
builder.Services.AddScoped<IPhotoAnalysisService, PhotoAnalysisService>();
builder.Services.AddScoped<ILayoutScoringService, LayoutScoringService>();
builder.Services.AddScoped<ILayoutCacheService, LayoutCacheService>();
builder.Services.AddMemoryCache();

// CORS for frontend
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowFrontend", policy =>
//    {
//        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
//              .AllowAnyMethod()
//              .AllowAnyHeader()
//              .AllowCredentials();
//    });
//});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LayoutDbContext>();
    db.Database.Migrate();
}

app.MapDefaultEndpoints();

//if (app.Environment.IsDevelopment())
//{
app.MapOpenApi();
//}

//app.UseCors("AllowFrontend");
app.UseHttpsRedirection();


app.MapScalarApiReference(options =>
{
    // Disable default fonts to avoid download unnecessary fonts
    options.DefaultFonts = false;
});
app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();


app.MapControllers();

app.Run();