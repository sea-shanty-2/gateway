using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Gateway.Repositories;
using Gateway.Schemas;
using GraphQL;
using GraphQL.Http;
using GraphQL.Types;
using GraphQL.Server;
using GraphQL.Server.Ui.Playground;
using Gateway.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Gateway.Models;
using Gateway.Extensions;
using Gateway.Services;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Gateway.Requirements;
using GraphQL.Validation;
using GraphQL.Validation.Complexity;

namespace Gateway
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
                    cfg.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = Configuration.GetValue<string>("JWT_ISSUER"),
                        ValidateAudience = false,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("JWT_KEY")))
                    };
                });

            if (Environment.IsDevelopment() || Environment.IsStaging())
                services.AddSingleton<IDatabase, TestDatabase>();
            else
                services.AddSingleton<IDatabase, Database>();

            services
                .AddSingleton<IDependencyResolver, GraphQLDependencyResolver>()
                .AddSingleton<IDocumentExecuter, DocumentExecuter>()
                .AddSingleton<IDocumentWriter, DocumentWriter>()
                .AddSingleton<IRepository, Repository>()
                .AddSingleton<ISchema, MainSchema>()
                .AddSingleton<JWTService>()
                .AddRelayGraphTypes()
                .AddGraphQLAuth()
                .AddGraphTypes();
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
                .UseDefaultFiles()
                .UseStaticFiles();
        }
    }
}
