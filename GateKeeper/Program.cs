using GateKeeper;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/honeypot.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add Authentication and Authorization
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "http://localhost:57000";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

// Add YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

WebApplication app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// Honeypot endpoint
app.MapPost("/RegisterMobileUser", () =>
{
    // This is a honeypot endpoint
    // Log the access attempt and return a random fake response
    Log.Warning("Honeypot accessed at {Time}", DateTime.UtcNow);

    string[] responses = new[] { "User registered", "User already exists" };
    string randomResponse = responses[new Random().Next(responses.Length)];
    return Results.Ok(randomResponse);
}).RequireAuthorization();

// Middleware for processing requests
app.UseGatekeeper();

app.MapReverseProxy();

app.Run();