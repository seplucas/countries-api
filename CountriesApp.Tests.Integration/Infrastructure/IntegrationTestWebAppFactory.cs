using CountriesApp.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;
using Testcontainers.Redis;

namespace CountriesApp.Tests.Integration.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly IConfiguration _configuration;
    private readonly MsSqlContainer _msSqlContainer;
    private readonly RedisContainer _redisContainer;

    public IntegrationTestWebAppFactory()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Testing.json", optional: false)
            .Build();

        var sqlImage = _configuration["Testcontainers:SqlServer:Image"]!;
        var sqlPassword = _configuration["Testcontainers:SqlServer:Password"]!;
        var redisImage = _configuration["Testcontainers:Redis:Image"]!;

        _msSqlContainer = new MsSqlBuilder()
            .WithImage(sqlImage)
            .WithPassword(sqlPassword)
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage(redisImage)
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<CountriesAppDbContext>));
            services.RemoveAll(typeof(CountriesAppDbContext));

            services.AddDbContext<CountriesAppDbContext>(options =>
            {
                options.UseSqlServer(_msSqlContainer.GetConnectionString());
            });

            services.RemoveAll(typeof(Microsoft.Extensions.Caching.StackExchangeRedis.RedisCacheOptions));
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = _redisContainer.GetConnectionString();
                options.InstanceName = "TestCountriesApp:";
            });

            services.AddAuthentication("Test")
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _msSqlContainer.StartAsync();
        }
        catch (NotSupportedException ex) when (ex.Message.Contains("sqlcmd binary could not be found"))
        {
        }
        
        await _redisContainer.StartAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CountriesAppDbContext>();
        
        var maxRetries = 30;
        var retryCount = 0;
        while (retryCount < maxRetries)
        {
            try
            {
                await dbContext.Database.MigrateAsync();
                break;
            }
            catch (Exception)
            {
                retryCount++;
                if (retryCount >= maxRetries)
                    throw;
                await Task.Delay(1000);
            }
        }
    }

    public new async Task DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
