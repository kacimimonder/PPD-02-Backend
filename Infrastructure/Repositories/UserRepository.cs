using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        MiniCourseraContext _miniCourseraContext { get; set; }
        public UserRepository(MiniCourseraContext miniCourseraContext)
        {
            _miniCourseraContext = miniCourseraContext;
        }

        public async Task AddAsync(User entity)
        {
            try
            {
                await _miniCourseraContext.Users.AddAsync(entity);
                await _miniCourseraContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var innerEx = ex.InnerException; // Contains the real SQL error
                throw; // Re-throw to see in debugger
            }

        }

        public async Task<User?> GetByEmailAndPasswordAsync(string email, string password)
        {
            var user = _miniCourseraContext.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
            return await user;
        }


        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<User>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(User entity)
        {
            throw new NotImplementedException();
        }
    }
}
