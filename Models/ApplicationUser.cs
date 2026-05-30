using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace NexFit.Models
{
    [BsonIgnoreExtraElements]
    public class ApplicationUser
    {
        // =========================================
        // CORE ID
        // =========================================
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        // =========================================
        // AUTH
        // =========================================

        [BsonElement("FullName")]
        public string FullName { get; set; } = string.Empty;

        [BsonElement("Email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("PasswordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("Roles")]
        public List<string> Roles { get; set; } = new();

        // =========================================
        // MEMBERSHIP
        // =========================================

        [BsonElement("IsApproved")]
        public bool IsApproved { get; set; } = false;

        [BsonElement("MustChangePassword")]
        public bool MustChangePassword { get; set; } = false;

        [BsonElement("SubscriptionActive")]
        public bool SubscriptionActive { get; set; } = false;

        [BsonElement("MembershipType")]
        public string MembershipType { get; set; } = "Basic";

        [BsonElement("PaymentTransactionId")]
        public string PaymentTransactionId { get; set; } = string.Empty;

        // =========================================
        // DATES (SAFE NULLABLE)
        // =========================================

        [BsonElement("RegistrationDate")]
        public DateTime? RegistrationDate { get; set; } = DateTime.UtcNow;

        [BsonElement("LastLoginDate")]
        public DateTime? LastLoginDate { get; set; }

        [BsonElement("MembershipStartDate")]
        public DateTime? MembershipStartDate { get; set; } = DateTime.UtcNow;

        [BsonElement("MembershipEndDate")]
        public DateTime? MembershipEndDate { get; set; }

        // =========================================
        // PROFILE
        // =========================================

        [BsonElement("Age")]
        public int Age { get; set; } = 18;

        [BsonElement("Gender")]
        public string Gender { get; set; } = string.Empty;

        [BsonElement("Height")]
        public double Height { get; set; }

        [BsonElement("Weight")]
        public double Weight { get; set; }

        [BsonElement("GoalWeight")]
        public double GoalWeight { get; set; }

        [BsonElement("FitnessGoal")]
        public string FitnessGoal { get; set; } = string.Empty;

        [BsonElement("ActivityLevel")]
        public string ActivityLevel { get; set; } = string.Empty;

        [BsonElement("Bio")]
        public string Bio { get; set; } = string.Empty;

        [BsonElement("ProfileImage")]
        public string ProfileImage { get; set; } = "/images/default-user.png";

        // =========================================
        // FITNESS STATS
        // =========================================

        [BsonElement("WorkoutStreak")]
        public int WorkoutStreak { get; set; }

        [BsonElement("TotalVisits")]
        public int TotalVisits { get; set; }

        [BsonElement("TotalVisitsThisMonth")]
        public int TotalVisitsThisMonth { get; set; }

        [BsonElement("CaloriesBurned")]
        public int CaloriesBurned { get; set; }

        [BsonElement("CompletedWorkouts")]
        public int CompletedWorkouts { get; set; }

        [BsonElement("GoalCompletion")]
        public int GoalCompletion { get; set; }

        [BsonElement("ChurnRisk")]
        public int ChurnRisk { get; set; }

        [BsonElement("BMI")]
        public double BMI { get; set; }

        [BsonElement("WaterIntake")]
        public double WaterIntake { get; set; }

        [BsonElement("SleepHours")]
        public double SleepHours { get; set; }

        [BsonElement("LastAttendanceDate")]
        public DateTime? LastAttendanceDate { get; set; }

        // =========================================
        // ATTENDANCE
        // =========================================

        [BsonElement("AttendanceLogs")]
        public List<DateTime> AttendanceLogs { get; set; } = new();

        // =========================================
        // AI FEATURES
        // =========================================

        [BsonElement("NextClass")]
        public string NextClass { get; set; } = string.Empty;

        // =========================================
        // LIVE STATUS
        // =========================================

        [BsonElement("IsInsideGym")]
        public bool IsInsideGym { get; set; }

        [BsonElement("LastCheckInTime")]
        public DateTime? LastCheckInTime { get; set; }

        [BsonElement("LastCheckOutTime")]
        public DateTime? LastCheckOutTime { get; set; }

        // =========================================
        // IGNORE COMPUTED FIELDS
        // =========================================

        [BsonIgnore]
        public bool IsMembershipExpired =>
            MembershipEndDate.HasValue && MembershipEndDate.Value < DateTime.UtcNow;

        [BsonIgnore]
        public int RemainingMembershipDays =>
            MembershipEndDate.HasValue
                ? (MembershipEndDate.Value - DateTime.UtcNow).Days
                : 0;

        [BsonIgnore]
        public bool ShouldRenewMembership =>
            RemainingMembershipDays <= 5;

        [BsonIgnore]
        public double WeightDifference =>
            GoalWeight - Weight;
    }
}