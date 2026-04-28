using System.Data;
using HRM.Domain.Interfaces;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    private IDbTransaction? _transaction;

    public IDbTransaction? Transaction => _transaction;

    public UnitOfWork(IDbConnection connection)
    {
        _connection = connection;
    }

    public Task BeginTransactionAsync()
    {
        _transaction = _connection.BeginTransaction();
        return Task.CompletedTask;
    }

    public Task CommitAsync()
    {
        _transaction?.Commit();
        return Task.CompletedTask;
    }

    public Task RollbackAsync()
    {
        _transaction?.Rollback();
        return Task.CompletedTask;
    }

    Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        return CommitAsync().ContinueWith(_ => 0, cancellationToken);
    }
}