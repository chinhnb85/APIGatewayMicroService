using Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System;
using System.Threading.Tasks;

namespace APIGateway
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json")
                   //add configuration.json  
                   .AddJsonFile("configuration.json", optional: false, reloadOnChange: true)
                   .AddEnvironmentVariables();

            Configuration = builder.Build();
        }
        
        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //configure the jwt   
           // ConfigureJwtAuthService(services);

            services.AddOcelot(Configuration);
        }

        //oauth 2
        public void ConfigureJwtAuthService(IServiceCollection services)
        {
            var audienceConfig = Configuration.GetSection("Audience");
            var symmetricKeyAsBase64 = audienceConfig["Secret"];
            var signingKey = JwtSecurityKey.Create(symmetricKeyAsBase64);           

            var tokenValidationParameters = new TokenValidationParameters
            {
                // The signing key must match!  
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                // Validate the JWT Issuer (iss) claim  
                ValidateIssuer = true,
                ValidIssuer = audienceConfig["Iss"],

                // Validate the JWT Audience (aud) claim  
                ValidateAudience = true,
                ValidAudience = audienceConfig["Aud"],

                // Validate the token expiry  
                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero,

                RequireExpirationTime = true,
            };

            var jwtBearerEvents = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    var value = context.Request.Headers["Authorization"].ToString();
                    if (!string.IsNullOrEmpty(value) && value.ToLower().Contains("bearer"))
                    {
                        value = value.Trim().Substring(6).Trim();
                    }
                    if (!string.IsNullOrEmpty(value))
                    {
                        context.Token = value;
                    }
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    return Task.CompletedTask;
                }
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = tokenValidationParameters;
                options.Events = jwtBearerEvents;
            });
        }

        //don't use Task here  
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            //console logging
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            //app.UseAuthentication();

            app.UseOcelot().Wait();
        }
    }
}
