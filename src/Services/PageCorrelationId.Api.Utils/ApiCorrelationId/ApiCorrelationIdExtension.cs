using Microsoft.AspNetCore.Builder;

namespace PageCorrelationId.Api.Utils.ApiCorrelationId
{
    public static class ApiCorrelationIdExtension
    {
        public static IApplicationBuilder UseApiCorrelationId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiCorrelationIdMiddleware>();
        }
    }
}