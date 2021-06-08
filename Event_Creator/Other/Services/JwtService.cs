using Event_Creator.models;
using Event_Creator.Other.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Event_Creator.Other.Services
{
    public class JwtService : IJwtService
    {
        private readonly ApplicationContext _appContext;
        private readonly JwtConfig _jwtConfig;
        private readonly RsaSecurityKey _rsaSecurityKey;
        public JwtService(ApplicationContext app, JwtConfig config , RsaSecurityKey rsa)
        {
            _appContext = app;
            _jwtConfig = config;
            _rsaSecurityKey = rsa;
        }

        public string JwtTokenGenerator(long userId, string jti)
        {
            var privateKey = Convert.FromBase64String(_jwtConfig.PrivateKey);
            using RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(privateKey, out _);
            var cryptoProviderFactory = new CryptoProviderFactory();
            cryptoProviderFactory.CacheSignatureProviders = false;
            var signingCredentials = new SigningCredentials(
                key: new RsaSecurityKey(rsa),
                algorithm: SecurityAlgorithms.RsaSha256
            )
            { CryptoProviderFactory=cryptoProviderFactory};
            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();

            var jwt = new JwtSecurityToken(
                   //audience: _jwtConfig.Issuer,
                   //issuer: _jwtConfig.Issuer,
                   claims: new Claim[]
                   {
                        new Claim(JwtRegisteredClaimNames.Iat, unixTimeSeconds.ToString(),ClaimValueTypes.Integer64),
                        new Claim(JwtRegisteredClaimNames.Jti,jti),
                        new Claim("uid",userId.ToString())
                   },
                   expires: now.AddSeconds(40),
                   signingCredentials: signingCredentials
            );
            string token = new JwtSecurityTokenHandler().WriteToken(jwt);
            return token;
        }


        public async Task<RefreshToken> GenerateRefreshToken(string jwtId, long userId , string ip)
        {
            User user = await  _appContext.Users.SingleOrDefaultAsync(x => x.UserId == userId);
            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
            return new RefreshToken()
            {
                JwtTokenId = jwtId,
                user = user,
                expirationTime = unixTimeSeconds + 90,
                Revoked = false,
                Token = Guid.NewGuid().ToString(),
                ipAddress = ip
            };
        }


        public async Task<AuthResponse> RefreshToken(RefreshRequest refreshRequest, string ip)
        {
            RefreshToken refreshToken = await _appContext.refreshTokens.SingleOrDefaultAsync(x => x.Token.Equals(refreshRequest.refreshToken));
            if (refreshToken != null) await _appContext.Entry(refreshToken).Reference(x => x.user).LoadAsync();
            if (refreshToken == null)
            {
                return new AuthResponse()
                {
                    JwtAccessToken = null,
                    RefreshToken = null,
                    success = false,
                    ErrorList = Errors.NotFoundRefreshToken,
                    statusCode = 404
                };
            }
            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();

            if(refreshToken.expirationTime < unixTimeSeconds)
            {
                _appContext.refreshTokens.Remove(refreshToken);
                await _appContext.SaveChangesAsync();
                return new AuthResponse()
                {
                    RefreshToken = null,
                    ErrorList = Errors.refreshTokenExpired,
                    JwtAccessToken = null,
                    statusCode = 403,
                    success = false
                };
            }

            if (refreshToken.Revoked == true)
            {
                return new AuthResponse()
                {
                    ErrorList = Errors.RevokedToken,
                    RefreshToken = null,
                    JwtAccessToken = null,
                    statusCode = 403,
                    success = false
                };
            }

            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireSignedTokens = true,
                RequireExpirationTime = true,
                ValidateLifetime = false,
                IssuerSigningKey = _rsaSecurityKey
            };

            try
            {
                var jwtToken = new JwtSecurityTokenHandler().ValidateToken(refreshRequest.jwtAccessToken, parameters, out SecurityToken validatedToken);

                var jti = jwtToken.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                var utcExpiryDate = long.Parse(jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                if(jti != refreshToken.JwtTokenId)
                {
                    return new AuthResponse()
                    {
                        JwtAccessToken = null,
                        RefreshToken = null,
                        statusCode = 401,
                        success = false,
                        ErrorList = Errors.InvalidJwtToken
                    };
                }

                if (utcExpiryDate > unixTimeSeconds)
                {
                    return new AuthResponse()
                    {
                        ErrorList = Errors.NotExpiredToken,
                        JwtAccessToken = null,
                        RefreshToken = null,
                        statusCode = 403,
                        success = false
                    };
                }

              
                string jwtId = Guid.NewGuid().ToString();
                string newJwtAccessToken = JwtTokenGenerator(refreshToken.user.UserId, jwtId);
                RefreshToken newrefreshToken = await GenerateRefreshToken(jwtId, refreshToken.user.UserId,ip);
                await _appContext.refreshTokens.AddAsync(newrefreshToken);
                _appContext.refreshTokens.Remove(refreshToken);
                await _appContext.SaveChangesAsync();
                return new AuthResponse()
                {
                    ErrorList = null,
                    RefreshToken = newrefreshToken.Token,
                    JwtAccessToken = newJwtAccessToken,
                    statusCode = 200,
                    success = true
                };
            }
            catch
            {
                return new AuthResponse()
                {
                    RefreshToken = null,
                    JwtAccessToken = null,
                    statusCode = 403,
                    success = false,
                    ErrorList = Errors.InvalidJwtToken
                };
            }
        }




    }
}
