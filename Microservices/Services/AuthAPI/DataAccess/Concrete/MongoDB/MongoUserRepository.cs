using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DataAccess.Abstract;
using DataAccess.Concrete.MongoDB.Context;
using Entities.Concrete;
using MongoDB.Driver;

namespace DataAccess.Concrete.MongoDB.Repositories
{
    public class MongoUserRepository : IUserDal
    {
        private readonly IMongoCollection<User> _userCollection;

        public MongoUserRepository(MongoContext context)
        {
            _userCollection = context.Users;
        }

        public void Add(User entity)
        {
            _userCollection.InsertOne(entity);
        }

        public void Delete(User entity)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, entity.Id);
            _userCollection.DeleteOne(filter);
        }

        public User Get(Expression<Func<User, bool>> filter)
        {
            return _userCollection.Find(filter).FirstOrDefault();
        }

        public List<User> GetAll(Expression<Func<User, bool>> filter = null)
        {
            return filter == null
                ? _userCollection.Find(_ => true).ToList()
                : _userCollection.Find(filter).ToList();
        }

        public User GetByEmailOrPhoneNumber(Expression<Func<User, bool>> filter)
        {
            return _userCollection.Find(filter).FirstOrDefault();
        }

        public void Update(User entity)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, entity.Id);
            var update = Builders<User>.Update
                .Set(u => u.Email, entity.Email)
                .Set(u => u.PhoneNumber, entity.PhoneNumber)
                .Set(u => u.Username, entity.Username)
                .Set(u => u.Password, entity.Password)
                .Set(u => u.ProfilePicture, entity.ProfilePicture)
                //.Set(u => u.DeviceInfo, entity.DeviceInfo)
                .Set(u => u.Location, entity.Location)
                .Set(u => u.FacebookId, entity.FacebookId)
                .Set(u => u.GoogleId, entity.GoogleId)
                .Set(u => u.Verified, entity.Verified)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            _userCollection.UpdateOne(filter, update);
        }
    }
}
