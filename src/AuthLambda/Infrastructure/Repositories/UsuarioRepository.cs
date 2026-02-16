using Dapper;
using Infrastructure.Database;
using Infrastructure.Database.Models;
using Infrastructure.Gateways.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data.Common;

namespace Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioGateway
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IDapperExecutor _dapperExecutor;

        public UsuarioRepository(IDbConnectionFactory connectionFactory, IDapperExecutor dapperExecutor)
        {
            _connectionFactory = connectionFactory;
            _dapperExecutor = dapperExecutor;
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

            await using var connection = await _connectionFactory.CreateOpenConnectionAsync();

            // Buscar dados do usuário
            var usuario = await _dapperExecutor.QuerySingleOrDefaultAsync<UsuarioModel>(connection, query, new { documentoIdentificador });
            
            if (usuario == null)
                return null;

            // Buscar roles do usuário
            var roles = await _dapperExecutor.QueryAsync<int>(connection, rolesQuery, new { documentoIdentificador });
            usuario.Roles = roles.Select(r => r switch
            {
                1 => "Administrador",
                2 => "Cliente",
                3 => "Sistema",
                _ => "Unknown"
            }).ToList();

            return usuario;
        }
    }
}
