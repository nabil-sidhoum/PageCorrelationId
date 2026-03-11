// ─────────────────────────────────────────────
//  Responsabilités :
//    1. Lire l'en-tête X-Correlation-ID envoyé par le Site MVC
//    2. Stocker le CID dans HttpContext.Items["CorrelationId"]
//    3. Renvoyer le même CID dans la réponse (X-Correlation-ID)
//    4. Logger chaque requête avec le CID
// ─────────────────────────────────────────────
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace PageCorrelationId.Api.Utils.ApiCorrelationId
{
    public class ApiCorrelationIdMiddleware
    {
        public const string HeaderName = "X-Correlation-ID";
        public const string ItemKey = "CorrelationId";
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiCorrelationIdMiddleware> _logger;

        public ApiCorrelationIdMiddleware(RequestDelegate next, ILogger<ApiCorrelationIdMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            // 1) Lire/générer le CID
            string correlationId =
                context.Request.Headers.TryGetValue(HeaderName, out StringValues existing) &&
                !string.IsNullOrWhiteSpace(existing)
                    ? existing.ToString()
                    : Guid.NewGuid().ToString();

            // 2) Exposer partout
            context.Items[ItemKey] = correlationId;

            // 3) Header réponse
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(HeaderName))
                {
                    context.Response.Headers.Append(HeaderName, correlationId);
                }

                return Task.CompletedTask;
            });

            // 4) Taguer l'Activity courante si elle existe, ou en créer une propre
            bool activityCreated = false;
            Activity activity = Activity.Current;

            if (activity is null)
            {
                activity = new Activity("incoming-http").Start();
                activityCreated = true;
            }

            activity.SetTag("correlation.id", correlationId);

            // 5) Scope logging
            using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
            {
                await _next(context);
            }

            // 6) Stopper uniquement l'Activity qu'on a créée nous-mêmes
            if (activityCreated)
            {
                activity.Stop();
            }
        }
    }
}