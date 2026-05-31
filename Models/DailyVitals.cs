using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NexFit.Models
{
    public class DailyVitals
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.UtcNow;

        // =========================
        // BODY
        // =========================

        public double Weight { get; set; }
        public double Height { get; set; }

        public double WaterIntake { get; set; }

        public double SleepHours { get; set; }

        // =========================
        // FITNESS
        // =========================

        public int StepsWalked { get; set; }

        public int WorkoutMinutes { get; set; }

        public int CaloriesBurned { get; set; }

        // =========================
        // HEALTH
        // =========================

        public int HeartRate { get; set; }

        public string Mood { get; set; } = string.Empty;

        public string ExerciseType { get; set; } = string.Empty;

        // =========================
        // AI SCORE
        // =========================

        public int FitnessScore { get; set; }
    }
}