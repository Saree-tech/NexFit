using Microsoft.AspNetCore.Mvc;
using NexFit.Services;

namespace NexFit.Controllers
{
    public class AIController : Controller
    {
        private readonly DietSnapService _dietService;
        private readonly PostureService _postureService;
        private readonly WorkoutService _workoutService;

        public AIController(DietSnapService dietService, PostureService postureService, WorkoutService workoutService)
        {
            _dietService = dietService;
            _postureService = postureService;
            _workoutService = workoutService;
        }

        public IActionResult DietSnap() => View();
        public IActionResult VideoUpload() => View();
        public IActionResult WorkoutArchitect() => View();

        [HttpPost]
        public async Task<IActionResult> AnalyzeFood(IFormFile image)
        {
            if (image == null)
                return Json(new { foodName = "No image", calories = 0, proteinG = 0, carbsG = 0, fatG = 0, servingSize = "" });

            using var ms = new MemoryStream();
            await image.CopyToAsync(ms);
            var bytes = ms.ToArray();

            try
            {
                var result = await _dietService.AnalyzeFoodPhoto(bytes);
                return Json(new
                {
                    foodName = result.FoodName,
                    calories = result.Calories,
                    proteinG = result.ProteinG,
                    carbsG = result.CarbsG,
                    fatG = result.FatG,
                    servingSize = result.ServingSize
                });
            }
            catch (Exception ex)
            {
                return Json(new { foodName = "Error: " + ex.Message, calories = 0, proteinG = 0, carbsG = 0, fatG = 0, servingSize = "" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AnalyzePosture(IFormFile video)
        {
            if (video == null) return Json(new { feedback = "No video uploaded" });
            using var ms = new MemoryStream();
            await video.CopyToAsync(ms);
            var feedback = await _postureService.AnalyzePostureFrame(ms.ToArray());
            return Json(new { feedback });
        }

        [HttpPost]
        public async Task<IActionResult> GenerateWorkout([FromBody] WorkoutRequest request)
        {
            var plan = await _workoutService.GenerateWorkoutPlan(
                request.Injury, request.Goal, request.Level);
            return Json(new { plan });
        }
    }

    public class WorkoutRequest
    {
        public string Injury { get; set; } = "";
        public string Goal { get; set; } = "";
        public string Level { get; set; } = "";
    }
}