using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol.Server;
using MyMCPServer.Services;

namespace MyMCPWebAPIServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [McpServerResourceType()]
    public class RagController : ControllerBase
    {
        private readonly ILogger<RagController> _logger;
        private readonly RagService _ragService;

        public RagController(ILogger<RagController> logger, RagService ragService)
        {
            _logger = logger;
            _ragService = ragService;
        }

        [HttpGet(Name = "ask")]
        [McpServerResource(Name = "Ask", Title = "Ask for answers")]
        public async Task<object> Ask(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Results.BadRequest("Query parameter is required.");
            }

            var response = await _ragService.GetAnswerAsync(query);

            return Results.Ok(new { query, response });
        }
    }
}
