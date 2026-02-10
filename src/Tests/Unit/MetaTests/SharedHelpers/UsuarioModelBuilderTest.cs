using FluentAssertions;
using Tests.SharedHelpers.Builders;

namespace Tests.Unit.MetaTests.SharedHelpers;

public class UsuarioModelBuilderTest
{
    [Fact(DisplayName = "Build deve retornar UsuarioModel válido por padrão")]
    public void Build_DeveRetornarUsuarioModelValido()
    {
        // Act
        var usuario = new UsuarioModelBuilder().Build();

        // Assert
        usuario.Should().NotBeNull();
        usuario.Id.Should().NotBeEmpty();
        usuario.DocumentoIdentificador.Should().NotBeNullOrWhiteSpace();
        usuario.DocumentoIdentificador.Should().HaveLength(11); // CPF sem máscara
        usuario.SenhaHash.Should().NotBeNullOrWhiteSpace();
        usuario.Status.Should().Be("ativo");
        usuario.ClienteId.Should().BeNull();
        usuario.Roles.Should().NotBeNull();
        usuario.Roles.Should().Contain("Cliente");
    }

    [Fact(DisplayName = "ComId deve permitir sobrescrever Id")]
    public void ComId_DevePermitirSobrescreverId()
    {
        // Arrange
        var idEsperado = Guid.NewGuid();

        // Act
        var usuario = new UsuarioModelBuilder()
            .ComId(idEsperado)
            .Build();

        // Assert
        usuario.Id.Should().Be(idEsperado);
    }

    [Fact(DisplayName = "ComDocumento deve permitir sobrescrever documento")]
    public void ComDocumento_DevePermitirSobrescreverDocumento()
    {
        // Arrange
        const string documentoEsperado = "98765432100";

        // Act
        var usuario = new UsuarioModelBuilder()
            .ComDocumento(documentoEsperado)
            .Build();

        // Assert
        usuario.DocumentoIdentificador.Should().Be(documentoEsperado);
    }

    [Fact(DisplayName = "ComSenhaHash deve permitir sobrescrever SenhaHash")]
    public void ComSenhaHash_DevePermitirSobrescreverSenhaHash()
    {
        // Arrange
        const string senhaHashEsperada = "novo_hash_base64";

        // Act
        var usuario = new UsuarioModelBuilder()
            .ComSenhaHash(senhaHashEsperada)
            .Build();

        // Assert
        usuario.SenhaHash.Should().Be(senhaHashEsperada);
    }

    [Fact(DisplayName = "ComStatus deve permitir sobrescrever Status")]
    public void ComStatus_DevePermitirSobrescreverStatus()
    {
        // Arrange
        const string statusEsperado = "inativo";

        // Act
        var usuario = new UsuarioModelBuilder()
            .ComStatus(statusEsperado)
            .Build();

        // Assert
        usuario.Status.Should().Be(statusEsperado);
    }

    [Fact(DisplayName = "ComClienteId deve permitir sobrescrever ClienteId")]
    public void ComClienteId_DevePermitirSobrescreverClienteId()
    {
        // Arrange
        var clienteIdEsperado = Guid.NewGuid();

        // Act
        var usuario = new UsuarioModelBuilder()
            .ComClienteId(clienteIdEsperado)
            .Build();

        // Assert
        usuario.ClienteId.Should().Be(clienteIdEsperado);
    }

    [Fact(DisplayName = "ComRoles deve permitir sobrescrever Roles")]
    public void ComRoles_DevePermitirSobrescreverRoles()
    {
        // Arrange
        var rolesEsperadas = new List<string> { "Administrador", "Sistema" };

        // Act
        var usuario = new UsuarioModelBuilder()
            .ComRoles(rolesEsperadas)
            .Build();

        // Assert
        usuario.Roles.Should().BeEquivalentTo(rolesEsperadas);
    }
}
