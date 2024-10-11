using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberSystem.Domain.Entities;

namespace UberSystem.Infrastructure.Repository
{
    public interface IRefreshTokenRepository
    {
        Task Add(RefreshToken refreshToken);
        Task Update(RefreshToken refreshToken);
        Task<RefreshToken> GetRefreshTokenByUserID(long userId);
        Task<RefreshToken> GetRefreshTokenByToken(string token);
    }
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly UberSystemDbContext dbContext;

        public RefreshTokenRepository(UberSystemDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Add(RefreshToken refreshToken)
        {
            await dbContext.RefreshTokens.AddAsync(refreshToken);
            await dbContext.SaveChangesAsync();
        }

        public async Task<RefreshToken> GetRefreshTokenByToken(string token)
        {
            return await dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token);
        }

        public async Task<RefreshToken> GetRefreshTokenByUserID(long userId)
        {
            return await dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task Update(RefreshToken refreshToken)
        {
            dbContext.RefreshTokens.Update(refreshToken);
            await dbContext.SaveChangesAsync();
        }
    }
}
