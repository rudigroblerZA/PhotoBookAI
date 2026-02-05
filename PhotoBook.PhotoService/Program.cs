using Microsoft.EntityFrameworkCore;
using PhotoBook.PhotoService.Data;
using PhotoBook.PhotoService.Services;
using PhotoBook.ServiceDefaults;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.AddNpgsqlDbContext<PhotoDbContext>("photobook-db");
builder.AddAzureBlobClient("photostore");

builder.Services.AddScoped<IPhotoUploadService, PhotoUploadService>();
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();
builder.Services.AddScoped<IMetadataExtractionService, MetadataExtractionService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PhotoDbContext>();
    db.Database.Migrate();
}

app.MapOpenApi();

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.MapControllers();

app.MapScalarApiReference(options =>
{
    options.DefaultFonts = false;
});
app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

app.MapDefaultEndpoints();

app.Run();