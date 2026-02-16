using FluentAssertions;
using Tests.SharedHelpers.Builders;
using Tests.SharedHelpers.Fixtures;
using Tests.SharedHelpers.Mocks;

namespace Tests.Unit.Repositories;

public class UsuarioRepositoryTest
{
    private readonly UsuarioRepositoryTestFixture _fixture = new();

    [Fact(DisplayName = "Deve retornar null quando usuário não existe")]
    [Trait("Repository", "UsuarioRepository")]
    public async Task ObterUsuarioAtivoAsync_DeveRetornarNull_QuandoUsuarioNaoExiste()
    {
        // Arrange
        var documentoIdentificador = "12345678901";

        _fixture.DapperExecutorMock
            .AoConsultarUsuario()
            .NaoRetornaNada();

        // Act
        var resultado = await _fixture.UsuarioRepository.ObterUsuarioAtivoAsync(documentoIdentificador);

        // Assert
        resultado.Should().BeNull();

        _fixture.DapperExecutorMock.NaoDeveTerConsultadoRoles();
    }

    [Fact(DisplayName = "Deve mapear roles corretamente quando usuário existe")]
    [Trait("Repository", "UsuarioRepository")]
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
            .ComRoles(new List<string>())
            .Build();

        var roleIds = new List<int> { 1, 2, 3 };

        _fixture.DapperExecutorMock
            .AoConsultarUsuario()
            .Retorna(usuarioModel);

        _fixture.DapperExecutorMock
            .AoConsultarRoles()
            .Retorna(roleIds);

        // Act
        var resultado = await _fixture.UsuarioRepository.ObterUsuarioAtivoAsync(documentoIdentificador);

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
    [Trait("Repository", "UsuarioRepository")]
    public async Task ObterUsuarioAtivoAsync_DeveMappearRoleDesconhecidaParaUnknown()
    {
        // Arrange
        var documentoIdentificador = "12345678901";

        var usuarioModel = new UsuarioModelBuilder()
            .ComDocumento(documentoIdentificador)
            .ComRoles(new List<string>())
            .Build();

        var roleIds = new List<int> { 999 };

        _fixture.DapperExecutorMock
            .AoConsultarUsuario()
            .Retorna(usuarioModel);

        _fixture.DapperExecutorMock
            .AoConsultarRoles()
            .Retorna(roleIds);

        // Act
        var resultado = await _fixture.UsuarioRepository.ObterUsuarioAtivoAsync(documentoIdentificador);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Roles.Should().HaveCount(1);
        resultado.Roles.Should().Contain("Unknown");
    }

    [Fact(DisplayName = "Deve passar documento identificador como parâmetro nas queries")]
    [Trait("Repository", "UsuarioRepository")]
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

        _fixture.DapperExecutorMock
            .AoConsultarUsuario()
            .RetornaComCallback(usuarioModel, (conn, sql, param) =>
            {
                paramCapturadoQueryUsuario = param;
            });

        _fixture.DapperExecutorMock
            .AoConsultarRoles()
            .RetornaComCallback(roleIds, (conn, sql, param) =>
            {
                paramCapturadoQueryRoles = param;
            });

        // Act
        var resultado = await _fixture.UsuarioRepository.ObterUsuarioAtivoAsync(documentoIdentificador);

        // Assert
        resultado.Should().NotBeNull();

        paramCapturadoQueryUsuario.Should().NotBeNull();
        paramCapturadoQueryUsuario.Should().BeEquivalentTo(new { documentoIdentificador });

        paramCapturadoQueryRoles.Should().NotBeNull();
        paramCapturadoQueryRoles.Should().BeEquivalentTo(new { documentoIdentificador });
    }
}
