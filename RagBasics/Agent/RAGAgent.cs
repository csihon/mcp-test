
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

class AIAgent
{
    private static readonly HttpClient httpClient = new HttpClient();

    public async Task<string> GetWeatherAsync(string city)
    {
        try
        {
            // Replace with your actual weather API key and endpoint
            string apiKey = "YOUR_API_KEY";
            string apiUrl = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

            HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            JObject weatherData = JObject.Parse(responseBody);

            string description = weatherData["weather"][0]["description"].ToString();
            string temperature = weatherData["main"]["temp"].ToString();

            return $"The weather in {city} is {description} with a temperature of {temperature}°C.";
        }
        catch (Exception ex)
        {
            return $"Sorry, I couldn't fetch the weather data. Error: {ex.Message}";
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello! I am your AI agent. Please enter a city to get the weather:");
        string city = Console.ReadLine();

        AIAgent agent = new AIAgent();
        string weatherInfo = await agent.GetWeatherAsync(city);

        Console.WriteLine(weatherInfo);
    }
}


