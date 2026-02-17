using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;

namespace Infrastructure.Repositories
{
    public class EnrollmentProgressRepository : IEnrollmentProgressRepository
    {
        MiniCourseraContext _miniCourseraContext;
        public EnrollmentProgressRepository(MiniCourseraContext miniCourseraContext)
        {
            this._miniCourseraContext = miniCourseraContext;
        }

        public async Task AddAsync(EnrollmentProgress entity)
        {
            await _miniCourseraContext.EnrollmentProgresses.AddAsync(entity);
            //await _miniCourseraContext.SaveChangesAsync(); //Achieved Using Unit of work

        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<EnrollmentProgress>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<EnrollmentProgress?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(EnrollmentProgress entity)
        {
            throw new NotImplementedException();
        }
    }
}
