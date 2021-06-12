using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Event_Creator.models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Event_Creator.Other;
using Microsoft.AspNetCore.Identity;
using Event_Creator.Other.Interfaces;
using Event_Creator.Other.Services;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.Authorization;
using Event_Creator.Other.MiddleWare;
namespace Event_Creator
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var jwtTokenConfig = Configuration.GetSection("JwtConfig").Get<JwtConfig>();
            services.AddSingleton(jwtTokenConfig);

            services.AddSingleton<RsaSecurityKey>(provider => {
                RSA rsa = RSA.Create();
                rsa.ImportRSAPublicKey(
                    source: Convert.FromBase64String(jwtTokenConfig.PublicKey),
                    bytesRead: out int _
                );

                return new RsaSecurityKey(rsa);
            });
            IdentityModelEventSource.ShowPII = true;
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwt => {
                jwt.RequireHttpsMetadata = false;
                jwt.SaveToken = true;
                SecurityKey rsa = services.BuildServiceProvider().GetRequiredService<RsaSecurityKey>();
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    RequireSignedTokens=true,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtTokenConfig.Issuer,
                    ValidAudience = jwtTokenConfig.Audience,
                    IssuerSigningKey = rsa,
                    //ClockSkew = TimeSpan.FromSeconds(30)
                };
            });
            services.AddHttpContextAccessor();
            services.AddDbContext<ApplicationContext>(opts => opts.UseSqlServer(Configuration["ConnectionString:EventDB"]));
            services.AddControllers();
            services.AddScoped<IUserService,UserService>();
            services.AddScoped<IJwtService, JwtService>();
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<SecurityMiddleWare>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
