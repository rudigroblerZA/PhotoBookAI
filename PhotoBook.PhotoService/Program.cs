using Microsoft.EntityFrameworkCore;
using PhotoBook.PhotoService.Data;
using PhotoBook.PhotoService.Services;
using PhotoBook.ServiceDefaults;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

//var withApiVersioning = builder.Services.AddApiVersioning();

//builder.AddDefaultOpenApi(withApiVersioning);
//builder.AddDefaultOpenApi();
builder.Services.AddOpenApi();

// Database - PostgreSQL
builder.AddNpgsqlDbContext<PhotoDbContext>("photobook-db");

// Azure Blob Storage
builder.AddAzureBlobClient("photostore");

// Services
builder.Services.AddScoped<IPhotoUploadService, PhotoUploadService>();
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();
builder.Services.AddScoped<IMetadataExtractionService, MetadataExtractionService>();

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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PhotoDbContext>();
    db.Database.Migrate();
}

//app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.MapOpenApi();
//}

//app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
//app.UseDefaultOpenApi();
app.MapControllers();

app.MapScalarApiReference(options =>
{
    // Disable default fonts to avoid download unnecessary fonts
    options.DefaultFonts = false;
});
app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

//app.UseDefaultOpenApi();
app.MapDefaultEndpoints();

app.Run();













//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

//app.UseHttpsRedirection();

//var summaries = new[]
//{
//    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
//};

//var api = app.NewVersionedApi("Photo").MapGroup("api/photo").HasApiVersion(1.0);

//api.MapGet("/weatherforecast", () =>
//{
//    var forecast =  Enumerable.Range(1, 5).Select(index =>
//        new WeatherForecast
//        (
//            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//            Random.Shared.Next(-20, 55),
//            summaries[Random.Shared.Next(summaries.Length)]
//        ))
//        .ToArray();
//    return forecast;
//})
//.WithName("GetWeatherForecast");

//app.UseDefaultOpenApi();
//app.MapDefaultEndpoints();
//app.Run();

//record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
//{
//    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//}
