using System.Data.Common;
using Dapper;

namespace Infrastructure.Database
{
    public class DapperExecutor : IDapperExecutor
    {
        public async Task<T?> QuerySingleOrDefaultAsync<T>(DbConnection connection, string sql, object? param)
        {
            return await connection.QuerySingleOrDefaultAsync<T>(sql, param);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(DbConnection connection, string sql, object? param)
        {
            return await connection.QueryAsync<T>(sql, param);
        }
    }
}
