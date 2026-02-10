using FluentAssertions;
using Infrastructure.Database;
using Infrastructure.Database.Models;
using Infrastructure.Repositories;
using Moq;
using System.Data.Common;
using Tests.SharedHelpers.Builders;
using Xunit;

namespace Tests.Unit.Repositories;

public class UsuarioRepositoryTest
{
    private readonly Mock<IDbConnectionFactory> _connectionFactoryMock;
    private readonly Mock<IDapperExecutor> _dapperExecutorMock;
    private readonly Mock<DbConnection> _connectionMock;
    private readonly UsuarioRepository _repository;

    public UsuarioRepositoryTest()
    {
        _connectionFactoryMock = new Mock<IDbConnectionFactory>();
        _dapperExecutorMock = new Mock<IDapperExecutor>();
        _connectionMock = new Mock<DbConnection>();

        // Setup connection factory to return mock connection
        _connectionFactoryMock
            .Setup(x => x.CreateOpenConnectionAsync())
            .ReturnsAsync(_connectionMock.Object);

        _repository = new UsuarioRepository(_connectionFactoryMock.Object, _dapperExecutorMock.Object);
    }

    [Fact(DisplayName = "Deve retornar null quando usuário não existe")]
    public async Task ObterUsuarioAtivoAsync_DeveRetornarNull_QuandoUsuarioNaoExiste()
    {
        // Arrange
        var documentoIdentificador = "12345678901";

        _dapperExecutorMock
            .Setup(x => x.QuerySingleOrDefaultAsync<UsuarioModel>(
                It.IsAny<DbConnection>(),
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync((UsuarioModel?)null);

        // Act
        var resultado = await _repository.ObterUsuarioAtivoAsync(documentoIdentificador);

        // Assert
        resultado.Should().BeNull();

        // Verify que QueryAsync NÃO foi chamado (early return optimization)
        _dapperExecutorMock.Verify(
            x => x.QueryAsync<int>(
                It.IsAny<DbConnection>(),
                It.IsAny<string>(),
                It.IsAny<object>()),
            Times.Never);
    }

    [Fact(DisplayName = "Deve mapear roles corretamente quando usuário existe")]
    public async Task ObterUsuarioAtivoAsync_DeveMappearRolesCorretamente_QuandoUsuarioExiste()
    {
        // Arrange
        var documentoIdentificador = "12345678901";
        var usuarioId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();

        var usuarioModel = new UsuarioModelBuilder()
            .ComId(usuarioId)
            .ComDocumento(documentoIdentificador)
            .ComSenhaHash("hash-senha")
            .ComStatus("ativo")
            .ComClienteId(clienteId)
            .ComRoles(new List<string>()) // Será preenchido pela query de roles
            .Build();

        var roleIds = new List<int> { 1, 2, 3 };

        _dapperExecutorMock
            .Setup(x => x.QuerySingleOrDefaultAsync<UsuarioModel>(
                It.IsAny<DbConnection>(),
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(usuarioModel);

        _dapperExecutorMock
            .Setup(x => x.QueryAsync<int>(
                It.IsAny<DbConnection>(),
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(roleIds);

        // Act
        var resultado = await _repository.ObterUsuarioAtivoAsync(documentoIdentificador);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(usuarioId);
        resultado.DocumentoIdentificador.Should().Be(documentoIdentificador);
        resultado.ClienteId.Should().Be(clienteId);
        resultado.Roles.Should().HaveCount(3);
        resultado.Roles.Should().Contain("Administrador");
        resultado.Roles.Should().Contain("Cliente");
        resultado.Roles.Should().Contain("Sistema");
    }

    [Fact(DisplayName = "Deve mapear role desconhecida para Unknown")]
    public async Task ObterUsuarioAtivoAsync_DeveMappearRoleDesconhecidaParaUnknown()
    {
        // Arrange
        var documentoIdentificador = "12345678901";

        var usuarioModel = new UsuarioModelBuilder()
            .ComDocumento(documentoIdentificador)
            .ComRoles(new List<string>())
            .Build();

        var roleIds = new List<int> { 999 }; // Role ID desconhecido

        _dapperExecutorMock
            .Setup(x => x.QuerySingleOrDefaultAsync<UsuarioModel>(
                It.IsAny<DbConnection>(),
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(usuarioModel);

        _dapperExecutorMock
            .Setup(x => x.QueryAsync<int>(
                It.IsAny<DbConnection>(),
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(roleIds);

        // Act
        var resultado = await _repository.ObterUsuarioAtivoAsync(documentoIdentificador);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Roles.Should().HaveCount(1);
        resultado.Roles.Should().Contain("Unknown");
    }

    [Fact(DisplayName = "Deve passar documento identificador como parâmetro nas queries")]
    public async Task ObterUsuarioAtivoAsync_DevePassarDocumentoIdentificadorComoParametro_NasQueries()
    {
        // Arrange
        var documentoIdentificador = "12345678901";

        var usuarioModel = new UsuarioModelBuilder()
            .ComDocumento(documentoIdentificador)
            .ComRoles(new List<string>())
            .Build();

        var roleIds = new List<int> { 2 };

        object? paramCapturadoQueryUsuario = null;
        object? paramCapturadoQueryRoles = null;

        _dapperExecutorMock
            .Setup(x => x.QuerySingleOrDefaultAsync<UsuarioModel>(
                It.IsAny<DbConnection>(),
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(usuarioModel)
            .Callback<DbConnection, string, object?>((conn, sql, param) =>
            {
                paramCapturadoQueryUsuario = param;
            });

        _dapperExecutorMock
            .Setup(x => x.QueryAsync<int>(
                It.IsAny<DbConnection>(),
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(roleIds)
            .Callback<DbConnection, string, object?>((conn, sql, param) =>
            {
                paramCapturadoQueryRoles = param;
            });

        // Act
        var resultado = await _repository.ObterUsuarioAtivoAsync(documentoIdentificador);

        // Assert
        resultado.Should().NotBeNull();

        // Verificar que ambas as queries receberam o parâmetro correto
        paramCapturadoQueryUsuario.Should().NotBeNull();
        paramCapturadoQueryUsuario.Should().BeEquivalentTo(new { documentoIdentificador });

        paramCapturadoQueryRoles.Should().NotBeNull();
        paramCapturadoQueryRoles.Should().BeEquivalentTo(new { documentoIdentificador });
    }
}
