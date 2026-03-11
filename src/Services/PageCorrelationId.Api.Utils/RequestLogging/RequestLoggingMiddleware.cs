using System;
using System.Diagnostics;
using System.Threading.Tasks;
using PageCorrelationId.Api.Utils.ApiCorrelationId;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace PageCorrelationId.Api.Utils.RequestLogging
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            Stopwatch timer = Stopwatch.StartNew();
            Exception caughtException = null;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                caughtException = ex;
                throw;
            }
            finally
            {
                timer.Stop();

                string correlationId = context.Items[ApiCorrelationIdMiddleware.ItemKey]?.ToString() ?? "N/A";

                if (caughtException is not null)
                {
                    // ASP.NET Core initialise StatusCode à 200 par défaut.
                    // Si une exception survient avant l'écriture de la réponse (HasStarted = false),
                    // StatusCode n'a jamais été mis à jour — on force 500.
                    // HasStarted = true uniquement en streaming ou écriture manuelle dans Response.Body.
                    int statusCode = context.Response.HasStarted ? context.Response.StatusCode : 500;

                    _logger.LogError(
                        caughtException,
                        "[API] {Scheme} {Method} {Url} → {StatusCode} in {Elapsed}ms | CID: {CorrelationId}",
                        context.Request.Scheme.ToUpperInvariant(),
                        context.Request.Method,
                        context.Request.Path.Value + context.Request.QueryString,
                        statusCode,
                        timer.ElapsedMilliseconds,
                        correlationId);
                }
                else
                {
                    _logger.LogInformation(
                        "[API] {Scheme} {Method} {Url} → {StatusCode} in {Elapsed}ms | CID: {CorrelationId}",
                        context.Request.Scheme.ToUpperInvariant(),
                        context.Request.Method,
                        context.Request.Path.Value + context.Request.QueryString,
                        context.Response.StatusCode,
                        timer.ElapsedMilliseconds,
                        correlationId);
                }
            }
        }
    }
}