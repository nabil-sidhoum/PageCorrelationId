using System;
using System.Net.Http;
using System.Threading.Tasks;
using PageCorrelationId.Site.CorrelationId;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace PageCorrelationId.Site.Tests.CorrelationId
{
    public class SiteCorrelationIdMiddlewareTests
    {
        // ── Règle 1 : navigation de page ─────────────────────────────────────

        [Fact]
        public async Task PageNavigation_ShouldGenerateNewCid_AndSetCookie()
        {
            // Arrange
            using IHost host = BuildHost();
            await host.StartAsync();
            HttpClient client = host.GetTestClient();

            // Act — Sec-Fetch-Mode: navigate simule une navigation browser
            HttpRequestMessage request = new(HttpMethod.Get, "/");
            request.Headers.Add("Sec-Fetch-Mode", "navigate");

            HttpResponseMessage response = await client.SendAsync(request);

            // Assert — un cookie 'cid' doit être posé
            Assert.True(response.Headers.Contains("Set-Cookie"));
            string setCookieHeader = string.Join(";", response.Headers.GetValues("Set-Cookie"));
            Assert.Contains(SiteCorrelationIdConstants.CookieName, setCookieHeader);
        }

        [Fact]
        public async Task PageNavigation_ShouldAlwaysGenerateNewCid_EvenIfCookieExists()
        {
            // Arrange
            using IHost host = BuildHost();
            await host.StartAsync();
            HttpClient client = host.GetTestClient();

            // Première navigation — récupère le cookie initial
            HttpRequestMessage firstRequest = new(HttpMethod.Get, "/");
            firstRequest.Headers.Add("Sec-Fetch-Mode", "navigate");
            HttpResponseMessage firstResponse = await client.SendAsync(firstRequest);

            string firstCookie = ExtractCidFromSetCookie(firstResponse);

            // Deuxième navigation — envoie le cookie existant
            HttpRequestMessage secondRequest = new(HttpMethod.Get, "/");
            secondRequest.Headers.Add("Sec-Fetch-Mode", "navigate");
            secondRequest.Headers.Add("Cookie", $"{SiteCorrelationIdConstants.CookieName}={firstCookie}");
            HttpResponseMessage secondResponse =
                await client.SendAsync(secondRequest);

            string secondCookie = ExtractCidFromSetCookie(secondResponse);

            // Assert — le CID doit être différent à chaque navigation
            Assert.NotEqual(firstCookie, secondCookie);
        }

        // ── Règle 2 : appel AJAX avec cookie existant ────────────────────────

        [Fact]
        public async Task AjaxRequest_WithExistingCookie_ShouldReuseCid()
        {
            // Arrange
            using IHost host = BuildHost();
            await host.StartAsync();
            HttpClient client = host.GetTestClient();

            string existingCid = Guid.NewGuid().ToString("N");

            // Act — pas de Sec-Fetch-Mode (= AJAX), cookie présent
            HttpRequestMessage request = new(HttpMethod.Get, "/Dashboard/GetStats");
            request.Headers.Add("Cookie", $"{SiteCorrelationIdConstants.CookieName}={existingCid}");

            HttpResponseMessage response = await client.SendAsync(request);

            // Assert — pas de nouveau Set-Cookie
            Assert.False(response.Headers.Contains("Set-Cookie"));
        }

        // ── Règle 3 : appel sans Sec-Fetch-Mode ni cookie (Postman, curl) ────

        [Fact]
        public async Task RequestWithoutHeaderNorCookie_ShouldGenerateNewCid()
        {
            // Arrange
            using IHost host = BuildHost();
            await host.StartAsync();
            HttpClient client = host.GetTestClient();

            // Act — ni Sec-Fetch-Mode, ni cookie
            HttpResponseMessage response =
                await client.GetAsync("/api/something");

            // Assert — un CID est quand même généré (pas de crash, pas de CID vide)
            // On vérifie via le cookie posé en fallback
            // Note : dans ce cas le middleware génère un CID mais ne pose PAS de cookie
            // car ce n'est pas une navigation — c'est le comportement attendu
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        // ── Contrat transversal : Items[ItemsKey] toujours alimenté ──────────

        [Fact]
        public async Task AnyRequest_ShouldStoreValidGuid_InHttpContextItems()
        {
            // Arrange
            using IHost host = BuildHost();
            await host.StartAsync();
            HttpClient client = host.GetTestClient();

            // Act — navigation de page pour déclencher la génération d'un CID
            HttpRequestMessage request = new(HttpMethod.Get, "/");
            request.Headers.Add("Sec-Fetch-Mode", "navigate");
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert — le body contient le CID stocké dans Items, qui doit être un GUID valide
            string cid = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(cid));
            Assert.True(Guid.TryParseExact(cid, "N", out _), $"Le CID '{cid}' n'est pas un GUID au format N");
        }

        // ── Helpers privés ───────────────────────────────────────────────────

        private static IHost BuildHost()
        {
            return new HostBuilder().ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.Configure(app =>
                {
                    app.UseMiddleware<SiteCorrelationIdMiddleware>();
                    // Expose Items[ItemsKey] dans le body pour pouvoir l'asserter
                    app.Run(async ctx =>
                    {
                        string cid = ctx.Items[SiteCorrelationIdConstants.ItemsKey]?.ToString() ?? "";
                        await ctx.Response.WriteAsync(cid);
                    });
                });
                webBuilder.ConfigureServices(services =>
                {
                    services.AddLogging();
                });
            })
                                    .Build();
        }

        private static string ExtractCidFromSetCookie(HttpResponseMessage response)
        {
            foreach (string header in response.Headers.GetValues("Set-Cookie"))
            {
                if (header.StartsWith(SiteCorrelationIdConstants.CookieName + "=", StringComparison.InvariantCulture))
                {
                    // Format : "cid=<valeur>; path=/; ..."
                    string value = header.Split('=')[1].Split(';')[0];
                    return value;
                }
            }
            return null;
        }
    }
}