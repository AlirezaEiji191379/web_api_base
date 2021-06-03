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


        public async Task<RefreshToken> GenerateRefreshToken(string jwtId, long userId)
        {
            User user = await Task.Run(() => {
                return _appContext.Users.SingleOrDefault(x => x.UserId == userId);
            });
            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
            return new RefreshToken()
            {
                JwtTokenId = jwtId,
                user = user,
                expirationTime = unixTimeSeconds + 90,
                Revoked = false,
                Token = Guid.NewGuid().ToString()
            };
        }


        public async Task<AuthResponse> RefreshToken(RefreshRequest refreshRequest)
        {
            RefreshToken refreshToken = await Task.Run(() => {
                return _appContext.refreshTokens.SingleOrDefault(x => x.Token == refreshRequest.refreshToken);
            });
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

            //using RSA rsa = RSA.Create();
            //rsa.ImportRSAPublicKey(Convert.FromBase64String(_jwtConfig.PublicKey), out _);
            var parameters = new TokenValidationParameters
            {
                ////ValidIssuer = _jwtConfig.Issuer,
                ////ValidAudience = _jwtConfig.Audience,
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

            //if (refreshToken.JwtTokenId != jti)
            //{
            //    return new AuthResponse()
            //    {
            //        RefreshToken = null,
            //        JwtAccessToken = null,
            //        statusCode = 403,
            //        ErrorList = Errors.InvalidJwtToken,
            //        success = false
            //    };
            //}


            var utcExpiryDate = long.Parse(jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
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
                refreshToken= await GenerateRefreshToken(jwtId, refreshToken.user.UserId);
            //await Task.Run(() =>
            //{
            //    _appContext.refreshTokens.Update(refreshToken);
            //});

                return new AuthResponse()
                {
                    ErrorList = null,
                    RefreshToken = refreshToken.Token,
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
