using System.Text;
using System.Text.Json.Serialization;
using EvoConnect.Common;
using EvoConnect.Common.Models;
using EvoConnect.Server;
using EvoConnect.Server.Data;
using EvoConnect.Server.Initializers;
using EvoConnect.Server.Repository;
using EvoConnect.Server.Repository.Interfaces;
using EvoConnect.Server.Sync;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.UseUrls("http://0.0.0.0:6222");

var services = builder.Services;
services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

var conn = AppData.EfConnectionString();

services.AddDbContext<ClinicDbContext>(options =>
{
    options.UseFirebird(conn);
}, ServiceLifetime.Scoped);


var initializer = new KpiDatabaseInitializer(conn);

try
{
    await initializer.InitializeAsync();
    Console.WriteLine("KPI database ready");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to initialize KPI database: {ex.Message}");
}
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

services.AddEndpointsApiExplorer();

services.AddHttpContextAccessor();
services.AddScoped<IAppointmentsDA, AppointmentsDA>();
services.AddScoped<IFinancialDA, FinancialDA>();
services.AddScoped<IImagesDA, ImagesDA>();
services.AddScoped<IPartnerDA, PartnerDA>();
services.AddScoped<IPatientsDA, PatientsDA>();
services.AddScoped<IPaymentDA, PaymentDA>();
services.AddScoped<IActesRepository, ActesRepository>();
services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
services.AddScoped<IAuthRepository, AuthRepository>();
services.AddSingleton<IExecuterDA, ExecuterDA>();
services.AddSingleton<Synchronise>();
services.AddSingleton<CancelationConf>();
services.AddScoped<IKpiConfigRepository, KpiConfigRepository>();

services.AddHostedService<DataCollector>();
services.AddHostedService<UdpServicePublisher>();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(AppData.ImageDir()),
    RequestPath = "/image",
    ServeUnknownFileTypes = false,
    OnPrepareResponse = ctx =>
    {
        // Optional: Add caching headers for better performance
        ctx.Context.Response.Headers.Append(
            "Cache-Control",
            "public,max-age=3600"
        );

        // Optional: Enable range processing for large images
        ctx.Context.Response.Headers.Append(
            "Accept-Ranges",
            "bytes"
        );
    }
});
app.MapControllers();
app.UseRouting();

app.UseHttpsRedirection();

app.Run();