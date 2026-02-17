using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Interfaces;

namespace Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MiniCourseraContext _context;
        public UnitOfWork(MiniCourseraContext context)
        {
            _context = context;
        }
        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }

}
