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
    public interface IGSPRepository
    {
        Task<GSP> GetGSP(string source, string destination);
        Task<GSP> GetGSPByPStart(string locaiton);
        Task<GSP> GetGSPByLocation(string locaiton); 
        Task Add(List<GSP> gsps);
        Task<GSP> GetGSPByPStartPEnd(string pStart, string pEnd);

        Task<string> GetlocaRan();
    }
    public class GSPRepository : IGSPRepository
    {
        private readonly UberSystemDbContext dbContext;
        public GSPRepository(UberSystemDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<string> GetlocaRan()
        {
            return await dbContext.GSPs
                .OrderBy(gsp => Guid.NewGuid()) 
                .Select(gsp => gsp.PStart) 
                .FirstOrDefaultAsync(); 
        }
        public async Task Add(List<GSP> gsps)
        {
            await dbContext.AddRangeAsync(gsps);
            await dbContext.SaveChangesAsync();
        }

        public async Task<GSP> GetGSP(string source, string destination)
        {
            return await dbContext.GSPs.FirstOrDefaultAsync(gsp => gsp.Regions.Contains(source) && gsp.Regions.Contains(destination));
        }

        public async Task<GSP> GetGSPByLocation(string locaiton)
        {
            return await dbContext.GSPs.FirstOrDefaultAsync(gsp => gsp.Regions.Contains(locaiton));
        }

        public async Task<GSP> GetGSPByPStart(string locaiton)
        {
            return await dbContext.GSPs.FirstOrDefaultAsync(g => g.PStart == locaiton);
        }

        public async Task<GSP> GetGSPByPStartPEnd(string pStart, string pEnd)
        {
            return await dbContext.GSPs.FirstOrDefaultAsync(gsp => gsp.PStart == pStart && gsp.PEnd == pEnd);
        }
    }
}
