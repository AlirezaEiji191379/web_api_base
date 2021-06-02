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
        public JwtService(ApplicationContext app , JwtConfig config)
        {
            _appContext = app;
            _jwtConfig = config;
        }

        public string JwtTokenGenerator(long userId , string jti)
        {
            var privateKey = _jwtConfig.PrivateKey;
            using RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
            var signingCredentials = new SigningCredentials(
                key: new RsaSecurityKey(rsa),
                algorithm: SecurityAlgorithms.RsaSha256
            );
            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();

            var jwt = new JwtSecurityToken(
                   audience: _jwtConfig.Issuer,
                   issuer: _jwtConfig.Issuer,
                   claims: new Claim[]
                   {
                        new Claim(JwtRegisteredClaimNames.Iat, unixTimeSeconds.ToString(),ClaimValueTypes.Integer64),
                        new Claim(JwtRegisteredClaimNames.Jti,jti),
                        new Claim("uid",userId.ToString())
                   },
                   expires:now.AddMinutes(5),
                   signingCredentials: signingCredentials
            ) ;
            string token = new JwtSecurityTokenHandler().WriteToken(jwt);
            return token;
        }


        public async Task<RefreshToken> GenerateRefreshToken(string jwtId , long userId)
        {
            User user = await Task.Run(() => {
                return _appContext.Users.SingleOrDefault(x=> x.UserId == userId);
            });
            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
            return new RefreshToken()
            {
                JwtTokenId = jwtId,
                user=user,
                expirationTime=unixTimeSeconds+604800,
                Revoked=false,
                Token= Guid.NewGuid().ToString()
            };
        }


        public async Task<AuthResponse> RefreshToken(RefreshRequest refreshRequest)
        {
            RefreshToken refreshToken = await Task.Run(() => {
                return _appContext.refreshTokens.SingleOrDefault(x => x.Token == refreshRequest.refreshToken);
            });

            if(refreshToken== null)
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

            using RSA rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(_jwtConfig.PublicKey), out _);
             var parameters = new TokenValidationParameters
            {
                ValidIssuer = _jwtConfig.Issuer,
                ValidAudience = _jwtConfig.Audience,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                RequireSignedTokens = true,
                RequireExpirationTime = true,
                //ValidateLifetime = true,
                IssuerSigningKey = new RsaSecurityKey(rsa)
            };

            try
            {
                var jwtToken = new JwtSecurityTokenHandler().ValidateToken(refreshRequest.jwtAccessToken, parameters, out SecurityToken validatedToken);

                var jti = jwtToken.Claims.SingleOrDefault(x => x.Type== JwtRegisteredClaimNames.Jti).Value;
                
                if(refreshToken.JwtTokenId != jti)
                {
                    return new AuthResponse() {
                        RefreshToken = null,
                        JwtAccessToken = null,
                        statusCode = 403,
                        ErrorList =Errors.InvalidJwtToken,
                        success = false
                    };
                }

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

                if (refreshToken.Revoked == true)
                {
                    return new AuthResponse()
                    {
                        ErrorList = Errors.RevokedToken,
                        RefreshToken = null,
                        JwtAccessToken = null,
                        statusCode = 403,
                        success=false
                    };
                }

                string jwtId = Guid.NewGuid().ToString();
                string newJwtAccessToken = this.JwtTokenGenerator(refreshToken.user.UserId,jwtId);
                RefreshToken newRefreshToken = await this.GenerateRefreshToken(jwtId,refreshToken.user.UserId);

                return new AuthResponse()
                {
                    ErrorList = null,
                    RefreshToken = newRefreshToken.Token,
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
