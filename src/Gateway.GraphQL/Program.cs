using System;
using System.Net.Http;
using FirebaseAdmin;
using Gateway.GraphQL.Services;
using Gateway.Models;
using Gateway.Repositories;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Serilog;
using Serilog.Events;

namespace Gateway.GraphQL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .Enrich.FromLogContext()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Hour)
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                var host = CreateWebHostBuilder(args).Build();

                using (var scope = host.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var environment = services.GetService<IHostingEnvironment>();
                    if (!environment.IsProduction())
                    {
                        var context = services.GetRequiredService<SeedService>();
                        context.Seed();
                    }
                    else
                    {
                        var repository = services.GetRequiredService<IRepository<Broadcast>>();
                        var configuration = services.GetRequiredService<IConfiguration>();
                        
                        repository.UpdateRangeAsync(
                            x => x.Expired == false,
                            new Broadcast
                            {
                                Expired = true,
                                Activity = DateTime.UtcNow
                            }
                        ).Wait();

                        var client = new HttpClient
                        {
                            BaseAddress = new Uri(configuration.GetValue<string>("CLUSTERING_URL"))
                        };

                        client.GetAsync("/data/clear").Wait();
                    }

                }

                host.Run();

            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog();
    }
}