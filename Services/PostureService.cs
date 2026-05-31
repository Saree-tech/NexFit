namespace NexFit.Services
{
    public class PostureService
    {
        public async Task<string> AnalyzePostureFrame(byte[] frameBytes)
        {
            await Task.Delay(2000);

            var feedbacks = new List<string>
            {
                @"Exercise Detected: Squat

✅ Good Form Points:
- Back is straight and aligned
- Feet shoulder-width apart
- Core appears engaged

⚠️ Areas to Improve:
- Knees slightly caving inward
- Lower the squat depth for full range of motion

💡 Tip: Focus on pushing knees outward as you descend.",

                @"Exercise Detected: Push Up

✅ Good Form Points:
- Arms at correct width
- Head in neutral position

⚠️ Areas to Improve:
- Hips are slightly raised
- Elbows flaring too wide

💡 Tip: Keep your body in a straight line from head to heels.",

                @"Exercise Detected: Deadlift

✅ Good Form Points:
- Bar close to body
- Shoulders back and down

⚠️ Areas to Improve:
- Lower back showing slight rounding
- Hips rising too fast

💡 Tip: Engage your lats and think about pushing the floor away."
            };

            var random = new Random();
            return feedbacks[random.Next(feedbacks.Count)];
        }
    }
}