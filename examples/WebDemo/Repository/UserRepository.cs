using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using WebDemo.Models;

namespace WebDemo.Repository
{
    public class UserRepository
    {
        private readonly UserDBContext _dbContext;

        public UserRepository(UserDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public UserModel Login(string name)
        {
            UserModel model = _dbContext.Users.FirstOrDefault(m => m.Name == name);

            if(model != null)
            {
                return model;
            }

            model = new UserModel
            {
                Name = name
            };

            _dbContext.Users.Add(model);

            _dbContext.SaveChanges();

            return model;
        }

        public List<UserModel> GetAllUsers()
        {
            return _dbContext.Users.ToList();
        }

        public bool Revoke(int id)
        {
            UserModel model = _dbContext.Users.FirstOrDefault(m => m.Id == id);

            if (model == null)
            {
                throw new NullReferenceException("not found");
            }

            model.AccessToken = null;

            int count = _dbContext.SaveChanges();

            return count >= 0;
        }

        public UserModel GetUser(int id)
        {
            return _dbContext.Users.FirstOrDefault(m => m.Id == id);
        }

        public bool ConnectLine(int id, string accessToken)
        {
            UserModel model = _dbContext.Users.FirstOrDefault(m => m.Id == id);

            if (model == null)
            {
                throw new NullReferenceException("not found");
            }

            model.AccessToken = accessToken;

            int count = _dbContext.SaveChanges();

            return count >= 0;
        }
    }
}
