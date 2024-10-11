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
    public interface IDriverRepository
    {
        Task<List<Driver>> GetDrivers();
        Task Update(Driver newDriver);
        Task Add(Driver newDriver);
        Task<Driver> GetDriverByDriverID(long driverID);
        Task UpdateLocationAsync(long driverId, double latitude, double longitude);

    }
    public class DriverRepository : IDriverRepository
    {
        private readonly UberSystemDbContext dbContext;
        public DriverRepository(UberSystemDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task UpdateLocationAsync(long UserId, double latitude, double longitude)
        {
            //var driver = await dbContext.Drivers.User.FindAsync(UserId);
            var driver = await dbContext.Drivers.FirstOrDefaultAsync(d => d.User.Id == UserId); 

            driver.LocationLatitude = latitude;
            driver.LocationLongitude = longitude;

            dbContext.Drivers.Update(driver);
            await dbContext.SaveChangesAsync();
        }
        public async Task Add(Driver newDriver)
        {
            await dbContext.Drivers.AddAsync(newDriver);
            await dbContext.SaveChangesAsync();
        }
        public Task<Driver> GetDriverByDriverID(long driverID)
        {
            return dbContext.Drivers.Include(d => d.Cab).FirstOrDefaultAsync(u => u.Id == driverID);
        }

        public async Task<List<Driver>> GetDrivers()
        {
            List<Driver> drivers = new List<Driver>();
            var listDriver = await dbContext.Drivers
                        .OrderByDescending(d => d.DriverRating ?? 0)
                        .ToListAsync();
            foreach (var driver in listDriver)
            {
                Trip item = await dbContext.Trips.FirstOrDefaultAsync(t => t.DriverId == driver.Id);
                if (item == null || !item.Status.Equals("A") || !item.Status.Equals("P") ||!item.Status.Equals("T"))
                {
                    drivers.Add(driver);
                }
            }
            return drivers;
        }

        public async Task Update(Driver newDriver)
        {
            dbContext.Drivers.Update(newDriver);
            await dbContext.SaveChangesAsync();
        }
    }
}
