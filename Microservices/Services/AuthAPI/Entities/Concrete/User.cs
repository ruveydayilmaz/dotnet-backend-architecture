using System;
using System.Collections.Generic;
using Core.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Entities.Concrete
{
    public class User : IEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRequired]
        [BsonRepresentation(BsonType.String)]
        public string Email { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string PhoneNumber { get; set; }

        [BsonRequired]
        [BsonRepresentation(BsonType.String)]
        public string Username { get; set; }

        [BsonRequired]
        [BsonRepresentation(BsonType.String)]
        public string Password { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string ProfilePicture { get; set; }

        //[BsonRepresentation(BsonType.Array)]
        //public List<string> DeviceInfo { get; set; }

        [BsonElement("Location")]
        public Location Location { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string FacebookId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string GoogleId { get; set; }

        [BsonElement("Verified")]
        public VerifiedInfo Verified { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; }

        public User()
        {
            Id = ObjectId.GenerateNewId().ToString();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public class Location
    {
        public string City { get; set; }
        public string Country { get; set; }
    }

    public class VerifiedInfo
    {
        public bool IsVerified { get; set; }
        public string VerifiedWith { get; set; }
    }
}
