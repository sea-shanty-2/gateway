using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Gateway.Services
{
    public class JWTService
    {
        private readonly IConfiguration _configuration;
        public JWTService(IConfiguration Configuration) {
            _configuration = Configuration;
        }

        public string CreateAccessToken(string userId, string name)
        {
            var now = DateTime.UtcNow;
            
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.UniqueName, userId),
                new Claim(ClaimTypes.Name, name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, now.ToString(), ClaimValueTypes.Integer64),
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("JWT_KEY"))),
                SecurityAlgorithms.HmacSha256);
                
            var expires = now.AddMinutes(_configuration.GetValue<double>("JWT_EXPIRES"));
            var jwt = new JwtSecurityToken(claims: claims, issuer: _configuration.GetValue<string>("JWT_ISSUER"), notBefore: now, expires: expires, signingCredentials: credentials);
            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            return token;
        }
    }
}