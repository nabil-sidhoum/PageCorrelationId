namespace PageCorrelationId.Site.CorrelationId
{
    public static class SiteCorrelationIdConstants
    {
        public const string ItemsKey = "SiteCorrelationId";  // contrat HttpContext.Items
        public const string CookieName = "cid";              // contrat cookie navigateur
        public const string HeaderName = "X-Correlation-ID"; // contrat header HTTP vers l'API
    }
}