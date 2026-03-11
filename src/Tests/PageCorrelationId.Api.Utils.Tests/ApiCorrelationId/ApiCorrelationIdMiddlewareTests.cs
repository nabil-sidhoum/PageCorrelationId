using System.Net.Http;
using System.Threading.Tasks;
using PageCorrelationId.Api.Utils.ApiCorrelationId;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace PageCorrelationId.Api.Utils.Tests.ApiCorrelationId
{
    public class ApiCorrelationIdMiddlewareTests
    {
        // ── Cas 1 : header présent dans la requête entrante ───────────────────

        [Fact]
        public async Task Request_WithCorrelationHeader_ShouldReuseCid()
        {
            // Arrange
            using IHost host = BuildHost();
            await host.StartAsync();
            HttpClient client = host.GetTestClient();

            string expectedCid = "my-business-correlation-id";

            HttpRequestMessage request = new(HttpMethod.Get, "/api/test");
            request.Headers.Add(ApiCorrelationIdMiddleware.HeaderName, expectedCid);

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert — le même CID doit être renvoyé dans la réponse
            Assert.True(response.Headers.Contains(ApiCorrelationIdMiddleware.HeaderName));
            string actual = string.Join("",
                response.Headers.GetValues(ApiCorrelationIdMiddleware.HeaderName));
            Assert.Equal(expectedCid, actual);
        }

        // ── Cas 2 : header absent → nouveau CID généré ───────────────────────

        [Fact]
        public async Task Request_WithoutCorrelationHeader_ShouldGenerateNewCid()
        {
            // Arrange
            using IHost host = BuildHost();
            await host.StartAsync();
            HttpClient client = host.GetTestClient();

            // Act — aucun header X-Correlation-ID
            HttpResponseMessage response = await client.GetAsync("/api/test");

            // Assert — un CID doit quand même être présent dans la réponse
            Assert.True(response.Headers.Contains(ApiCorrelationIdMiddleware.HeaderName));
            string generatedCid = string.Join("",
                response.Headers.GetValues(ApiCorrelationIdMiddleware.HeaderName));
            Assert.False(string.IsNullOrWhiteSpace(generatedCid));
        }

        // ── Cas 3 : deux requêtes sans header → CIDs différents ──────────────

        [Fact]
        public async Task TwoRequests_WithoutHeader_ShouldGenerateDifferentCids()
        {
            // Arrange
            using IHost host = BuildHost();
            await host.StartAsync();
            HttpClient client = host.GetTestClient();

            // Act
            HttpResponseMessage response1 = await client.GetAsync("/api/test");
            HttpResponseMessage response2 = await client.GetAsync("/api/test");

            string cid1 = string.Join("",
                response1.Headers.GetValues(ApiCorrelationIdMiddleware.HeaderName));
            string cid2 = string.Join("",
                response2.Headers.GetValues(ApiCorrelationIdMiddleware.HeaderName));

            // Assert — chaque requête sans CID entrant doit avoir son propre CID
            Assert.NotEqual(cid1, cid2);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static IHost BuildHost()
        {
            return new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseTestServer();
                    webBuilder.Configure(app =>
                    {
                        app.UseMiddleware<ApiCorrelationIdMiddleware>();
                        app.Run(ctx => Task.CompletedTask);
                    });
                    webBuilder.ConfigureServices(services => services.AddLogging());
                })
                .Build();
        }
    }
}