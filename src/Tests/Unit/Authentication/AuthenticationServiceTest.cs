using FluentAssertions;
using Shared.Enums;
using Shared.Exceptions;
using Tests.SharedHelpers.Builders;
using Tests.SharedHelpers.Fixtures;
using Tests.SharedHelpers.Mocks;

namespace Tests.Unit.Authentication;

public class AuthenticationServiceTest
{
    private readonly AuthenticationServiceTestFixture _fixture = new();

    [Fact(DisplayName = "Deve lançar DomainException InvalidInput quando documento for vazio")]
    [Trait("Service", "AuthenticationService")]
    public async Task ValidateCredentialsAndGenerateTokenAsync_DeveLancarDomainException_InvalidInput_QuandoDocumentoForVazio()
    {
        // Arrange
        var request = new TokenRequestDtoBuilder()
            .ComDocumento("")
            .Build();

        // Act
        var act = async () => await _fixture.AuthenticationService.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorType == ErrorType.InvalidInput)
            .WithMessage("Documento identificador e senha são obrigatórios.");
    }

    [Fact(DisplayName = "Deve lançar DomainException InvalidInput quando senha for vazia")]
    [Trait("Service", "AuthenticationService")]
    public async Task ValidateCredentialsAndGenerateTokenAsync_DeveLancarDomainException_InvalidInput_QuandoSenhaForVazia()
    {
        // Arrange
        var request = new TokenRequestDtoBuilder()
            .ComSenha("")
            .Build();

        // Act
        var act = async () => await _fixture.AuthenticationService.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorType == ErrorType.InvalidInput)
            .WithMessage("Documento identificador e senha são obrigatórios.");
    }

    [Fact(DisplayName = "Deve lançar DomainException Unauthorized quando CPF for inválido")]
    [Trait("Service", "AuthenticationService")]
    public async Task ValidateCredentialsAndGenerateTokenAsync_DeveLancarDomainException_Unauthorized_QuandoCpfForInvalido()
    {
        // Arrange
        var request = new TokenRequestDtoBuilder()
            .ComDocumento("12345678901") // CPF inválido
            .Build();

        // Act
        var act = async () => await _fixture.AuthenticationService.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorType == ErrorType.Unauthorized)
            .WithMessage("Documento identificador inválido.");
    }

    [Fact(DisplayName = "Deve lançar DomainException Unauthorized quando CNPJ for inválido")]
    [Trait("Service", "AuthenticationService")]
    public async Task ValidateCredentialsAndGenerateTokenAsync_DeveLancarDomainException_Unauthorized_QuandoCnpjForInvalido()
    {
        // Arrange
        var request = new TokenRequestDtoBuilder()
            .ComDocumento("12345678000100") // CNPJ inválido
            .Build();

        // Act
        var act = async () => await _fixture.AuthenticationService.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorType == ErrorType.Unauthorized)
            .WithMessage("Documento identificador inválido.");
    }

    [Fact(DisplayName = "Deve lançar DomainException Unauthorized quando usuário não for encontrado")]
    [Trait("Service", "AuthenticationService")]
    public async Task ValidateCredentialsAndGenerateTokenAsync_DeveLancarDomainException_Unauthorized_QuandoUsuarioNaoForEncontrado()
    {
        // Arrange
        var cpfValido = "52998224725"; // CPF válido
        var request = new TokenRequestDtoBuilder()
            .ComDocumento(cpfValido)
            .Build();

        _fixture.UsuarioGatewayMock
            .AoObterUsuarioAtivo()
            .ComDocumento(cpfValido)
            .NaoRetornaNada();

        // Act
        var act = async () => await _fixture.AuthenticationService.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorType == ErrorType.Unauthorized)
            .WithMessage("Usuário não encontrado ou inativo.");
    }

    [Fact(DisplayName = "Deve lançar DomainException Unauthorized quando senha for incorreta")]
    [Trait("Service", "AuthenticationService")]
    public async Task ValidateCredentialsAndGenerateTokenAsync_DeveLancarDomainException_Unauthorized_QuandoSenhaForIncorreta()
    {
        // Arrange
        var cpfValido = "52998224725"; // CPF válido
        var senha = "Senha@123";
        var senhaHash = "hash-da-senha";

        var request = new TokenRequestDtoBuilder()
            .ComDocumento(cpfValido)
            .ComSenha(senha)
            .Build();

        var usuario = new UsuarioModelBuilder()
            .ComDocumento(cpfValido)
            .ComSenhaHash(senhaHash)
            .Build();

        _fixture.UsuarioGatewayMock
            .AoObterUsuarioAtivo()
            .ComDocumento(cpfValido)
            .Retorna(usuario);

        _fixture.PasswordHasherMock
            .AoVerificarSenha()
            .ComSenhaEHash(senha, senhaHash, false); // Senha incorreta

        // Act
        var act = async () => await _fixture.AuthenticationService.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorType == ErrorType.Unauthorized)
            .WithMessage("Senha incorreta.");
    }

    [Fact(DisplayName = "Deve retornar TokenResponseDto quando credenciais forem válidas")]
    [Trait("Service", "AuthenticationService")]
    public async Task ValidateCredentialsAndGenerateTokenAsync_DeveRetornarTokenResponseDto_QuandoCredenciaisForemValidas()
    {
        // Arrange
        var cpfValido = "52998224725"; // CPF válido
        var senha = "Senha@123";
        var senhaHash = "hash-da-senha";
        var tokenEsperado = "jwt-token-mock";

        var request = new TokenRequestDtoBuilder()
            .ComDocumento(cpfValido)
            .ComSenha(senha)
            .Build();

        var usuario = new UsuarioModelBuilder()
            .ComDocumento(cpfValido)
            .ComSenhaHash(senhaHash)
            .Build();

        _fixture.UsuarioGatewayMock
            .AoObterUsuarioAtivo()
            .ComDocumento(cpfValido)
            .Retorna(usuario);

        _fixture.PasswordHasherMock
            .AoVerificarSenha()
            .ComSenhaEHash(senha, senhaHash, true); // Senha correta

        string? userIdCapturado = null;
        Guid? clienteIdCapturado = null;
        List<string>? rolesCapturadas = null;

        _fixture.TokenServiceMock
            .AoGerarToken()
            .RetornaComCallback(tokenEsperado, (userId, clienteId, roles) =>
            {
                userIdCapturado = userId;
                clienteIdCapturado = clienteId;
                rolesCapturadas = roles;
            });

        // Act
        var resultado = await _fixture.AuthenticationService.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Token.Should().Be(tokenEsperado);
        resultado.TokenType.Should().Be("Bearer");
        resultado.ExpiresIn.Should().Be(3600);

        // Validar parâmetros capturados
        userIdCapturado.Should().Be(usuario.Id.ToString());
        clienteIdCapturado.Should().Be(usuario.ClienteId);
        rolesCapturadas.Should().BeEquivalentTo(usuario.Roles);
    }

    [Fact(DisplayName = "Deve aceitar documento com máscara e limpar para validação")]
    [Trait("Service", "AuthenticationService")]
    public async Task ValidateCredentialsAndGenerateTokenAsync_DeveAceitarDocumentoComMascara_ELimparParaValidacao()
    {
        // Arrange
        var cpfComMascara = "529.982.247-25"; // CPF válido com máscara
        var cpfSemMascara = "52998224725";
        var senha = "Senha@123";
        var senhaHash = "hash-da-senha";
        var tokenEsperado = "jwt-token-mock";

        var request = new TokenRequestDtoBuilder()
            .ComDocumento(cpfComMascara)
            .ComSenha(senha)
            .Build();

        var usuario = new UsuarioModelBuilder()
            .ComDocumento(cpfSemMascara)
            .ComSenhaHash(senhaHash)
            .Build();

        // O gateway deve ser chamado com o documento original (com máscara)
        _fixture.UsuarioGatewayMock
            .AoObterUsuarioAtivo()
            .ComDocumento(cpfComMascara)
            .Retorna(usuario);

        _fixture.PasswordHasherMock
            .AoVerificarSenha()
            .ComSenhaEHash(senha, senhaHash, true);

        _fixture.TokenServiceMock
            .AoGerarToken()
            .Retorna(tokenEsperado);

        // Act
        var resultado = await _fixture.AuthenticationService.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Token.Should().Be(tokenEsperado);
    }

    [Fact(DisplayName = "Deve lançar DomainException Unauthorized quando documento tiver tamanho inválido")]
    [Trait("Service", "AuthenticationService")]
    public async Task ValidateCredentialsAndGenerateTokenAsync_DeveLancarDomainException_Unauthorized_QuandoDocumentoTiverTamanhoInvalido()
    {
        // Arrange
        var request = new TokenRequestDtoBuilder()
            .ComDocumento("123456789012") // 12 dígitos - nem CPF nem CNPJ
            .Build();

        // Act
        var act = async () => await _fixture.AuthenticationService.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorType == ErrorType.Unauthorized)
            .WithMessage("Documento identificador inválido.");
    }

    [Fact(DisplayName = "Deve lançar DomainException Unauthorized quando CPF tiver todos dígitos iguais")]
    [Trait("Service", "AuthenticationService")]
    public async Task ValidateCredentialsAndGenerateTokenAsync_DeveLancarDomainException_Unauthorized_QuandoCpfTiverTodosDigitosIguais()
    {
        // Arrange
        var request = new TokenRequestDtoBuilder()
            .ComDocumento("11111111111")
            .Build();

        // Act
        var act = async () => await _fixture.AuthenticationService.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorType == ErrorType.Unauthorized)
            .WithMessage("Documento identificador inválido.");
    }

    [Fact(DisplayName = "Deve lançar DomainException Unauthorized quando CNPJ tiver todos dígitos iguais")]
    [Trait("Service", "AuthenticationService")]
    public async Task ValidateCredentialsAndGenerateTokenAsync_DeveLancarDomainException_Unauthorized_QuandoCnpjTiverTodosDigitosIguais()
    {
        // Arrange
        var request = new TokenRequestDtoBuilder()
            .ComDocumento("11111111111111")
            .Build();

        // Act
        var act = async () => await _fixture.AuthenticationService.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorType == ErrorType.Unauthorized)
            .WithMessage("Documento identificador inválido.");
    }
}
