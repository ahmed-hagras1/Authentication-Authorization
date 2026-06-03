using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YourAppName.Infrastructure.Abstracts
{
    public interface IGenericRepositoryAsync<T> where T : class
    {
        // Supports int, string (GUIDs), and composite keys
        Task<T?> GetByIdAsync(params object[] keyValues);

        IQueryable<T> GetTableNoTracking();
        IQueryable<T> GetTableAsTracking();

        Task<T> AddAsync(T entity);
        Task AddRangeAsync(ICollection<T> entities);

        Task UpdateAsync(T entity);
        Task UpdateRangeAsync(ICollection<T> entities);

        Task DeleteAsync(T entity);
        Task DeleteRangeAsync(ICollection<T> entities);

        Task SaveChangesAsync();

        // UNCOMMENTED & ASYNC: Expose your transaction methods
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitAsync();
        Task RollBackAsync();
    }
}
