using FluentAssertions;
using Tests.SharedHelpers.Builders;

namespace Tests.Unit.MetaTests.SharedHelpers;

public class TokenRequestDtoBuilderTest
{
    [Fact(DisplayName = "Build deve retornar TokenRequestDto válido com CPF e senha")]
    public void Build_DeveRetornarTokenRequestDtoValido()
    {
        // Act
        var tokenRequest = new TokenRequestDtoBuilder().Build();

        // Assert
        tokenRequest.Should().NotBeNull();
        tokenRequest.DocumentoIdentificadorUsuario.Should().NotBeNullOrWhiteSpace();
        tokenRequest.Senha.Should().NotBeNullOrWhiteSpace();
        tokenRequest.DocumentoIdentificadorUsuario.Should().HaveLength(11); // CPF sem máscara
    }

    [Fact(DisplayName = "ComDocumento deve permitir sobrescrever documento")]
    public void ComDocumento_DevePermitirSobrescreverDocumento()
    {
        // Arrange
        const string documentoEsperado = "98765432100";

        // Act
        var tokenRequest = new TokenRequestDtoBuilder()
            .ComDocumento(documentoEsperado)
            .Build();

        // Assert
        tokenRequest.DocumentoIdentificadorUsuario.Should().Be(documentoEsperado);
    }

    [Fact(DisplayName = "ComSenha deve permitir sobrescrever senha")]
    public void ComSenha_DevePermitirSobrescreverSenha()
    {
        // Arrange
        const string senhaEsperada = "SenhaTeste@456";

        // Act
        var tokenRequest = new TokenRequestDtoBuilder()
            .ComSenha(senhaEsperada)
            .Build();

        // Assert
        tokenRequest.Senha.Should().Be(senhaEsperada);
    }
}
