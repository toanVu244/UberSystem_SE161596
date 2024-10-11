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
    public interface ICabRepsitory
    {
        Task Update(Cab cab);
        Task Add(Cab cab);
        Task<List<Cab>> GetAll();   
    }
    public class CabRepository : ICabRepsitory
    {
        private readonly UberSystemDbContext dbContext;

        public CabRepository(UberSystemDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Add(Cab cab)
        {
            await dbContext.Cabs.AddAsync(cab); 
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<Cab>> GetAll()
        {
            return await dbContext.Cabs.ToListAsync();
        }

        public async Task Update(Cab cab)
        {
            dbContext.Cabs.Update(cab);
            await dbContext.SaveChangesAsync();
        }
    }
}
