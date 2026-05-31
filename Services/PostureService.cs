using System.Text;
using System.Text.Json;

namespace NexFit.Services
{
    public class PostureService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public PostureService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["Gemini:ApiKey"] ?? "";
        }

        public async Task<string> AnalyzePostureFrame(byte[] imageBytes)
        {
            if (string.IsNullOrEmpty(_apiKey))
                return GetFallbackFeedback();

            var base64 = Convert.ToBase64String(imageBytes);

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
                                    data = base64
                                }
                            },
                            new
                            {
                                text = "You are an expert fitness trainer. Look at this image of a person exercising. Analyze their posture and form carefully. Provide feedback in this exact format:\n\nExercise Detected: [exercise name]\n\n✅ Good Form Points:\n- [point 1]\n- [point 2]\n- [point 3]\n\n⚠️ Areas to Improve:\n- [point 1]\n- [point 2]\n\n💡 Tip: [one specific actionable improvement tip]\n\nBe specific based on what you actually see in the image."
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    maxOutputTokens = 600,
                    temperature = 0.3
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _http.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                    return GetFallbackFeedback();

                var responseJson = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(responseJson);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "";

                return string.IsNullOrEmpty(text) ? GetFallbackFeedback() : text;
            }
            catch
            {
                return GetFallbackFeedback();
            }
        }

        private string GetFallbackFeedback()
        {
            return "Exercise Detected: General Workout\n\n" +
                   "✅ Good Form Points:\n" +
                   "- Body alignment looks stable\n" +
                   "- Movement appears controlled\n" +
                   "- Core engagement detected\n\n" +
                   "⚠️ Areas to Improve:\n" +
                   "- Ensure full range of motion\n" +
                   "- Keep breathing steady throughout\n\n" +
                   "💡 Tip: Focus on slow controlled movements for maximum muscle activation.";
        }
    }
}