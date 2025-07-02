using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace HotelBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public AIController(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration.GetSection("OpenAI")["ApiKey"];
        }

        [HttpPost]
        public async Task<IActionResult> ParseMessage([FromBody] AIRequest request)
        {
            var systemPrompt = @"
You are an AI hotel booking assistant. 

Given a user message, extract the following JSON object. 
Even if some fields are missing in the user’s message, fill them as null or default.

JSON output:
{
  ""intent"": ""query | book | cancel"",
  ""city"": ""(city name or null)"",
  ""startDate"": ""yyyy-MM-dd or null"",
  ""endDate"": ""yyyy-MM-dd or null"",
  ""guestCount"": number or 0,
}

Examples:

User: I'd like to book a hotel in Ankara from July 15 to July 18 for 2 adults.
{
  ""intent"": ""query"",
  ""city"": ""Ankara"",
  ""startDate"": ""2025-07-15"",
  ""endDate"": ""2025-07-18"",
  ""guestCount"": 2,
}

{
  ""intent"": ""query"",
  ""city"": null,
  ""startDate"": null,
  ""endDate"": null,
  ""guestCount"": 0,
}

ALWAYS respond only with valid JSON. No explanation or text outside JSON.";


            var messages = new[]
            {
                new {
                    role = "system",
                    content = systemPrompt
                },
                new {
                    role = "user",
                    content = request.Message
                }
            };

            var body = new
            {
                model = "gpt-3.5-turbo",
                messages = messages,
                temperature = 0,
                max_tokens = 500 
            };

            var json = JsonSerializer.Serialize(body);

            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.openai.com/v1/chat/completions"),
                Headers =
                {
                    { "Authorization", $"Bearer {_apiKey}" }
                },
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, responseBody);

            var result = JsonDocument.Parse(responseBody);
            var messageContent = result.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            AIResponse aiResponse;
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                aiResponse = JsonSerializer.Deserialize<AIResponse>(messageContent, options);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to parse JSON.", details = ex.Message, raw = messageContent });
            }

            return Ok(aiResponse);
        }
    }

    public class AIRequest
    {
        public string Message { get; set; }
    }

    public class AIResponse
    {
        public string Intent { get; set; }
        public string? City { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public int? GuestCount { get; set; }
    }

    public class HotelDto
    {
        public string? Name { get; set; }
        public string? Location { get; set; }
        public double? Rating { get; set; }
        public double? PricePerNight { get; set; }
    }
}
