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
using Event_Creator.Other.Filters;

namespace Event_Creator
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public readonly string corsPolicyName = "cors";
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
                jwt.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["access-token"];
                        return Task.CompletedTask;
                    },
                };

            });
            services.AddHttpContextAccessor();
            services.AddAntiforgery( options=>
            {
                options.HeaderName = "X-CSRF-Header";
                options.Cookie.Name = "CSRF-TOKEN";
                options.Cookie.HttpOnly = false;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            }
            );
            services.AddCors(options => {
                options.AddPolicy(
                    name:corsPolicyName,builder => {
                        builder.WithOrigins("http://localhost:3000");
                    }
                    );
            
            });
            services.AddDbContext<ApplicationContext>(opts => opts.UseSqlServer(Configuration["ConnectionString:EventDB"]));
            services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            services.AddScoped<IUserService,UserService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<CsrfActionFilter>();
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(corsPolicyName);
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
