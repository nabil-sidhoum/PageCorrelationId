using System;
using PageCorrelationId.Api.Utils.ApiCorrelationId;
using PageCorrelationId.Api.Utils.RequestLogging;
using PageCorrelationId.Application;
using PageCorrelationId.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace PageCorrelationId.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // ── IoC ──────────────────────────────────
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplication();
            services.AddInfrastructure();

            services.AddControllers();
            services.AddEndpointsApiExplorer();

            string websiteUrl = Configuration.GetValue<string>("WebsiteUrl") ?? throw new InvalidOperationException("La clé de configuration 'WebsiteUrl' est manquante. Vérifiez appsettings.json ou les variables d'environnement.");

            services.AddCors(options =>
            {
                options.AddPolicy("Website Demo", builder => builder.WithOrigins(websiteUrl)
                                                                    .AllowAnyHeader()
                                                                    .AllowAnyMethod());
            });

            services.AddSwaggerGen();
        }

        // ── Pipeline HTTP ─────────────────────────
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseApiCorrelationId();
            app.UseRequestLoggingMiddleware();

            app.UseRouting();

            app.UseCors("Website Demo");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}