using Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
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
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile("appsettings.{env.EnvironmentName}.json", optional: true)
                   //add configuration.json  
                   .AddJsonFile("configuration.json", optional: true, reloadOnChange: true);

            if (env.IsEnvironment("Development"))
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }
        
        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //configure the jwt   
           ConfigureJwtAuthService(services);

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
                    var err = "";

                    if (context.Exception.GetType() == typeof(SecurityTokenValidationException))
                    {
                        err = "invalid token";
                    }
                    else if (context.Exception.GetType() == typeof(SecurityTokenInvalidIssuerException))
                    {
                        err = "invalid issuer";
                    }
                    else if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        err = "token expired";
                    }

                    var resp = new
                    {
                        error = err,
                        status = 401
                    };

                    context.Response.WriteAsync(JsonConvert.SerializeObject(resp, Formatting.Indented));

                    return Task.FromResult<object>(0);

                    //return Task.CompletedTask;
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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //console logging
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            app.UseExceptionHandler(appBuilder =>
            {
                appBuilder.Use(async (context, next) =>
                {
                    var error = context.Features[typeof(IExceptionHandlerFeature)] as IExceptionHandlerFeature;

                    //when authorization has failed, should retrun a json message to client
                    if (error != null && error.Error is SecurityTokenExpiredException)
                    {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                        {
                            State = "Unauthorized",
                            Msg = "token expired"
                        }));
                    }
                    //when orther error, retrun a error message json to client
                    else if (error != null && error.Error != null)
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                        {
                            State = "Internal Server Error",
                            Msg = error.Error.Message
                        }));
                    }
                    //when no error, do next.
                    else await next();
                });
            });

            app.UseAuthentication();

            app.UseOcelot().Wait();
        }
    }
}
