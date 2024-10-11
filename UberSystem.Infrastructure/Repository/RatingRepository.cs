using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberSystem.Domain.Entities;

namespace UberSystem.Infrastructure.Repository
{
    public interface IRatingRepository
    {
        Task<List<Rating>> GetAllRating();
        Task Add(Rating rating);
        Task<double> GetAverageRating(long driverId);
    }
    public class RatingRepository : IRatingRepository
    {
        private readonly UberSystemDbContext _dbContext;

        public RatingRepository(UberSystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(Rating rating)
        {
            await _dbContext.Ratings.AddAsync(rating);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Rating>> GetAllRating()
        {
            return await _dbContext.Ratings.ToListAsync();  
        }

        public async Task<double> GetAverageRating(long driverId)
        {
            var ratings = await _dbContext.Ratings
                .Where(r => r.DriverId == driverId && r.Rating1.HasValue) 
                .AverageAsync(r => r.Rating1.Value);

            return ratings;
        }

    }
}
