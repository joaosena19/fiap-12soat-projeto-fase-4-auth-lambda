using System.Data.Common;

namespace Infrastructure.Database
{
    public interface IDapperExecutor
    {
        Task<T?> QuerySingleOrDefaultAsync<T>(DbConnection connection, string sql, object? param);
        Task<IEnumerable<T>> QueryAsync<T>(DbConnection connection, string sql, object? param);
    }
}
