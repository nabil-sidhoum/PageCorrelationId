using System;
using System.Net.Http;
using System.Threading.Tasks;
using PageCorrelationId.Api.Utils.ApiCorrelationId;
using PageCorrelationId.Api.Utils.RequestLogging;
using PageCorrelationId.Api.Utils.Tests.Fakes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace PageCorrelationId.Api.Utils.Tests.RequestLogging
{
    public class RequestLoggingMiddlewareTests
    {
        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task SuccessfulRequest_ShouldLog_Information_WithStatusCode()
        {
            // Arrange
            FakeLogger<RequestLoggingMiddleware> fakeLogger = new();
            using IHost host = BuildHost(fakeLogger, throwException: false);
            await host.StartAsync();
            HttpClient client = host.GetTestClient();

            // Act
            await client.GetAsync("/api/test");

            // Assert — un LogInformation doit avoir été émis
            Assert.Contains(fakeLogger.Entries, entry => entry.Level == LogLevel.Information && entry.Message.Contains("200"));
        }

        [Fact]
        public async Task FailingRequest_ShouldLog_Error_WithStatusCode500()
        {
            // Arrange
            FakeLogger<RequestLoggingMiddleware> fakeLogger = new();
            using IHost host = BuildHost(fakeLogger, throwException: true);
            await host.StartAsync();
            HttpClient client = host.GetTestClient();

            // Act — l'exception est levée dans le pipeline, on l'attrape côté client
            try
            {
                await client.GetAsync("/api/test");
            }
            catch (Exception)
            {
                // Exception attendue — on veut juste vérifier le log
            }

            // Assert — un LogError doit avoir été émis avec 500
            Assert.Contains(fakeLogger.Entries, entry => entry.Level == LogLevel.Error && entry.Message.Contains("500"));
        }

        // ── Helpers privés ────────────────────────────────────────────────────

        private static IHost BuildHost(
            FakeLogger<RequestLoggingMiddleware> fakeLogger,
            bool throwException)
        {
            return new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseTestServer();
                    webBuilder.Configure(app =>
                    {
                        // ApiCorrelationIdMiddleware doit être avant RequestLoggingMiddleware
                        // car RequestLoggingMiddleware lit Items[ItemKey] pour le CID
                        app.UseMiddleware<ApiCorrelationIdMiddleware>();
                        app.UseMiddleware<RequestLoggingMiddleware>();

                        app.Run(ctx =>
                        {
                            if (throwException)
                            {
                                throw new InvalidOperationException("Erreur simulée pour le test");
                            }
                            return Task.CompletedTask;
                        });
                    });
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddLogging();
                        // Injecter le FakeLogger pour pouvoir l'inspecter
                        services.AddSingleton<ILogger<RequestLoggingMiddleware>>(fakeLogger);
                    });
                })
                .Build();
        }
    }
}