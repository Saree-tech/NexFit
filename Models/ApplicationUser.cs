using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace NexFit.Models
{
    [BsonIgnoreExtraElements]
    public class ApplicationUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("fullName")]
        public string FullName { get; set; } = string.Empty;

        [BsonElement("roles")]
        public List<string> Roles { get; set; } = new List<string>();

        [BsonElement("isApproved")]
        public bool IsApproved { get; set; } = false;

        [BsonElement("mustChangePassword")]
        public bool MustChangePassword { get; set; } = false;

        // BsonDefaultValue attribute handles documents where this element might be missing entirely
        [BsonElement("paymentTransactionId")]
        [BsonDefaultValue("")]
        public string PaymentTransactionId { get; set; } = string.Empty;

        [BsonElement("registrationDate")]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        [BsonElement("totalVisitsThisMonth")]
        public int TotalVisitsThisMonth { get; set; } = 0;

        [BsonElement("isSubscriptionActive")]
        public bool IsSubscriptionActive { get; set; } = false;
    }
}