using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
    {
        Task<RefreshToken?> GetActiveRefreshToken(int userId);
        Task<RefreshToken?> GetRefreshTokenByTokenAndUserId(int userId,string refreshToken);
        Task<RefreshToken?> GetRefreshTokenByToken(string refreshToken);

    }
}
