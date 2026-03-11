using System;
using PageCorrelationId.Site.CorrelationId;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace PageCorrelationId.Site
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment WebHostEnvironment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
        }

        // ── IoC ──────────────────────────────────
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // Requis par SiteCorrelationIdDelegatingHandler pour accéder à HttpContext
            services.AddHttpContextAccessor();

            // DelegatingHandler transient (une instance par requête)
            services.TryAddTransient<SiteCorrelationIdDelegatingHandler>();

            // HttpClient nommé "ApiClient" — toujours avec le handler de corrélation
            services.AddHttpClient("ApiClient", client =>
            {
                string apiBase = Configuration["ApiSettings:BaseUrl"];
                if (string.IsNullOrWhiteSpace(apiBase))
                {
                    throw new InvalidOperationException("La clé de configuration 'ApiSettings:BaseUrl' est manquante. Vérifiez appsettings.json ou les variables d'environnement.");
                }

                client.BaseAddress = new Uri(apiBase);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddHttpMessageHandler<SiteCorrelationIdDelegatingHandler>();
        }

        // ── Pipeline HTTP ─────────────────────────
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // ► Middleware de corrélation AVANT UseRouting
            app.UseMiddleware<SiteCorrelationIdMiddleware>();

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Dashboard}/{action=Index}/{id?}");
            });
        }
    }
}