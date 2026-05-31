using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NexFit.Services
{
    public class DietSnapService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public DietSnapService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<DietResult> AnalyzeFoodPhoto(byte[] imageBytes)
        {
            // Mock data - API key ke bagair bhi kaam karega
            await Task.Delay(1500);

            return new DietResult
            {
                FoodName = "Detected Food Item",
                Calories = new Random().Next(200, 800),
                ProteinG = Math.Round(new Random().NextDouble() * 30, 1),
                CarbsG = Math.Round(new Random().NextDouble() * 60, 1),
                FatG = Math.Round(new Random().NextDouble() * 25, 1),
                ServingSize = "1 serving (estimated)"
            };
        }
    }

    public class DietResult
    {
        [JsonPropertyName("food_name")]
        public string FoodName { get; set; } = "";
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