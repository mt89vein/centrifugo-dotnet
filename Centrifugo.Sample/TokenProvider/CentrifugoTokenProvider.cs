using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Centrifugo.Client.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Centrifugo.Sample.TokenProvider
{
    public class CentrifugoTokenProvider : ICentrifugoTokenProvider
    {
        private readonly AuthOptions _authOptions;

        public CentrifugoTokenProvider(IOptions<AuthOptions> authOptions)
        {
            _authOptions = authOptions.Value;
        }

        public Task<string> GenerateTokenAsync(
            string clientId,
            string? clientProvidedInfo = null
        )
        {
            var now = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                new Claim("sub", clientId)
            };

            if (_authOptions.TokenLifetime.HasValue)
            {
                var unixTime = DateTimeOffset.UtcNow.Add(_authOptions.TokenLifetime.Value).ToUnixTimeSeconds();
                claims.Add(new Claim("exp", unixTime.ToString()));
            }

            if (clientProvidedInfo != null)
            {
                claims.Add(new Claim("info", clientProvidedInfo));
            }

            var jwt = new JwtSecurityToken(
                issuer: _authOptions.Issuer,
                audience: _authOptions.Audience,
                notBefore: now,
                claims: claims,
                expires: _authOptions.TokenLifetime.HasValue
                    ? now.Add(_authOptions.TokenLifetime.Value)
                    : now,
                signingCredentials: new SigningCredentials(GetSymmetricSecurityKey(_authOptions.SecretKey), SecurityAlgorithms.HmacSha256)
            );

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(jwt));

            static SymmetricSecurityKey GetSymmetricSecurityKey(string secretKey)
            {
                return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            }
        }
    }
}