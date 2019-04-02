using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Gateway.Models;
using Gateway.Repositories;
using Gateway.Types;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace Gateway.Queries
{
    public class AuthenticationQuery : ObjectGraphType<object>
    {
        public AuthenticationQuery(IConfiguration configuration, IHttpClientFactory clientFactory, IHttpContextAccessor contextAccessor)
        {
            FieldAsync<BooleanGraphType>(
                "facebook",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>
                    {
                        Name = "token",
                        Description = "A unique short lived access token.",
                    }),
                resolve: async resolveContext =>
                {
                    var client = clientFactory.CreateClient();

                    var query = new Dictionary<string, string>
                    {
                        ["grant_type"] = "fb_exchange_token",
                        ["client_id"] = configuration.GetValue<string>("FACEBOOK_APP_ID"),
                        ["client_secret"] = configuration.GetValue<string>("FACEBOOK_APP_SECRET"),
                        ["fb_exchange_token"] = resolveContext.GetArgument<string>("token")
                    };

                    var response = await client
                        .GetAsync(QueryHelpers
                            .AddQueryString("https://graph.facebook.com/oauth/access_token", query));

                    if (response.IsSuccessStatusCode)
                    {
                        var content = JObject.Parse(await response.Content.ReadAsStringAsync());
                        var access_token = content.GetValue("access_token").Value<string>();
                        var appsecret_proof = "";

                        // Generate app proof
                        using (var hasher = new HMACSHA256(Encoding.UTF8.GetBytes(query["client_secret"])))
                        {
                            var bytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(access_token));

                            // Convert byte array to a string   
                            var builder = new StringBuilder();

                            for (int i = 0; i < bytes.Length; i++)
                            {
                                builder.Append(bytes[i].ToString("x2"));
                            }

                            appsecret_proof = builder.ToString();
                        }

                        query = new Dictionary<string, string>
                        {
                            ["access_token"] = access_token,
                            ["appsecret_proof"] = appsecret_proof,
                            ["fields"] = "id,name",
                        };

                        response = await client.GetAsync(QueryHelpers
                            .AddQueryString("https://graph.facebook.com/v3.2/me", query));

                        content = JObject.Parse(await response.Content.ReadAsStringAsync());

                        var claims = new List<Claim>() {
                            new Claim(ClaimTypes.NameIdentifier, content["id"].Value<string>()),
                            new Claim(ClaimTypes.Name, content["name"].Value<string>()),
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            //AllowRefresh = <bool>,
                            // Refreshing the authentication session should be allowed.

                            //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                            // The time at which the authentication ticket expires. A 
                            // value set here overrides the ExpireTimeSpan option of 
                            // CookieAuthenticationOptions set with AddCookie.

                            //IsPersistent = true,
                            // Whether the authentication session is persisted across 
                            // multiple requests. When used with cookies, controls
                            // whether the cookie's lifetime is absolute (matching the
                            // lifetime of the authentication ticket) or session-based.

                            //IssuedUtc = <DateTimeOffset>,
                            // The time at which the authentication ticket was issued.

                            //RedirectUri = <string>
                            // The full path or absolute URI to be used as an http 
                            // redirect response value.
                        };

                        
                        await contextAccessor.HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);


                        //await userContext.SignInAsync(AuthenticationSchemes.Basic.ToString(), new ClaimsPrincipal(new ClaimsIdentity(claims, "Facebook")));

                        /* httpContext.Request.Headers.Add("access_token", access_token);
                        httpContext.Request.Headers.Add("appsecret_proof", appsecret_proof); */

                        return true;
                    }

                    return false;
                }
            );
        }
    }
}