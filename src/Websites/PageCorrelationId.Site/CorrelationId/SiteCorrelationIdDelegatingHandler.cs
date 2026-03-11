// ─────────────────────────────────────────────
//  Rôle : intercepter chaque appel HttpClient "ApiClient"
//         et y ajouter l'en-tête X-Correlation-ID
//         en lisant le CID stocké dans IHttpContextAccessor.
// ─────────────────────────────────────────────
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace PageCorrelationId.Site.CorrelationId
{
    public class SiteCorrelationIdDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SiteCorrelationIdDelegatingHandler> _logger;

        public SiteCorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor, ILogger<SiteCorrelationIdDelegatingHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpContext httpContext = _httpContextAccessor.HttpContext;
            string correlationId = httpContext?.Items[SiteCorrelationIdConstants.ItemsKey]?.ToString();

            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                request.Headers.TryAddWithoutValidation(SiteCorrelationIdConstants.HeaderName, correlationId);

                _logger.LogInformation(
                    "[SITE][HttpClient] → {Method} {Uri} — CID : {CID}",
                    request.Method,
                    request.RequestUri,
                    correlationId);
            }
            else
            {
                _logger.LogWarning(
                    "[SITE][HttpClient] CID introuvable dans Items pour {Uri}",
                    request.RequestUri);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}