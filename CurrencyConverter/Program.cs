using CurrencyConverter.Business.Contracts;
using CurrencyConverter.Business.Core;
using CurrencyConverter.Business.Provider;
using CurrencyConverter.Common.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient(typeof(IExchangeRateService), typeof(ExchangeRateService));
builder.Services.AddTransient(typeof(IAuthService), typeof(AuthService));
builder.Services.AddHttpClient<FrankfurterProvider>();
builder.Services.Configure<SentrySettings>(builder.Configuration.GetSection("SentrySettings"));
builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("JWTSettings"));
builder.Services.AddSingleton<IExchangeRateProviderFactory, FrankfurterProviderFactory>();
builder.Services.AddTransient<IExchangeRateProvider, FrankfurterProvider>();
builder.Services.AddHttpContextAccessor();

// Read JWT settings from IOptions
var jwtSettingsSection = builder.Configuration.GetSection("JWTSettings");
builder.Services.Configure<JWTSettings>(jwtSettingsSection);
var jwtSettings = jwtSettingsSection.Get<JWTSettings>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ILS.Portal.WebApi - ", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                   {
                     new OpenApiSecurityScheme
                     {
                       Reference = new OpenApiReference
                       {
                         Type = ReferenceType.SecurityScheme,
                         Id = "Bearer"
                       }
                      },
                      new string[] { }
                    }
                  });
});
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
Log.Logger = new LoggerConfiguration()
    .CreateLogger();

builder.Host.UseSerilog();

// Register Serilog logger for DI
builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ILS.Portal.WebApi v1");
    c.RoutePrefix = string.Empty; // Swagger UI at https://localhost:<port>/
});// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
