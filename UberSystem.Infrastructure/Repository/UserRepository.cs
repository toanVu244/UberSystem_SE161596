using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberSystem.Domain.Entities;
using UberSystem.Domain.Models;
using UberSystem.Infrastructure;
using UberSystem.Infrastructure.Model;

namespace UberSystem.Domain.Repository
{
    public interface IUserRepository
    {
        Task<User> GetUserByEmail (string email);
        Task<User> GetUserById(long id);    
        Task<int> UpdateUser(User user);
        Task<List<User>> GetAllUser();
        Task<int> AddUser(User user);
        Task<User> GetUserFromCustomerId(long customerId);
        Task<User> Login(string username, string password);
        Task Delete(User user);
        Task<User> GetUserByCode(string code);
    }
    public class UserRepository : IUserRepository
    {
        private readonly UberSystemDbContext dbContext;
        public UserRepository(UberSystemDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<User> GetUserByEmail(string email)
        {
            return await dbContext.Users.FirstOrDefaultAsync(dbContext => dbContext.Email == email);
        }

        public async Task<User> GetUserById(long id)
        {
            return await dbContext.Users.FirstOrDefaultAsync(dbContext=>dbContext.Id == id);
        }

        public async Task<int> UpdateUser(User user)
        {
            dbContext.Users.Update(user);
            return await dbContext.SaveChangesAsync();  
        }

        public async Task<int> AddUser(User user)
        {
            dbContext.Users.Add(user);
            return await dbContext.SaveChangesAsync();
        }

        public async Task<List<User>> GetAllUser()
        {
            return await dbContext.Users.ToListAsync();
        }

        public async Task<User> GetUserFromCustomerId(long customerId)
        {
            return await dbContext.Customers
                .Where(c => c.Id == customerId)
                .Select(c => c.User)
                .FirstOrDefaultAsync();
        }

        public async Task<User> Login(string username, string password)
        {
            return await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == username && u.Password == password);    
        }

        public async Task Delete(User user)
        {
            dbContext.Users.Remove(user); 
            await dbContext.SaveChangesAsync();
        }

        public async Task<User> GetUserByCode(string code)
        {
            return await dbContext.Users.FirstOrDefaultAsync(dbContext => dbContext.Code == code);
        }
    }
}
