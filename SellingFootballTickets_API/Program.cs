using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SellingFootballTickets_API.Data;
using SellingFootballTickets_API.middleware;
using SellingFootballTickets_API.Models;
using SellingFootballTickets_API.Service;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Server
builder.Services.AddDbContext<ServiceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var key = builder.Configuration["Jwt:Key"];

// JWT
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key)),
            RoleClaimType = ClaimTypes.Role
        };
    });
//policy config
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Personal", policy => policy.RequireRole("user","admin").RequireClaim(ClaimTypes.Name));
    options.AddPolicy("adminOnly", policy => policy.RequireRole("admin"));
    options.AddPolicy("userOnly", policy => policy.RequireRole("user"));
    options.AddPolicy("adminOrUser", policy => policy.RequireRole("admin", "user"));
});

// bind SMTP config
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));

// register OTP service
builder.Services.AddTransient<IOTPService, ServiceEmail>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Selling Football Tickets API",
        Version = "v1"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use custom exception middleware
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapRazorPages();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();
app.Run();