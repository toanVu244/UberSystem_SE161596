using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberSystem.Domain.Entities;

namespace UberSystem.Infrastructure.Repository
{
    public interface IPaymentRepository
    {
        Task Add(Payment payment);
        Task<List<Payment>> GetAll();
        Task Update(Payment payment);  
        Task<Payment> GetById(long id);
        Task<Payment> GetByTripId(long TripId);

    }
    public class PaymentRepository : IPaymentRepository
    {
        private readonly UberSystemDbContext _dbContext;

        public PaymentRepository(UberSystemDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task Add(Payment payment)
        {
            await _dbContext.Payments.AddAsync(payment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Payment>> GetAll()
        {
            return await _dbContext.Payments.ToListAsync();
        }

        public async Task<Payment> GetById(long id)
        {
            return await _dbContext.Payments.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Payment> GetByTripId(long TripId)
        {
            var trip = await _dbContext.Trips.FirstOrDefaultAsync(t => t.Id == TripId); 
            return await _dbContext.Payments.FirstOrDefaultAsync(p => p.Id == trip.PaymentId);
        }

        public async Task Update(Payment payment)
        {
            _dbContext.Payments.Update(payment);    
            await _dbContext.SaveChangesAsync();
        }
    }
}
