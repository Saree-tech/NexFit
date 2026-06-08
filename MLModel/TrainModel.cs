using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;

namespace NexFit.MLModel
{
    public class TrainModel
    {
        public static void Train(string datasetPath, string modelSavePath)
        {
            var mlContext = new MLContext(seed: 1);

            Console.WriteLine("📁 Loading images from dataset...");

            var images = new List<ImageData>();
            var folders = Directory.GetDirectories(datasetPath);

            foreach (var folder in folders)
            {
                var label = Path.GetFileName(folder);
                var files = Directory.GetFiles(folder, "*.jpg")
                    .Concat(Directory.GetFiles(folder, "*.jpeg"))
                    .Concat(Directory.GetFiles(folder, "*.png"))
                    .Concat(Directory.GetFiles(folder, "*.jfif"));

                foreach (var file in files)
                {
                    images.Add(new ImageData { ImagePath = file, Label = label });
                }
                Console.WriteLine($"✅ {label}: {files.Count()} images loaded");
            }

            Console.WriteLine($"\n📊 Total images: {images.Count}");

            var data = mlContext.Data.LoadFromEnumerable(images);
            var shuffled = mlContext.Data.ShuffleRows(data);
            var split = mlContext.Data.TrainTestSplit(shuffled, testFraction: 0.2);

            Console.WriteLine("\n🔧 Building training pipeline...");

            var pipeline = mlContext.Transforms
                .Conversion.MapValueToKey("LabelKey", "Label")
                .Append(mlContext.Transforms.LoadImages(
                    outputColumnName: "ImageObject",
                    imageFolder: null,
                    inputColumnName: "ImagePath"))
                .Append(mlContext.Transforms.ResizeImages(
                    outputColumnName: "ImageResized",
                    imageWidth: 224,
                    imageHeight: 224,
                    inputColumnName: "ImageObject"))
                .Append(mlContext.Transforms.ExtractPixels(
                    outputColumnName: "Pixels",
                    inputColumnName: "ImageResized",
                    interleavePixelColors: true,
                    offsetImage: 117))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                    labelColumnName: "LabelKey",
                    featureColumnName: "Pixels"))
                .Append(mlContext.Transforms.Conversion
                    .MapKeyToValue("PredictedLabel", "PredictedLabel"));

            Console.WriteLine("🚀 Training started... (1-2 minutes)");
            var model = pipeline.Fit(split.TrainSet);

            Console.WriteLine("\n📈 Evaluating model...");
            var predictions = model.Transform(split.TestSet);
            var metrics = mlContext.MulticlassClassification.Evaluate(
                predictions, labelColumnName: "LabelKey");
            Console.WriteLine($"✅ Accuracy: {metrics.MacroAccuracy:P2}");

            Console.WriteLine("\n💾 Saving model...");
            Directory.CreateDirectory(Path.GetDirectoryName(modelSavePath)!);
            mlContext.Model.Save(model, data.Schema, modelSavePath);
            Console.WriteLine($"✅ Model saved: {modelSavePath}");
        }
    }

    public class ImageData
    {
        public string ImagePath { get; set; } = "";
        public string Label { get; set; } = "";
    }

    public class ImagePrediction
    {
        public string? PredictedLabel { get; set; }
        public float[]? Score { get; set; }
    }
}