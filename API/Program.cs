using API.Middleware;
using Application.Services;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

try
{
    // Add services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "E-Commerce Inventory API",
            Version = "v1",
            Description = "A RESTful API for managing e-commerce inventory with JWT authentication, built with .NET Core and PostgreSQL."
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your valid JWT token.\n\nExample: \"eyJhbGciOiJIUzI1NiIsInR5cCI6...\""
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        // Include XML comments for Swagger
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        }
        else
        {
            Console.WriteLine($"Warning: XML documentation file not found at {xmlPath}. Swagger groups may not display correctly.");
        }
    });

    // Database
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Dependency Injection
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();

    // JWT Authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                ClockSkew = TimeSpan.FromMinutes(5)
            };
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    context.HandleResponse();
                    return context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "Forbidden: Authentication required" }));
                }
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("ApiAccess", policy => policy.RequireClaim(ClaimTypes.NameIdentifier));
    });

    // CORS for Swagger testing
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    });

    var app = builder.Build();

    // Configure middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce Inventory API v1");
            c.RoutePrefix = string.Empty; // Serves Swagger at the root in development
        });
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    app.MapControllers();

    app.Run();
}
catch (Exception)
{
    throw;
}