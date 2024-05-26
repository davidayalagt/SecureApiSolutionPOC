using System.Threading.Tasks;
using ProtectedApi;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:5001";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

WebApplication app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/register", (RegisterRequest request) => Task.FromResult(Results.Ok("User registered successfully"))).RequireAuthorization();

app.Run();