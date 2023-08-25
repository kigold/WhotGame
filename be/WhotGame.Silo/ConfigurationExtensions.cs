using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenIddict.Validation.AspNetCore;
using WhotGame.Core.Data.Models;
using WhotGame.Core.Data.Context;
using static OpenIddict.Abstractions.OpenIddictConstants;
using WhotGame.Core.Data.Repositories;
using WhoteGame.Services;

namespace WhotGame.Silo
{
    public static class ConfigurationExtensions
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddTransient<IRepository<Game>, Repository<Game>>();
            services.AddTransient<IRepository<User>, Repository<User>>();
            services.AddScoped<IGameService, GameService>();
            services.AddScoped<ICardService, CardService>();
            //services.AddScoped<IRealTimeClient, RealTimeClient>();
        }

        public static void AddEFDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default");
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(AppDbContext).FullName));
            });
        }

        public static void UseAppSwagger(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "WhotGame V1"); });
        }

        public static void MapAppControllers(this WebApplication app)
        {
            app.MapControllers().RequireAuthorization(new AuthorizationOptions { AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme });
            app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
            app.MapDefaultControllerRoute();
        }

        public static void AddAppSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "WhotGame API",
                    Description = "WhotGame API",
                    Version = "v1"
                });

                c.AddSecurityDefinition("BearerAuth", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "BearerAuth"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = options.Authority = "http://localhost:5287/";
                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = Claims.Name,
                    RoleClaimType = Claims.Role,
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/gameHub")))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.User.RequireUniqueEmail = false;
            })
            .AddUserStore<UserStore<User, Role, AppDbContext, long, IdentityUserClaim<long>, UserRole, IdentityUserLogin<long>, IdentityUserToken<long>, RoleClaim>>()
            .AddSignInManager()
            .AddUserManager<UserManager<User>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(24);
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = Claims.Role;
                options.SignIn.RequireConfirmedAccount = false;
                // configure more options if necessary...
            });

            services.AddControllersWithViews();

            services.AddDbContext<AppDbContext>(options =>
            {
                // Configure Entity Framework Core to use Microsoft SQL Server.
                options.UseSqlServer(configuration.GetConnectionString("Default"));

                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need to replace the default OpenIddict entities.
                options.UseOpenIddict<long>();
            });

            services.AddOpenIddict()
                // Register the OpenIddict core components.
                .AddCore(options =>
                {
                    // Configure OpenIddict to use the Entity Framework Core stores and models.
                    // Note: call ReplaceDefaultEntities() to replace the default entities.
                    options.UseEntityFrameworkCore()
                           .UseDbContext<AppDbContext>()
                           .ReplaceDefaultEntities<long>();
                })

                // Register the OpenIddict server components.
                .AddServer(options =>
                {
                    options.RegisterScopes(Scopes.Email,
                        Scopes.Profile,
                        Scopes.Address,
                        Scopes.Phone,
                        Scopes.OfflineAccess,
                        Scopes.OpenId
                    );

                    // Enable the token endpoint.
                    options.SetTokenEndpointUris("/connect/token")
                           .SetAuthorizationEndpointUris("/connect/authorize")
                           .SetUserinfoEndpointUris("/connect/userinfo");

                    options.AllowPasswordFlow();
                    options.AcceptAnonymousClients();
                    options.AllowRefreshTokenFlow();

                    // Register the signing and encryption credentials.
                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate()
                           .DisableAccessTokenEncryption();

                    // Register scopes (permissions)
                    options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.OfflineAccess, Scopes.OpenId, Scopes.Phone, "api");

                    // Register the ASP.NET Core host and configure the ASP.NET Core options.
                    options.UseAspNetCore()
                           .EnableTokenEndpointPassthrough()
                           .EnableAuthorizationEndpointPassthrough()
                           .DisableTransportSecurityRequirement();
                })

                // Register the OpenIddict validation components.
                .AddValidation(options =>
                {
                    // Import the configuration from the local OpenIddict server instance.
                    options.UseLocalServer();

                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();
                });
            services.AddHostedService<Worker>();
        }
    }
}

public class AuthorizationOptions : Microsoft.AspNetCore.Authorization.IAuthorizeData
{
    public string? Policy { get; set; }
    public string? Roles { get; set; }
    public string? AuthenticationSchemes { get; set; }
}
