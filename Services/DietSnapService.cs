using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NexFit.MLModel;

namespace NexFit.Services
{
    public class DietSnapService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly FoodClassifier _classifier;

        public DietSnapService(HttpClient http, IConfiguration config, IWebHostEnvironment env)
        {
            _http = http;
            _apiKey = config["Gemini:ApiKey"] ?? "";
            _classifier = new FoodClassifier(env);
        }

        public async Task<DietResult> AnalyzeFoodPhoto(byte[] imageBytes)
        {
            // STEP 1: Apna trained ML.NET model try karo
            if (_classifier.IsModelAvailable)
            {
                try
                {
                    var mlResult = _classifier.PredictFromBytes(imageBytes);
                    if (mlResult != null)
                    {
                        return new DietResult
                        {
                            FoodName = mlResult.FoodName,
                            Calories = mlResult.Calories,
                            ProteinG = mlResult.ProteinG,
                            CarbsG = mlResult.CarbsG,
                            FatG = mlResult.FatG,
                            ServingSize = mlResult.ServingSize
                        };
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ML error: {ex.Message} — using Gemini fallback");
                }
            }

            // STEP 2: Gemini API
            try
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
                                new { inline_data = new { mime_type = "image/jpeg", data = base64Image } },
                                new { text = "Look at this image. If this is NOT a food item (a person, selfie, face, or non-food object), return ONLY this JSON: {\"food_name\": \"Not a food item\", \"calories\": 0, \"protein_g\": 0, \"carbs_g\": 0, \"fat_g\": 0, \"serving_size\": \"N/A\"}. If it IS food, return ONLY valid JSON: {\"food_name\": \"name of food\", \"calories\": 000, \"protein_g\": 0.0, \"carbs_g\": 0.0, \"fat_g\": 0.0, \"serving_size\": \"description\"}" }
                            }
                        }
                    }
                };

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _http.PostAsync(url, content);

                if (!response.IsSuccessStatusCode) return GetFallbackResult();

                var responseJson = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(responseJson);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "";

                text = text.Replace("```json", "").Replace("```", "").Trim();
                var result = JsonSerializer.Deserialize<DietResult>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result ?? GetFallbackResult();
            }
            catch
            {
                return GetFallbackResult();
            }
        }

        private DietResult GetFallbackResult() => new DietResult
        {
            FoodName = "Food Item",
            Calories = 350,
            ProteinG = 15.0,
            CarbsG = 45.0,
            FatG = 10.0,
            ServingSize = "1 serving"
        };
    }

    public class DietResult
    {
        [JsonPropertyName("food_name")] public string FoodName { get; set; } = "";
        [JsonPropertyName("calories")] public int Calories { get; set; }
        [JsonPropertyName("protein_g")] public double ProteinG { get; set; }
        [JsonPropertyName("carbs_g")] public double CarbsG { get; set; }
        [JsonPropertyName("fat_g")] public double FatG { get; set; }
        [JsonPropertyName("serving_size")] public string ServingSize { get; set; } = "";
    }
}