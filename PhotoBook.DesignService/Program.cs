using Microsoft.EntityFrameworkCore;
using PhotoBook.DesignService.Data;
using PhotoBook.DesignService.Services;
using PhotoBook.ServiceDefaults;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Database
builder.AddNpgsqlDbContext<DesignDbContext>("photobook-db");

// Blob Storage for generated previews
builder.AddAzureBlobClient("photostore");

// HTTP Client for calling PhotoService
builder.Services.AddHttpClient("PhotoService", client =>
{
    client.BaseAddress = new Uri("http://photo-service");
});

// Services
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<IDesignService, DesignService>();
builder.Services.AddScoped<IFilterService, FilterService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IPageLayoutService, PageLayoutService>();
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


// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DesignDbContext>();
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
//app.UseDefaultOpenApi();

app.Run();