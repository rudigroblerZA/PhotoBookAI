using MassTransit;
using Microsoft.EntityFrameworkCore;
using PhotoBook.RenderService.Data;
using PhotoBook.RenderService.Services;
using PhotoBook.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);


// Database for render job tracking
builder.AddNpgsqlDbContext<RenderDbContext>("photobook-db");

// Blob Storage for PDFs
builder.AddAzureBlobClient("photostore");

// Messaging
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<RenderBookConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
    https://url.za.m.mimecastprotect.com/s/jd9QCmw0LzfYG5Gpf9swHRsWSw?domain=cfg.host("messaging");
        cfg.ConfigureEndpoints(context);
    });
});

// HTTP Clients
builder.Services.AddHttpClient("DesignService", client =>
{
    client.BaseAddress = new Uri("http://design-service");
    client.Timeout = TimeSpan.FromMinutes(2);
});

builder.Services.AddHttpClient("PhotoService", client =>
{
    client.BaseAddress = new Uri("http://photo-service");
    client.Timeout = TimeSpan.FromMinutes(2);
});

// Services
builder.Services.AddScoped<IPdfRenderService, QuestPdfRenderService>();
builder.Services.AddScoped<IPageCompositionService, PageCompositionService>();
builder.Services.AddScoped<IPhotoFetchService, PhotoFetchService>();
builder.Services.AddScoped<IRenderJobService, RenderJobService>();
builder.Services.AddMemoryCache();

// Background services
builder.Services.AddHostedService<RenderJobCleanupService>();

// CORS
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

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RenderDbContext>();
    db.Database.Migrate();
}

app.MapDefaultEndpoints();

//if (app.Environment.IsDevelopment())
//{
app.MapOpenApi();
//}


app.UseHttpsRedirection();

//app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

