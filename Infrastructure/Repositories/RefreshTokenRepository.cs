using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private MiniCourseraContext _miniCourseraContext;
        public RefreshTokenRepository(MiniCourseraContext miniCourseraContext)
        {
            _miniCourseraContext = miniCourseraContext;
        }

        public async Task AddAsync(RefreshToken entity)
        {
            _miniCourseraContext.Add(entity);
            await _miniCourseraContext.SaveChangesAsync();
        }
        public async Task UpdateAsync(RefreshToken entity)
        {
            RefreshToken? oldRefreshToken = await _miniCourseraContext.RefreshTokens
                .FindAsync(entity.TokenId);
            if (oldRefreshToken == null)
            {
                throw new Exception($"CourseModule with ID {entity.TokenId} not found.");
            }
            oldRefreshToken.Token = entity.Token;
            oldRefreshToken.ExpiresOn = entity.ExpiresOn;
            oldRefreshToken.RevokedOn = entity.RevokedOn;

            await _miniCourseraContext.SaveChangesAsync();
        }
        public async Task<RefreshToken?> GetActiveRefreshToken(int userId)
        {
            try
            {
                RefreshToken? refreshToken = await _miniCourseraContext.RefreshTokens
    .FirstOrDefaultAsync(rf => rf.UserId == userId && rf.ExpiresOn > DateTime.UtcNow
    && rf.RevokedOn == null);
                return refreshToken;
            }
            catch (Exception ex) { 
                return null;
            }

        }
        public async Task<RefreshToken?> GetRefreshTokenByTokenAndUserId(int userId, string refreshToken)
        {
            return await _miniCourseraContext.RefreshTokens
                .Include(rf => rf.User)
                .FirstOrDefaultAsync(rf => rf.Token == refreshToken
                && rf.UserId == userId);
        }
        public async Task<RefreshToken?> GetRefreshTokenByToken(string refreshToken)
        {
            return await _miniCourseraContext.RefreshTokens
                .Include(rf => rf.User)
                .FirstOrDefaultAsync(rf => rf.Token == refreshToken);
        }


        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<RefreshToken>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<RefreshToken?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }



    }
}
