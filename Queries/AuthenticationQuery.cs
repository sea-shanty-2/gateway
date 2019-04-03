using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Gateway.Models;
using Gateway.Repositories;
using Gateway.Services;
using Gateway.Types;
using GraphQL;
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

namespace Gateway.Queries
{
    public class AuthenticationQuery : ObjectGraphType<object>
    {
        public AuthenticationQuery(JWTService jwtservice, IRepository repository)
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

                    // Should be transformed to error request
                    if (!response.IsSuccessStatusCode)
                        return default;

                    var fbuser = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());

                    var account = new Account
                    {
                        FacebookId = fbuser.GetValue("id").ToString(),
                        DisplayName = "Doe"
                    };

                    account = await repository.UpdateOneAsync<Account>(x => x.FacebookId == account.FacebookId, new BsonDocument { { "$setOnInsert", account.ToBsonDocument() } }, true, context.CancellationToken);

                    return jwtservice.CreateAccessToken(account.Id, account.DisplayName);
                }
            );
        }
    }
}