var builder = DistributedApplication.CreateBuilder(args);



var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithPgWeb()
    .WithDataVolume()
    .AddDatabase("photobook-db");

var redis = builder.AddRedis("redis");

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator()
    .AddBlobs("photostore");

var rabbitmq = builder.AddRabbitMQ("messaging");








var photoService = builder.AddProject<Projects.PhotoBook_PhotoService>("photo-service")
    .WithExternalHttpEndpoints()
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(postgres)
    .WaitFor(postgres)
    .WithReference(storage)
    .WaitFor(storage)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithHttpHealthCheck("/health");

var renderService = builder.AddProject<Projects.PhotoBook_RenderService>("render-service")
    .WithExternalHttpEndpoints()
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(postgres)
    .WaitFor(postgres)
    .WithReference(storage)
    .WaitFor(storage)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithHttpHealthCheck("/health");
var designService = builder.AddProject<Projects.PhotoBook_DesignService>("design-service")
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(postgres)
    .WaitFor(postgres)
    .WithReference(storage)
    .WaitFor(storage)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");
var aiLayoutService = builder.AddProject<Projects.PhotoBook_AILayoutService>("ai-layout-service")
    .WithExternalHttpEndpoints()
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(postgres)
    .WaitFor(postgres)
    .WithReference(storage)
    .WaitFor(storage)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.PhotoBook_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(redis)
    .WaitFor(redis);

builder.Build().Run();
