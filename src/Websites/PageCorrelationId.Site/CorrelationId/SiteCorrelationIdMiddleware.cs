// ─────────────────────────────────────────────
//  RÈGLES MÉTIER :
//    • Requête de PAGE (navigation browser) → toujours un NOUVEAU CID
//      → Set-Cookie: cid=<nouveau>
//    • Tout le reste (AJAX, fetch, DataTables, appels serveur…)
//      → lire le cookie 'cid' existant
//      → NE PAS régénérer (même CID pour tous les appels de la page)
//
//  Détection de page : en-tête Sec-Fetch-Mode: navigate
//  Envoyé automatiquement par tous les navigateurs modernes lors
//  d'une navigation (clic lien, saisie URL, F5).
//  Aucune action requise côté JS — fonctionne avec jQuery, fetch,
//  DataTables, axios, etc. sans configuration supplémentaire.
//
//  Fallback : appels serveur-à-serveur (Postman, curl, tests)
//  n'envoient pas Sec-Fetch-Mode → on réutilise le cookie si présent,
//  sinon on génère un nouveau CID.
// ─────────────────────────────────────────────
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace PageCorrelationId.Site.CorrelationId
{
    public class SiteCorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SiteCorrelationIdMiddleware> _logger;

        public SiteCorrelationIdMiddleware(
            RequestDelegate next,
            ILogger<SiteCorrelationIdMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId;

            if (IsPageNavigation(context))
            {
                // ── Navigation de page : nouveau CID ────────────────
                correlationId = GenerateNewCid();

                context.Response.Cookies.Append(SiteCorrelationIdConstants.CookieName, correlationId, new CookieOptions
                {
                    HttpOnly = false,
                    SameSite = SameSiteMode.Lax,
                    IsEssential = true
                });

                _logger.LogInformation(
                    "[SITE] PAGE {Method} {Path} — NOUVEAU CID : {CID}",
                    context.Request.Method,
                    context.Request.Path,
                    correlationId);
            }
            else
            {
                // ── AJAX / fetch / appel serveur : réutiliser le cookie ──
                correlationId = context.Request.Cookies[SiteCorrelationIdConstants.CookieName] ?? GenerateNewCid();

                _logger.LogInformation(
                    "[SITE] NON-PAGE {Method} {Path} — CID réutilisé : {CID}",
                    context.Request.Method,
                    context.Request.Path,
                    correlationId);
            }

            // Disponible dans tout le pipeline (controllers, ViewComponents…)
            context.Items[SiteCorrelationIdConstants.ItemsKey] = correlationId;

            await _next(context);
        }

        /// <summary>
        /// Détecte une navigation de page via Sec-Fetch-Mode: navigate.
        /// Ce header est envoyé automatiquement par les navigateurs modernes
        /// (Chrome 76+, Firefox 90+, Edge 79+, Safari 16.4+).
        /// Il n'est jamais envoyé par les appels AJAX, fetch, ou serveur-à-serveur.
        /// </summary>
        private static bool IsPageNavigation(HttpContext context)
        {
            return string.Equals(
                context.Request.Headers["Sec-Fetch-Mode"],
                "navigate",
                StringComparison.OrdinalIgnoreCase);
        }

        private static string GenerateNewCid()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}