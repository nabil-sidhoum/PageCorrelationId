using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PageCorrelationId.Site.CorrelationId;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace PageCorrelationId.Site.Tests.CorrelationId
{
    public class SiteCorrelationIdDelegatingHandlerTests
    {
        // ── Cas 1 : CID présent dans Items ───────────────────────────────────

        [Fact]
        public async Task SendAsync_WithCidInItems_ShouldInjectCorrelationHeader()
        {
            // Arrange
            string expectedCid = "test-cid-12345";

            DefaultHttpContext httpContext = new();
            httpContext.Items[SiteCorrelationIdConstants.ItemsKey] = expectedCid;

            Mock<IHttpContextAccessor> accessorMock = new();
            accessorMock.Setup(a => a.HttpContext).Returns(httpContext);

            SiteCorrelationIdDelegatingHandler handler = new(
                accessorMock.Object,
                NullLogger<SiteCorrelationIdDelegatingHandler>.Instance)
            {
                // Handler terminal — retourne 200 sans faire d'appel réseau
                InnerHandler = new StubHttpHandler()
            };

            HttpMessageInvoker invoker = new(handler);
            HttpRequestMessage request = new(HttpMethod.Get,
                "https://api.example.com/test");

            // Act
            await invoker.SendAsync(request, CancellationToken.None);

            // Assert — le header doit être présent avec la bonne valeur
            Assert.True(request.Headers.Contains(SiteCorrelationIdConstants.HeaderName));
            string actual = string.Join("",
                request.Headers.GetValues(SiteCorrelationIdConstants.HeaderName));
            Assert.Equal(expectedCid, actual);
        }

        // ── Cas 2 : CID absent (HttpContext null) ─────────────────────────────

        [Fact]
        public async Task SendAsync_WithoutHttpContext_ShouldNotInjectHeader()
        {
            // Arrange
            Mock<IHttpContextAccessor> accessorMock = new();
            accessorMock.Setup(a => a.HttpContext).Returns((HttpContext)null);

            SiteCorrelationIdDelegatingHandler handler = new(
                accessorMock.Object,
                NullLogger<SiteCorrelationIdDelegatingHandler>.Instance)
            {
                InnerHandler = new StubHttpHandler()
            };

            HttpMessageInvoker invoker = new(handler);
            HttpRequestMessage request = new(HttpMethod.Get,
                "https://api.example.com/test");

            // Act
            await invoker.SendAsync(request, CancellationToken.None);

            // Assert — aucun header de corrélation ne doit être injecté
            Assert.False(request.Headers.Contains(SiteCorrelationIdConstants.HeaderName));
        }

        // ── Cas 3 : Items présent mais CID vide ──────────────────────────────

        [Fact]
        public async Task SendAsync_WithEmptyCidInItems_ShouldNotInjectHeader()
        {
            // Arrange
            DefaultHttpContext httpContext = new();
            httpContext.Items[SiteCorrelationIdConstants.ItemsKey] = "";

            Mock<IHttpContextAccessor> accessorMock = new();
            accessorMock.Setup(a => a.HttpContext).Returns(httpContext);

            SiteCorrelationIdDelegatingHandler handler = new(
                accessorMock.Object,
                NullLogger<SiteCorrelationIdDelegatingHandler>.Instance)
            {
                InnerHandler = new StubHttpHandler()
            };

            HttpMessageInvoker invoker = new(handler);
            HttpRequestMessage request = new(HttpMethod.Get,
                "https://api.example.com/test");

            // Act
            await invoker.SendAsync(request, CancellationToken.None);

            // Assert
            Assert.False(request.Headers.Contains(SiteCorrelationIdConstants.HeaderName));
        }

        // ── Stub ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Handler terminal qui retourne toujours 200 sans faire d'appel réseau.
        /// Nécessaire car DelegatingHandler exige un InnerHandler.
        /// </summary>
        private sealed class StubHttpHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }
    }
}