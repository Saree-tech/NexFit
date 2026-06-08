using Microsoft.ML;
using Microsoft.ML.Data;

namespace NexFit.MLModel
{
    public class FoodClassifier
    {
        private readonly MLContext _mlContext;
        private PredictionEngine<ImageData, ImagePrediction>? _predictionEngine;
        private readonly string _modelPath;

        private readonly Dictionary<string, NutritionInfo> _nutritionDb = new()
        {
            ["Biryani"] = new NutritionInfo { FoodName = "Chicken Biryani", Calories = 290, ProteinG = 18.0, CarbsG = 38.0, FatG = 7.0, ServingSize = "1 plate (250g)" },
            ["Burger"] = new NutritionInfo { FoodName = "Burger", Calories = 354, ProteinG = 20.0, CarbsG = 29.0, FatG = 17.0, ServingSize = "1 burger (150g)" },
            ["Pizza"] = new NutritionInfo { FoodName = "Pizza", Calories = 266, ProteinG = 11.0, CarbsG = 33.0, FatG = 10.0, ServingSize = "1 slice (107g)" },
            ["Salad"] = new NutritionInfo { FoodName = "Mixed Salad", Calories = 65, ProteinG = 3.0, CarbsG = 10.0, FatG = 1.5, ServingSize = "1 bowl (150g)" }
        };

        public FoodClassifier(IWebHostEnvironment env)
        {
            _mlContext = new MLContext(seed: 1);
            _modelPath = Path.Combine(env.ContentRootPath, "MLModel", "food_model.zip");
            LoadModel();
        }

        private void LoadModel()
        {
            if (File.Exists(_modelPath))
            {
                var model = _mlContext.Model.Load(_modelPath, out _);
                _predictionEngine = _mlContext.Model
                    .CreatePredictionEngine<ImageData, ImagePrediction>(model);
                Console.WriteLine("Food ML model loaded!");
            }
            else
            {
                Console.WriteLine("Model not found — will use Gemini fallback.");
            }
        }

        public string? Predict(string imagePath)
        {
            if (_predictionEngine == null) return null;
            var input = new ImageData { ImagePath = imagePath, Label = "" };
            var prediction = _predictionEngine.Predict(input);
            return prediction.PredictedLabel;
        }

        public NutritionInfo? PredictFromBytes(byte[] imageBytes)
        {
            if (_predictionEngine == null) return null;

            var tempPath = Path.Combine(Path.GetTempPath(), $"nexfit_{Guid.NewGuid()}.jpg");
            try
            {
                File.WriteAllBytes(tempPath, imageBytes);
                var label = Predict(tempPath);
                if (string.IsNullOrEmpty(label)) return null;

                Console.WriteLine($"Predicted food: {label}");

                if (_nutritionDb.TryGetValue(label, out var nutrition))
                    return nutrition;

                return new NutritionInfo { FoodName = label, Calories = 250, ProteinG = 10.0, CarbsG = 30.0, FatG = 8.0, ServingSize = "1 serving" };
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        public bool IsModelAvailable => _predictionEngine != null;
    }

    public class NutritionInfo
    {
        public string FoodName { get; set; } = "";
        public int Calories { get; set; }
        public double ProteinG { get; set; }
        public double CarbsG { get; set; }
        public double FatG { get; set; }
        public string ServingSize { get; set; } = "";
    }
}