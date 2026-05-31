using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NexFit.Services
{
    public class DietSnapService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public DietSnapService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["Gemini:ApiKey"] ?? "";
        }

        public async Task<DietResult> AnalyzeFoodPhoto(byte[] imageBytes)
        {
            var base64Image = Convert.ToBase64String(imageBytes);

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new
                            {
                                inline_data = new
                                {
                                    mime_type = "image/jpeg",
                                    data = base64Image
                                }
                            },
                            new
                            {
                                text = "Analyze this food image carefully. Return ONLY valid JSON with no markdown, no code block, no extra text. Use exactly this format: {\"food_name\": \"name of food\", \"calories\": 000, \"protein_g\": 0.0, \"carbs_g\": 0.0, \"fat_g\": 0.0, \"serving_size\": \"description\"}"
                            }
                        }
                    }
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                return GetFallbackResult();
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(responseJson);

            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";

            // Clean response
            text = text.Replace("```json", "").Replace("```", "").Trim();

            try
            {
                var result = JsonSerializer.Deserialize<DietResult>(text, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return result ?? GetFallbackResult();
            }
            catch
            {
                return GetFallbackResult();
            }
        }

        private DietResult GetFallbackResult()
        {
            return new DietResult
            {
                FoodName = "Food Item",
                Calories = 350,
                ProteinG = 15.0,
                CarbsG = 45.0,
                FatG = 10.0,
                ServingSize = "1 serving"
            };
        }
    }

    public class DietResult
    {
        [JsonPropertyName("food_name")]
        public string FoodName { get; set; } = "";

        [JsonPropertyName("calories")]
        public int Calories { get; set; }

        [JsonPropertyName("protein_g")]
        public double ProteinG { get; set; }

        [JsonPropertyName("carbs_g")]
        public double CarbsG { get; set; }

        [JsonPropertyName("fat_g")]
        public double FatG { get; set; }

        [JsonPropertyName("serving_size")]
        public string ServingSize { get; set; } = "";
    }
}