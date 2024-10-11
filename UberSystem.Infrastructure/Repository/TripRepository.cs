using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberSystem.Domain.Entities;
using UberSystem.Infrastructure;

namespace UberSystem.Domain.Repository
{
    public interface ITripRepository
    {
        Task Update(Trip trip);
        Task<Trip> GetTripById(long id);
        Task<Trip> GetTripByDriverId(long driverID);
        Task<List<Trip>> GetAll();
        Task Add(Trip trip);
        Task<Trip> GetTripByCustomerId(long customerId);    

    }
    public class TripRepository : ITripRepository
    {
        private readonly UberSystemDbContext _dbContext;

        public TripRepository(UberSystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(Trip trip)
        {
            await _dbContext.Trips.AddAsync(trip);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Trip>> GetAll()
        {
            return await _dbContext.Trips.ToListAsync();
        }

        public async Task<Trip> GetTripByCustomerId(long customerId)
        {
            return await _dbContext.Trips.Include(t => t.Customer).FirstOrDefaultAsync(t => t.CustomerId == customerId);
        }

        public async Task<Trip> GetTripByDriverId(long driverID)
        {
            return await _dbContext.Trips.FirstOrDefaultAsync(t => t.DriverId == driverID && t.Status.Equals("D"));
        }

        public async Task<Trip> GetTripById(long id)
        {
            return await _dbContext.Trips.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task Update(Trip trip)
        {
            _dbContext.Trips.Update(trip);   
            await _dbContext.SaveChangesAsync();
        }
    }
}
