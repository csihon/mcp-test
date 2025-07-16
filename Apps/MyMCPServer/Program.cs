using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Services;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using MyMCPServer.EmbeddingGenerator;
using MyMCPServer.Models;
using MyMCPServer.Repository;
using MyMCPServer.Services;

namespace MyMCPServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Fix: Access the configuration object from the builder
            var configuration = builder.Configuration;

            // Load settings from appsettings.json
            var connectionString = configuration.GetConnectionString("PgVector");
            var llmServerURL = configuration.GetSection("LlmServer").GetValue<string>("BaseUrl"); 
            var llmModel = configuration.GetSection("LlmServer").GetValue<string>("ModelName");

            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(llmServerURL) || string.IsNullOrEmpty(llmModel))
            {
                throw new InvalidOperationException("Required configuration settings are missing.");
            }

            // Register services
            builder.Services.AddSingleton<IEmbeddingGenerator>(sp => new OllamaEmbeddingGenerator(new Uri(llmServerURL), llmModel));

            builder.Services.AddSingleton(sp => new TextRepository(connectionString, sp.GetRequiredService<IEmbeddingGenerator>()));

            builder.Services.AddSingleton(sp => new RagService(sp.GetRequiredService<TextRepository>(), new Uri(llmServerURL), llmModel));

            var app = builder.Build();

            // Minimal API endpoints
            app.MapGet("/", () => "My MCP server is running!");

            app.MapPost("/add-text", async (TextRepository textRepository, HttpContext context) =>
            {
                var request = await context.Request.ReadFromJsonAsync<AddTextRequest>();
                if (string.IsNullOrWhiteSpace(request?.Content))
                {
                    return Results.BadRequest("Content is required.");
                }

                await textRepository.StoreTextAsync(request.Content);

                return Results.Ok("Text added successfully.");
            });
            
            app.MapGet("/ask", async (RagService ragService, string query) =>
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Results.BadRequest("Query parameter is required.");
                }

                var response = await ragService.GetAnswerAsync(query);

                return Results.Ok(new { query, response });
            });

            app.Run();
        }


    }
}