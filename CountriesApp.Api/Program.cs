using CountriesApp.Api.Middleware;
using CountriesApp.Application.Interfaces;
using CountriesApp.Application.Services;
using CountriesApp.Domain.Interfaces;
using CountriesApp.Infrastructure.Data;
using CountriesApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using CountriesApp.Application.Mapping;
using CountriesApp.Application.DTOs.Countries;
using CountriesApp.Application.DTOs.Cities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

//database
builder.Services.AddDbContext<CountriesAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Configuration"];
    options.InstanceName = builder.Configuration["Redis:InstanceName"];
});

builder.Services.Configure<Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions>(options =>
{
    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
});

//auth
var domain = builder.Configuration["Auth0:Domain"];
var audience = builder.Configuration["Auth0:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{domain}/";
        options.Audience = audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuers = [$"https://{domain}/", $"https://{domain}"]
        };
    });

builder.Services.AddAuthorization();

//rate limiting
var rateLimitConfig = builder.Configuration.GetSection("RateLimiting");
var permitLimit = rateLimitConfig.GetValue<int>("PermitLimit");
var window = rateLimitConfig.GetValue<int>("Window");
var queueLimit = rateLimitConfig.GetValue<int>("QueueLimit");

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = permitLimit,
                QueueLimit = queueLimit,
                Window = TimeSpan.FromSeconds(window)
            }));
    
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken: token);
    };
});

builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();

builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<ICityService, CityService>();

builder.Services.AddAutoMapper(cfg => 
{
    cfg.AddProfile<CountryProfile>();
    cfg.AddProfile<CityProfile>();
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Countries API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
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
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
//logs
app.UseMiddleware<LogMiddleware>();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// countries
app.MapGet("/countries", async (ICountryService countryService, string? search = null, int page = 1, int pageSize = 10) =>
{
    var result = await countryService.GetCountriesAsync(search, page, pageSize);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error!.Message);
}).RequireAuthorization();

app.MapGet("/countries/{id:guid}", async (ICountryService countryService, Guid id) =>
{
    var result = await countryService.GetCountryByIdAsync(id);
    return result.IsSuccess && result.Value != null
        ? Results.Ok(result.Value)
        : Results.NotFound(result.Error?.Message ?? "Country not found");
}).RequireAuthorization();

app.MapPost("/countries", async (ICountryService countryService, CountryCreateDto dto) =>
{
    var result = await countryService.CreateCountryAsync(dto);
    return result.IsSuccess && result.Value != null
        ? Results.Created($"/countries/{result.Value.Id}", result.Value)
        : Results.BadRequest(result.Error?.Message ?? "Failed to create country");
}).RequireAuthorization();

app.MapPut("/countries/{id:guid}", async (ICountryService countryService, Guid id, CountryUpdateDto dto) =>
{
    dto.Id = id;
    var result = await countryService.UpdateCountryAsync(dto);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error!.Message);
}).RequireAuthorization();

app.MapDelete("/countries/{id:guid}", async (ICountryService countryService, Guid id) =>
{
    var result = await countryService.DeleteCountryAsync(id);
    return result.IsSuccess ? Results.NoContent() : Results.NotFound(result.Error!.Message);
}).RequireAuthorization();

// cities
app.MapGet("/cities", async (ICityService cityService, string? search = null, Guid? countryId = null, int page = 1, int pageSize = 10) =>
{
    var result = await cityService.GetCitiesAsync(search, countryId, page, pageSize);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error!.Message);
}).RequireAuthorization();

app.MapGet("/cities/{id:guid}", async (ICityService cityService, Guid id) =>
{
    var result = await cityService.GetCityByIdAsync(id);
    return result.IsSuccess && result.Value != null
        ? Results.Ok(result.Value)
        : Results.NotFound(result.Error?.Message ?? "City not found");
}).RequireAuthorization();

app.MapPost("/cities", async (ICityService cityService, CityCreateDto dto) =>
{
    var result = await cityService.CreateCityAsync(dto);
    return result.IsSuccess && result.Value != null
        ? Results.Created($"/cities/{result.Value.Id}", result.Value)
        : Results.BadRequest(result.Error?.Message ?? "Failed to create city");
}).RequireAuthorization();

app.MapPut("/cities/{id:guid}", async (ICityService cityService, Guid id, CityUpdateDto dto) =>
{
    dto.Id = id;
    var result = await cityService.UpdateCityAsync(dto);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error!.Message);
}).RequireAuthorization();

app.MapDelete("/cities/{id:guid}", async (ICityService cityService, Guid id) =>
{
    var result = await cityService.DeleteCityAsync(id);
    return result.IsSuccess ? Results.NoContent() : Results.NotFound(result.Error!.Message);
}).RequireAuthorization();

app.Run();

public abstract partial class Program;