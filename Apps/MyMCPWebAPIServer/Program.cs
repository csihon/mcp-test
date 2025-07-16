
using MyMCPServer.Services;
using MyMCPWebAPIServer.EmbeddingGenerator;
using MyMCPWebAPIServer.Repository;

namespace MyMCPWebAPIServer
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
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSingleton<IEmbeddingGenerator>(sp => new OllamaEmbeddingGenerator(new Uri(llmServerURL), llmModel));
            builder.Services.AddSingleton(sp => new TextRepository(connectionString, sp.GetRequiredService<IEmbeddingGenerator>()));
            builder.Services.AddSingleton(sp => new RagService(sp.GetRequiredService<TextRepository>(), new Uri(llmServerURL), llmModel));
            
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Minimal API endpoints
            app.MapGet("/", () => "My MCP WebAPI server is running!!!");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
