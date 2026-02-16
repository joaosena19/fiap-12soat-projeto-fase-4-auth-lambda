using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Infrastructure.Database
{
    public class NpgsqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public NpgsqlConnectionFactory(IConfiguration configuration)
        {
            var host = configuration["DatabaseConnection:Host"];
            var port = configuration["DatabaseConnection:Port"];
            var database = configuration["DatabaseConnection:DatabaseName"];
            var username = configuration["DatabaseConnection:User"];
            var password = configuration["DatabaseConnection:Password"];

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(database) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException(
                    $"Configuração de banco de dados incompleta. " +
                    $"Host: {host}, Port: {port}, Database: {database}, User: {username}, " +
                    $"Password: {(string.IsNullOrEmpty(password) ? "VAZIO" : "DEFINIDO")}");
            }

            _connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};";
        }

        public async Task<DbConnection> CreateOpenConnectionAsync()
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}
