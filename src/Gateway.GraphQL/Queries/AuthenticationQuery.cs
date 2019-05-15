using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Gateway.GraphQL.Services;
using Gateway.Models;
using Gateway.Repositories;
using GraphQL;
using GraphQL.Authorization;
using GraphQL.Types;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gateway.GraphQL.Queries
{
    public class AuthenticationQuery : ObjectGraphType<object>
    {
        public AuthenticationQuery(JWTService jwtservice, IRepository<Account> repository)
        {
            FieldAsync<StringGraphType>(
                "facebook",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>>
                    {
                        Name = "token",
                        Description = "A unique short lived access token from facebook.",
                    }),
                resolve: async context =>
                {
                    var client = new HttpClient
                    {
                        BaseAddress = new Uri("https://graph.facebook.com/v3.2/")
                    };
                    var accessToken = context.GetArgument<string>("token");
                    var response = await client.GetAsync($"me?access_token={accessToken}&fields=id");

                    if (!response.IsSuccessStatusCode)
                    {
                        /// TODO: Deserialize JSON and instantiate execution error properly.
                        /// https://graphql.org/learn/validation/ 
                        var error = await response.Content.ReadAsStringAsync();
                        context.Errors.Add(new ExecutionError("error"));
                        return default;
                    }

                    var fbid = JsonConvert
                        .DeserializeObject<JObject>(await response.Content.ReadAsStringAsync())
                        .GetValue("id")
                        .ToString();

                    // Check if user account exists for the given facebook id
                    var account = await repository.FindAsync(x => x.FacebookId == fbid, context.CancellationToken);

                    // Else create a new user account with the given facebook id
                    if (account == default)
                        account = await repository.AddAsync(new Account
                        {
                            FacebookId = fbid,
                            DisplayName = "Jendal"
                        }, context.CancellationToken);

                    // Create and return an access token for the account
                    return jwtservice.CreateAccessToken(account.Id, account.DisplayName);
                }
            );
        }
    }
}