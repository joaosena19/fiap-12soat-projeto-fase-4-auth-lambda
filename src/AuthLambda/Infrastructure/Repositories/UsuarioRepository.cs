using Dapper;
using Infrastructure.Database.Models;
using Infrastructure.Gateways.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioGateway
    {
        private readonly string _connectionString;

        public UsuarioRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public UsuarioRepository(IConfiguration configuration)
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

        public async Task<UsuarioModel?> ObterUsuarioAtivoAsync(string documentoIdentificador)
        {
            const string query = @"
                SELECT 
                    u.id as Id,
                    u.documento_identificador as DocumentoIdentificador,
                    u.senha_hash as SenhaHash,
                    u.status as Status,
                    c.id as ClienteId
                FROM usuarios u
                LEFT JOIN clientes c ON u.documento_identificador = c.documento_identificador
                WHERE u.documento_identificador = @documentoIdentificador
                AND u.status = 'ativo'";

            const string rolesQuery = @"
                SELECT r.id as RoleId
                FROM usuarios u
                INNER JOIN usuarios_roles ur ON u.id = ur.usuario_id
                INNER JOIN roles r ON ur.role_id = r.id
                WHERE u.documento_identificador = @documentoIdentificador
                AND u.status = 'ativo'";

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Buscar dados do usuário
            var usuario = await connection.QuerySingleOrDefaultAsync<UsuarioModel>(query, new { documentoIdentificador });
            
            if (usuario == null)
                return null;

            // Buscar roles do usuário
            var roles = await connection.QueryAsync<int>(rolesQuery, new { documentoIdentificador });
            usuario.Roles = roles.Select(r => r switch
            {
                1 => "Administrador",
                2 => "Cliente",
                _ => "Unknown"
            }).ToList();

            return usuario;
        }
    }
}