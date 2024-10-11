using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberSystem.Domain.Entities;

namespace UberSystem.Infrastructure.Repository
{
    public interface ICustomerRepository
    {
        Task Add(Customer customer);
        Task<List<Customer>> GetAll();
        Task<Customer> GetCustomerById(long id);
    }
    public class CustomerRepository : ICustomerRepository
    {
        private readonly UberSystemDbContext dbContext;
        public CustomerRepository(UberSystemDbContext dbContext) 
        {
            this.dbContext = dbContext; 
        }

        public async Task Add(Customer customer)
        {
            await dbContext.Customers.AddAsync(customer);
            await dbContext.SaveChangesAsync(); 
        }

        public async Task<List<Customer>> GetAll()
        {
            return await dbContext.Customers.ToListAsync();
        }

        public async Task<Customer> GetCustomerById(long id)
        {
            return await dbContext.Customers.Include(t => t.Trips).FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}
