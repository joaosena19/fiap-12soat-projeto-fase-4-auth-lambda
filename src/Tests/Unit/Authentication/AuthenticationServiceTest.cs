using FluentAssertions;
using Infrastructure.Authentication;
using Infrastructure.Authentication.PasswordHashing;
using Infrastructure.Gateways.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using Shared.Enums;
using Shared.Exceptions;
using Tests.SharedHelpers.Builders;
using Tests.SharedHelpers.Mocks;
using Xunit;

namespace Tests.Unit.Authentication;

public class AuthenticationServiceTest
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IUsuarioGateway> _usuarioGatewayMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly AuthenticationService _service;

    public AuthenticationServiceTest()
    {
        _configurationMock = new Mock<IConfiguration>();
        _tokenServiceMock = new Mock<ITokenService>();
        _usuarioGatewayMock = new Mock<IUsuarioGateway>();
        _passwordHasherMock = new Mock<IPasswordHasher>();

        _service = new AuthenticationService(
            _configurationMock.Object,
            _tokenServiceMock.Object,
            _usuarioGatewayMock.Object,
            _passwordHasherMock.Object
        );
    }

    [Fact(DisplayName = "Deve lançar DomainException InvalidInput quando documento for vazio")]
    public async Task ValidateCredentialsAndGenerateTokenAsync_DeveLancarDomainException_InvalidInput_QuandoDocumentoForVazio()
    {
        // Arrange
        var request = new TokenRequestDtoBuilder()
            .ComDocumento("")
            .Build();

        // Act
        var act = async () => await _service.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorType == ErrorType.InvalidInput)
            .WithMessage("Documento identificador e senha são obrigatórios.");
    }

    [Fact(DisplayName = "Deve lançar DomainException InvalidInput quando senha for vazia")]
    public async Task ValidateCredentialsAndGenerateTokenAsync_DeveLancarDomainException_InvalidInput_QuandoSenhaForVazia()
    {
        // Arrange
        var request = new TokenRequestDtoBuilder()
            .ComSenha("")
            .Build();

        // Act
        var act = async () => await _service.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorType == ErrorType.InvalidInput)
            .WithMessage("Documento identificador e senha são obrigatórios.");
    }

    [Fact(DisplayName = "Deve lançar DomainException Unauthorized quando CPF for inválido")]
    public async Task ValidateCredentialsAndGenerateTokenAsync_DeveLancarDomainException_Unauthorized_QuandoCpfForInvalido()
    {
        // Arrange
        var request = new TokenRequestDtoBuilder()
            .ComDocumento("12345678901") // CPF inválido
            .Build();

        // Act
        var act = async () => await _service.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorType == ErrorType.Unauthorized)
            .WithMessage("Documento identificador inválido.");
    }

    [Fact(DisplayName = "Deve lançar DomainException Unauthorized quando CNPJ for inválido")]
    public async Task ValidateCredentialsAndGenerateTokenAsync_DeveLancarDomainException_Unauthorized_QuandoCnpjForInvalido()
    {
        // Arrange
        var request = new TokenRequestDtoBuilder()
            .ComDocumento("12345678000100") // CNPJ inválido
            .Build();

        // Act
        var act = async () => await _service.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorType == ErrorType.Unauthorized)
            .WithMessage("Documento identificador inválido.");
    }

    [Fact(DisplayName = "Deve lançar DomainException Unauthorized quando usuário não for encontrado")]
    public async Task ValidateCredentialsAndGenerateTokenAsync_DeveLancarDomainException_Unauthorized_QuandoUsuarioNaoForEncontrado()
    {
        // Arrange
        var cpfValido = "52998224725"; // CPF válido
        var request = new TokenRequestDtoBuilder()
            .ComDocumento(cpfValido)
            .Build();

        _usuarioGatewayMock
            .AoObterUsuarioAtivo()
            .ComDocumento(cpfValido)
            .NaoRetornaNada();

        // Act
        var act = async () => await _service.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorType == ErrorType.Unauthorized)
            .WithMessage("Usuário não encontrado ou inativo.");
    }

    [Fact(DisplayName = "Deve lançar DomainException Unauthorized quando senha for incorreta")]
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

        _usuarioGatewayMock
            .AoObterUsuarioAtivo()
            .ComDocumento(cpfValido)
            .Retorna(usuario);

        _passwordHasherMock
            .AoVerificarSenha()
            .ComSenhaEHash(senha, senhaHash, false); // Senha incorreta

        // Act
        var act = async () => await _service.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorType == ErrorType.Unauthorized)
            .WithMessage("Senha incorreta.");
    }

    [Fact(DisplayName = "Deve retornar TokenResponseDto quando credenciais forem válidas")]
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

        _usuarioGatewayMock
            .AoObterUsuarioAtivo()
            .ComDocumento(cpfValido)
            .Retorna(usuario);

        _passwordHasherMock
            .AoVerificarSenha()
            .ComSenhaEHash(senha, senhaHash, true); // Senha correta

        string? userIdCapturado = null;
        Guid? clienteIdCapturado = null;
        List<string>? rolesCapturadas = null;

        _tokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<List<string>>()))
            .Returns(tokenEsperado)
            .Callback<string, Guid?, List<string>>((userId, clienteId, roles) =>
            {
                userIdCapturado = userId;
                clienteIdCapturado = clienteId;
                rolesCapturadas = roles;
            });

        // Act
        var resultado = await _service.ValidateCredentialsAndGenerateTokenAsync(request);

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
        _usuarioGatewayMock
            .AoObterUsuarioAtivo()
            .ComDocumento(cpfComMascara)
            .Retorna(usuario);

        _passwordHasherMock
            .AoVerificarSenha()
            .ComSenhaEHash(senha, senhaHash, true);

        _tokenServiceMock
            .AoGerarToken()
            .Retorna(tokenEsperado);

        // Act
        var resultado = await _service.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Token.Should().Be(tokenEsperado);
    }

    [Fact(DisplayName = "Deve lançar DomainException Unauthorized quando documento tiver tamanho inválido")]
    public async Task ValidateCredentialsAndGenerateTokenAsync_DeveLancarDomainException_Unauthorized_QuandoDocumentoTiverTamanhoInvalido()
    {
        // Arrange
        var request = new TokenRequestDtoBuilder()
            .ComDocumento("123456789012") // 12 dígitos - nem CPF nem CNPJ
            .Build();

        // Act
        var act = async () => await _service.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorType == ErrorType.Unauthorized)
            .WithMessage("Documento identificador inválido.");
    }

    [Fact(DisplayName = "Deve lançar DomainException Unauthorized quando CPF tiver todos dígitos iguais")]
    public async Task ValidateCredentialsAndGenerateTokenAsync_DeveLancarDomainException_Unauthorized_QuandoCpfTiverTodosDigitosIguais()
    {
        // Arrange
        var request = new TokenRequestDtoBuilder()
            .ComDocumento("11111111111") // CPF com todos dígitos iguais
            .Build();

        // Act
        var act = async () => await _service.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorType == ErrorType.Unauthorized)
            .WithMessage("Documento identificador inválido.");
    }

    [Fact(DisplayName = "Deve lançar DomainException Unauthorized quando CNPJ tiver todos dígitos iguais")]
    public async Task ValidateCredentialsAndGenerateTokenAsync_DeveLancarDomainException_Unauthorized_QuandoCnpjTiverTodosDigitosIguais()
    {
        // Arrange
        var request = new TokenRequestDtoBuilder()
            .ComDocumento("11111111111111") // CNPJ com todos dígitos iguais
            .Build();

        // Act
        var act = async () => await _service.ValidateCredentialsAndGenerateTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorType == ErrorType.Unauthorized)
            .WithMessage("Documento identificador inválido.");
    }
}
