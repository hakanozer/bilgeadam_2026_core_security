using Microsoft.EntityFrameworkCore;
using RestApi.Data;
using RestApi.Utils;
using RestApi.Extensions;
using RestApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// AutoMapper ekle
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<AuthService, AuthService>();    
builder.Services.AddControllers();

// Add CORS policies
builder.Services.AddCustomCors();

// JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();
 
// Configure Cors policy
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
// Global exception handling middleware
app.UseMiddleware<GlobalExceptionMiddleware>();
app.MapControllers();
app.Run();
