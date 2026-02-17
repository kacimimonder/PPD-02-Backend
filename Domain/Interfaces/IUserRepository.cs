using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUserRepository:IBaseRepository<User>
    {
        public Task<User?> GetByEmailAndPasswordAsync(string email, string password);

    }
}
