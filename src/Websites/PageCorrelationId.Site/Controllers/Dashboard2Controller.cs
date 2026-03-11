using System.Net.Http;
using System.Threading.Tasks;
using PageCorrelationId.Site.CorrelationId;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PageCorrelationId.Site.Controllers
{
    public class Dashboard2Controller : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Dashboard2Controller> _logger;

        public Dashboard2Controller(IHttpClientFactory httpClientFactory, ILogger<Dashboard2Controller> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // ── GET /Dashboard2 ──────────────────────
        // Nouvelle page → SiteCorrelationIdMiddleware génère un NOUVEAU CID
        public IActionResult Index()
        {
            string cid = HttpContext.Items[SiteCorrelationIdConstants.ItemsKey]?.ToString() ?? "N/A";
            _logger.LogInformation("[SITE][Dashboard2] Index — CID : {CID}", cid);
            ViewBag.PageCid = cid;
            return View();
        }

        // ── GET /Dashboard2/GetSomething  (AJAX) ─
        [HttpGet]
        public async Task<IActionResult> GetSomething()
        {
            string cid = HttpContext.Items[SiteCorrelationIdConstants.ItemsKey]?.ToString() ?? "N/A";
            _logger.LogInformation("[SITE][Dashboard2] GetSomething AJAX — CID : {CID}", cid);

            HttpClient client = _httpClientFactory.CreateClient("ApiClient");
            HttpResponseMessage response = await client.GetAsync("api/something");

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