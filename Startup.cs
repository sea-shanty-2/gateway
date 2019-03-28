namespace Gateway
{
    using System;
    using Boxed.AspNetCore;
    using CorrelationId;
    using Gateway.Constants;
    using GraphQL.Server;
    using GraphQL.Server.Ui.Playground;
    using GraphQL.Server.Ui.Voyager;
    using Gateway.Schemas;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Gateway.Data;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.UI;
    using Microsoft.AspNetCore.Authentication.JwtBearer;

    /// <summary>
    /// The main start-up class for the application.
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IHostingEnvironment hostingEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration, where key value pair settings are stored. See
        /// http://docs.asp.net/en/latest/fundamentals/configuration.html</param>
        /// <param name="hostingEnvironment">The environment the application is running under. This can be Development,
        /// Staging or Production by default. See http://docs.asp.net/en/latest/fundamentals/environments.html</param>
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            this.configuration = configuration;
            this.hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Configures the services to add to the ASP.NET Core Injection of Control (IoC) container. This method gets
        /// called by the ASP.NET runtime. See
        /// http://blogs.msdn.com/b/webdev/archive/2014/06/17/dependency-injection-in-asp-net-vnext.aspx
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddCorrelationIdFluent()
                .AddCustomCaching()
                .AddCustomOptions(this.configuration)
                .AddCustomRouting()
                .AddCustomResponseCompression()
                .AddCustomStrictTransportSecurity()
                .AddCustomHealthChecks()
                .AddHttpContextAccessor()
                .AddMvcCore()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                    .AddAuthorization()
                    .AddJsonFormatters()
                    .AddCustomJsonOptions(this.hostingEnvironment)
                    .AddCustomCors()
                    .AddCustomMvcOptions(this.hostingEnvironment);


            services
                .AddAuthentication()
                .AddFacebook(options =>
                {
                    options.AppId = configuration["FACEBOOK_APP_ID"];
                    options.AppSecret = configuration["FACEBOOK_APP_SECRET"];
                });

            services
                .AddCustomGraphQL(this.hostingEnvironment)
                .AddIf(!this.hostingEnvironment.IsDevelopment(), x => x.AddSingleton<IDatabase, Database>())
                .AddIf(this.hostingEnvironment.IsDevelopment(), x => x.AddSingleton<IDatabase, TestDatabase>())
                .AddCustomGraphQLAuthorization()
                .AddProjectRepositories()
                .AddProjectSchemas()
                .BuildServiceProvider();
        }
        /// <summary>
        /// Configures the application and HTTP request pipeline. Configure is called after ConfigureServices is
        /// called by the ASP.NET runtime.
        /// </summary>
        public void Configure(IApplicationBuilder application) =>
            application
                // Pass a GUID in a X-Correlation-ID HTTP header to set the HttpContext.TraceIdentifier.
                .UseCorrelationId(new CorrelationIdOptions())
                .UseForwardedHeaders()
                .UseResponseCompression()
                .UseCors(CorsPolicyName.AllowAny)
                .UseIf(
                    !this.hostingEnvironment.IsDevelopment(),
                    x => x.UseHsts())
                .UseIf(
                    this.hostingEnvironment.IsDevelopment(),
                    x => x
                        .UseDeveloperErrorPages()
                        // Add the GraphQL Playground UI to try out the GraphQL API
                        .UseGraphQLPlayground(new GraphQLPlaygroundOptions()
                        {
                            Path = "/playground",
                            GraphQLEndPoint = "/"
                        })
                        // Add the GraphQL Voyager UI to navigate the GraphQL API
                        .UseGraphQLVoyager(new GraphQLVoyagerOptions()
                        {
                            Path = "/voyager",
                            GraphQLEndPoint = "/"
                        }))
                .UseHealthChecks("/status")
                .UseHealthChecks("/status/self", new HealthCheckOptions() { Predicate = _ => false })
                .UseAuthentication()
                .UseStaticFilesWithCacheControl()
                .UseWebSockets()
                // Use the GraphQL subscriptions in the specified schema and make them available at /
                .UseGraphQLWebSockets<MainSchema>("/")
                // Use the specified GraphQL schema and make them available at /
                .UseGraphQL<MainSchema>("/");
    }
}