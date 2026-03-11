using System.Net.Http;
using System.Threading.Tasks;
using PageCorrelationId.Site.CorrelationId;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PageCorrelationId.Site.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IHttpClientFactory httpClientFactory, ILogger<DashboardController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // ── GET /Dashboard ───────────────────────
        // Requête de page → SiteCorrelationIdMiddleware génère un NOUVEAU CID
        public IActionResult Index()
        {
            string cid = HttpContext.Items[SiteCorrelationIdConstants.ItemsKey]?.ToString() ?? "N/A";
            _logger.LogInformation("[SITE][Dashboard] Index — CID : {CID}", cid);
            ViewBag.PageCid = cid;
            return View();
        }

        // ── GET /Dashboard/GetStats  (AJAX) ──────
        // Cookie 'cid' renvoyé par le browser → même CID que la page
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            string cid = HttpContext.Items[SiteCorrelationIdConstants.ItemsKey]?.ToString() ?? "N/A";
            _logger.LogInformation("[SITE][Dashboard] GetStats AJAX — CID : {CID}", cid);

            HttpClient client = _httpClientFactory.CreateClient("ApiClient");
            HttpResponseMessage response = await client.GetAsync("api/stats");

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode,
                    new { error = "Erreur API", correlationId = cid });
            }

            string json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }
    }
}