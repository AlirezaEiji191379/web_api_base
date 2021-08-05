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
using Microsoft.AspNetCore.Http;

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

        public async Task<string> JwtTokenGenerator(long userId, string jti)
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
            User user = await _appContext.Users.SingleOrDefaultAsync(x => x.UserId == userId);
            var jwt = new JwtSecurityToken(
                   audience: _jwtConfig.Issuer,
                   issuer: _jwtConfig.Issuer,
                   claims: new Claim[]
                   {
                        new Claim(JwtRegisteredClaimNames.Iat, unixTimeSeconds.ToString(),ClaimValueTypes.Integer64),
                        new Claim(JwtRegisteredClaimNames.Jti,jti),
                        new Claim("uid",userId.ToString()),
                        new Claim(ClaimTypes.Role,user.role.ToString())
                   },
                   expires: now.AddSeconds(900),/////////////////
                   signingCredentials: signingCredentials
            );
            string token = new JwtSecurityTokenHandler().WriteToken(jwt);
            return token;
        }


        public async Task<RefreshToken> GenerateRefreshToken(string jwtId, long userId , HttpContext httpContext,bool refresh,int priorityNum)
        {
            User user = await  _appContext.Users.SingleOrDefaultAsync(x => x.UserId == userId);
            await _appContext.Entry(user).Collection(x => x.RefreshTokens).LoadAsync();
            int priority = 0;
            if (refresh == false) priority = user.RefreshTokens.Count + 1;
            else priority = priorityNum;
            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
            return new RefreshToken()
            {
                JwtTokenId = jwtId,
                user = user,
                expirationTime = unixTimeSeconds + 604800,
                Revoked = false,
                Token = Guid.NewGuid().ToString(),
                ipAddress = httpContext.Connection.RemoteIpAddress.ToString(),
                UserAgent = httpContext.Request.Headers.FirstOrDefault(x => x.Key.Contains("User-Agent")).ToString(),
                Priority = priority
            };
        }


        public async Task<AuthResponseMobile> RefreshTokenMobile(RefreshRequest refreshRequest, HttpContext httpContext)
        {
            RefreshToken refreshToken = await _appContext.refreshTokens.SingleOrDefaultAsync(x => x.Token.Equals(refreshRequest.refreshToken));
            if (refreshToken != null) await _appContext.Entry(refreshToken).Reference(x => x.user).LoadAsync();
            if (refreshToken == null)
            {
                return new AuthResponseMobile()
                {
                    JwtAccessToken = null,
                    RefreshToken = null,
                    ErrorList = Errors.NotFoundRefreshToken,
                    statusCode = 404
                };
            }
            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
            if (refreshToken.Revoked == true)
            {
                return new AuthResponseMobile()
                {
                    ErrorList = Errors.RevokedToken,
                    RefreshToken = null,
                    JwtAccessToken = null,
                    statusCode = 403,
                };
            }

            if (refreshToken.expirationTime < unixTimeSeconds)
            {
                _appContext.refreshTokens.Remove(refreshToken);
                await _appContext.SaveChangesAsync();
                return new AuthResponseMobile()
                {
                    RefreshToken = null,
                    ErrorList = Errors.refreshTokenExpired,
                    JwtAccessToken = null,
                    statusCode = 403,
                };
            }


            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _jwtConfig.Issuer,
                ValidAudience = _jwtConfig.Audience,
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
                    return new AuthResponseMobile()
                    {
                        JwtAccessToken = null,
                        RefreshToken = null,
                        statusCode = 401,
                        ErrorList = Errors.InvalidJwtToken
                    };
                }

                if (utcExpiryDate > unixTimeSeconds)
                {
                    return new AuthResponseMobile()
                    {
                        ErrorList = Errors.NotExpiredToken,
                        JwtAccessToken = null,
                        RefreshToken = null,
                        statusCode = 403,
                    };
                }

              
                string jwtId = Guid.NewGuid().ToString();
                string newJwtAccessToken =await JwtTokenGenerator(refreshToken.user.UserId, jwtId);
                RefreshToken newrefreshToken = await GenerateRefreshToken(jwtId, refreshToken.user.UserId,httpContext,true,refreshToken.Priority);
                await _appContext.refreshTokens.AddAsync(newrefreshToken);
                _appContext.refreshTokens.Remove(refreshToken);
                await _appContext.SaveChangesAsync();
                return new AuthResponseMobile()
                {
                    ErrorList = null,
                    RefreshToken = newrefreshToken.Token,
                    JwtAccessToken = newJwtAccessToken,
                    statusCode = 200,
                };
            }
            catch
            {
                return new AuthResponseMobile()
                {
                    RefreshToken = null,
                    JwtAccessToken = null,
                    statusCode = 403,
                    ErrorList = Errors.InvalidJwtToken
                };
            }
        }


        public async Task<AuthResponseWeb> RefreshTokenWeb(HttpContext httpContext)
        {
            if(httpContext.Request.Cookies["refresh-token"]==null && httpContext.Request.Cookies["access-token"] == null)
            {
                return new AuthResponseWeb()
                {
                    statusCode = 401,
                    ErrorList = "رفرش توکن نامعتبر است"
                };
            }
            String refreshTokenCookie = httpContext.Request.Cookies["refresh-token"].ToString();
            String accessTokenCookie = httpContext.Request.Cookies["access-token"].ToString();
            RefreshToken refreshToken = await _appContext.refreshTokens.SingleOrDefaultAsync(x => x.Token.Equals(refreshTokenCookie));
            if (refreshToken != null) await _appContext.Entry(refreshToken).Reference(x => x.user).LoadAsync();
            if (refreshToken == null)
            {
                return new AuthResponseWeb()
                {
                    ErrorList = Errors.NotFoundRefreshToken,
                    statusCode = 404
                };
            }
            var now = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
            if (refreshToken.Revoked == true)
            {
                return new AuthResponseWeb()
                {
                    ErrorList = Errors.RevokedToken,
                    statusCode=403
                };
            }

            if (refreshToken.expirationTime < unixTimeSeconds)
            {
                _appContext.refreshTokens.Remove(refreshToken);
                await _appContext.SaveChangesAsync();
                return new AuthResponseWeb()
                {
                    ErrorList = Errors.refreshTokenExpired,
                    statusCode = 403,
                };
            }


            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _jwtConfig.Issuer,
                ValidAudience = _jwtConfig.Audience,
                RequireSignedTokens = true,
                RequireExpirationTime = true,
                ValidateLifetime = false,
                IssuerSigningKey = _rsaSecurityKey
            };

            try
            {
                var jwtToken = new JwtSecurityTokenHandler().ValidateToken(accessTokenCookie, parameters, out SecurityToken validatedToken);

                var jti = jwtToken.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                var utcExpiryDate = long.Parse(jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                if (jti != refreshToken.JwtTokenId)
                {
                    return new AuthResponseWeb()
                    {
                        statusCode = 401,
                        ErrorList = Errors.InvalidJwtToken
                    };
                }

                if (utcExpiryDate > unixTimeSeconds)
                {
                    return new AuthResponseWeb()
                    {
                        ErrorList = Errors.NotExpiredToken,
                        statusCode = 403,
                    };
                }


                string jwtId = Guid.NewGuid().ToString();
                string newJwtAccessToken = await JwtTokenGenerator(refreshToken.user.UserId, jwtId);
                RefreshToken newrefreshToken = await GenerateRefreshToken(jwtId, refreshToken.user.UserId, httpContext, true, refreshToken.Priority);
                await _appContext.refreshTokens.AddAsync(newrefreshToken);
                _appContext.refreshTokens.Remove(refreshToken);
                await _appContext.SaveChangesAsync();

                httpContext.Response.Cookies.Append("access-token", newJwtAccessToken, new CookieOptions()
                {
                    /// securing cookies! with secure!
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax
                });

                httpContext.Response.Cookies.Append("refresh-token", newrefreshToken.Token, new CookieOptions()
                {
                    /// securing cookies! with secure!
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax
                }
                );

                return new AuthResponseWeb()
                {
                    ErrorList = null,
                    statusCode = 200,
                };
            }
            catch
            {
                return new AuthResponseWeb()
                {
                    statusCode = 403,
                    ErrorList = Errors.InvalidJwtToken
                };
            }
        }

        public long getUserIdFromJwt(HttpContext httpContext)
        {
            String stream = "";
            if (httpContext.Request.Headers.ContainsKey("Authorization"))
            {
                var authorizationHeader = httpContext.Request.Headers.Single(x => x.Key == "Authorization");
                stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            }else if (httpContext.Request.Cookies["access-token"]!=null)
            {
                stream = httpContext.Request.Cookies["access-token"].ToString();
            }
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var uid = tokenS.Claims.First(claim => claim.Type == "uid").Value;
            return Convert.ToInt64(uid);
        }

        public string getJwtIdFromJwt(HttpContext httpContext)
        {
            String stream = "";
            if (httpContext.Request.Headers.ContainsKey("Authorization"))
            {
                var authorizationHeader = httpContext.Request.Headers.Single(x => x.Key == "Authorization");
                stream = authorizationHeader.Value.Single(x => x.Contains("Bearer")).Split(" ")[1];
            }
            else if (httpContext.Request.Cookies["access-token"] != null)
            {
                stream = httpContext.Request.Cookies["access-token"].ToString();
            }
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = jsonToken as JwtSecurityToken;
            var jti = tokenS.Claims.First(claim => claim.Type == "jti").Value;
            return jti.ToString();
        }


    }
}
