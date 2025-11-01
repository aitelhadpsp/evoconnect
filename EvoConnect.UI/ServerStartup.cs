using System.Text.Json.Serialization;
using EvoConnect.Common;
using EvoConnect.Server.Data;
using EvoConnect.Server.Repository;
using EvoConnect.Server.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EvoConnect.UI
{
    public class ServerStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });

            var conn = AppData.ConnectionString();
            services.AddDbContext<ClinicDbContext>(options =>
            {
                options.UseFirebird(conn);
            }, ServiceLifetime.Scoped);

            // Register repositories
            services.AddScoped<IAppointmentsDA, AppointmentsDA>();
            services.AddScoped<IFinancialDA, FinancialDA>();
            services.AddScoped<IImagesDA, ImagesDA>();
            services.AddScoped<IPartnerDA, PartnerDA>();
            services.AddScoped<IPatientsDA, PatientsDA>();
            services.AddScoped<IPaymentDA, PaymentDA>();

            // Add health checks
            services.AddHealthChecks();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();
            app.UseCors();
            app.UseHttpsRedirection();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/device.xml");
            });
        }
    }
}