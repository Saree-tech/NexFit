using System.Text;
using System.Text.Json;

namespace NexFit.Services
{
    public class WorkoutService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public WorkoutService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["Gemini:ApiKey"] ?? "";
        }

        public async Task<string> GenerateWorkoutPlan(string injury, string goal, string level)
        {
            if (string.IsNullOrEmpty(_apiKey))
                return GetFallbackPlan(goal, level, injury);

            var prompt = $"You are a certified fitness trainer. Create a detailed 7-day workout plan for:\n" +
                        $"- Goal: {goal}\n" +
                        $"- Fitness Level: {level}\n" +
                        $"- Injury/Limitation: {(string.IsNullOrWhiteSpace(injury) ? "None" : injury)}\n\n" +
                        $"Format exactly like this:\n" +
                        $"Day 1: [Focus Area]\n" +
                        $"- Exercise Name: X sets x Y reps\n\n" +
                        $"Make sure exercises are safe considering any injuries. Include rest days.";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    maxOutputTokens = 1000,
                    temperature = 0.7
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _http.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                    return GetFallbackPlan(goal, level, injury);

                var responseJson = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(responseJson);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "";

                return string.IsNullOrEmpty(text) ? GetFallbackPlan(goal, level, injury) : text;
            }
            catch
            {
                return GetFallbackPlan(goal, level, injury);
            }
        }

        private string GetFallbackPlan(string goal, string level, string injury)
        {
            return $"7-Day Workout Plan\n" +
                   $"Goal: {goal} | Level: {level} | Injury: {(string.IsNullOrEmpty(injury) ? "None" : injury)}\n" +
                   $"================================================\n\n" +
                   $"Day 1: Chest & Triceps\n" +
                   $"- Push Ups: 3 sets x 15 reps\n" +
                   $"- Dumbbell Press: 3 sets x 12 reps\n" +
                   $"- Tricep Dips: 3 sets x 10 reps\n\n" +
                   $"Day 2: Back & Biceps\n" +
                   $"- Pull Ups: 3 sets x 10 reps\n" +
                   $"- Dumbbell Rows: 3 sets x 12 reps\n" +
                   $"- Bicep Curls: 3 sets x 15 reps\n\n" +
                   $"Day 3: REST DAY\n" +
                   $"- Light stretching & walking\n\n" +
                   $"Day 4: Legs\n" +
                   $"- Squats: 4 sets x 15 reps\n" +
                   $"- Lunges: 3 sets x 12 reps\n\n" +
                   $"Day 5: Shoulders & Core\n" +
                   $"- Shoulder Press: 3 sets x 12 reps\n" +
                   $"- Plank: 3 sets x 60 seconds\n\n" +
                   $"Day 6: Full Body HIIT\n" +
                   $"- Burpees: 3 sets x 10 reps\n" +
                   $"- Mountain Climbers: 3 sets x 30 seconds\n\n" +
                   $"Day 7: REST & RECOVERY\n" +
                   $"- Foam rolling & stretching";
        }
    }
}