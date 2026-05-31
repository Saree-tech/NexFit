namespace NexFit.Services
{
    public class WorkoutService
    {
        public async Task<string> GenerateWorkoutPlan(string injury, string goal, string level)
        {
            await Task.Delay(2000);

            return $@"7-Day Workout Plan
Goal: {goal} | Level: {level} | Injury: {(string.IsNullOrEmpty(injury) ? "None" : injury)}
================================================

Day 1: Chest & Triceps
- Push Ups: 3 sets x 15 reps
- Dumbbell Press: 3 sets x 12 reps
- Tricep Dips: 3 sets x 10 reps

Day 2: Back & Biceps
- Pull Ups: 3 sets x 10 reps
- Dumbbell Rows: 3 sets x 12 reps
- Bicep Curls: 3 sets x 15 reps

Day 3: REST DAY
- Light stretching & walking

Day 4: Legs & Glutes
- Squats: 4 sets x 15 reps
- Lunges: 3 sets x 12 reps each
- Leg Press: 3 sets x 10 reps

Day 5: Shoulders & Core
- Shoulder Press: 3 sets x 12 reps
- Lateral Raises: 3 sets x 15 reps
- Plank: 3 sets x 60 seconds

Day 6: Full Body HIIT
- Burpees: 3 sets x 10 reps
- Mountain Climbers: 3 sets x 30 seconds
- Jump Squats: 3 sets x 15 reps

Day 7: REST & RECOVERY
- Foam rolling & stretching";
        }
    }
}