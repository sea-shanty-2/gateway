using System.Text;
using Gateway.GraphQL.Extensions;
using Gateway.GraphQL.Services;
using Gateway.MongoDB;
using Gateway.MongoDB.Extensions;
using GraphQL;
using GraphQL.Http;
using GraphQL.Server.Ui.Playground;
using GraphQL.Server.Ui.Voyager;
using GraphQL.Types;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Gateway.GraphQL
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = Environment.IsProduction();
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = Configuration.GetValue<string>("JWT_ISSUER"),
                        ValidateAudience = false,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("JWT_KEY")))
                    };
                });

            services
                .AddSingleton<IDependencyResolver, DependencyResolver>()
                .AddSingleton<IDocumentExecuter, DocumentExecuter>()
                .AddSingleton<IDocumentWriter, DocumentWriter>()
                .AddSingleton<ISchema, MainSchema>()
                .AddSingleton<JWTService>()
                .AddRelayGraphTypes()
                .AddGraphQLAuth()
                .AddGraphTypes();

            if (Environment.IsDevelopment())
            {
                services
                    .AddSingleton<SeedService>()
                    .AddMongoDBRepositories();
            }
            else
            {
                services
                    .AddMongoDBRepositories(Configuration.GetConnectionString("Envue"));
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app
                .UseAuthentication()
                .UseGraphQL(options =>
                {
                    options.Path = "/";
                    options.ExposeExceptions = Environment.IsDevelopment();
                })
                .UseGraphQLPlayground(new GraphQLPlaygroundOptions() { Path = "/playground", GraphQLEndPoint = "/" })
                .UseGraphQLVoyager(new GraphQLVoyagerOptions() { Path = "/voyager", GraphQLEndPoint = "/" })
                .UseDefaultFiles()
                .UseStaticFiles();
        }
    }
}
