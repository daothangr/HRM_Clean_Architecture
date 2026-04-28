using System;
using System.Collections.Generic;
using System.Text;

namespace HRM.Application.Common.Interfaces.Repositories
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<TEntity>> ListAsync(CancellationToken cancellationToken = default);
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);
    }
}
