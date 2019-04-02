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
using AspNetCore.Identity.Mongo;
using Gateway.Models;
using Gateway.Extensions;

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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services
                .AddIdentityMongoDbProvider<Account>();

            services
                .AddAuthentication()
                .AddFacebook(options =>
                {
                    options.ClientId = Configuration.GetValue<string>("FACEBOOK_APP_ID");
                    options.ClientSecret = Configuration.GetValue<string>("FACEBOOK_APP_SECRET");
                });

            if (Environment.IsDevelopment())
            {
                services.AddSingleton<IDatabase, TestDatabase>();
            }
            else
            {
                services.AddSingleton<IDatabase, Database>();
            }

            services
                .AddHttpClient()
                .AddHttpContextAccessor()
                .AddSingleton<IDependencyResolver>(s => new FuncDependencyResolver(s.GetRequiredService))
                .AddSingleton<IDocumentExecuter, DocumentExecuter>()
                .AddSingleton<IDocumentWriter, DocumentWriter>()
                .AddSingleton<IRepository, Repository>()
                .AddSingleton<ISchema, MainSchema>()
                .AddGraphTypes()
                .AddRelayGraphTypes();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2); ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app
                .UseAuthentication()
                .UseGraphQLPlayground(new GraphQLPlaygroundOptions() { Path = "/playground", GraphQLEndPoint = "/" })
                .UseDefaultFiles()
                .UseStaticFiles()
                .UseCookiePolicy()
                .UseMvc();
        }
    }
}
