using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using FluentAssertions;
using Infrastructure.Authentication;
using Shared.Enums;
using Tests.SharedHelpers.Builders;
using Tests.SharedHelpers.Fixtures;
using Tests.SharedHelpers.Lambda;
using Tests.SharedHelpers.Mocks;

namespace Tests.Unit.Handlers;

public class LoginHandlerTest
{
    private readonly LoginHandlerTestFixture _fixture = new();
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact(DisplayName = "FunctionHandler deve retornar 400 quando Body for nulo")]
    [Trait("Handler", "LoginHandler")]
    public async Task FunctionHandler_DeveRetornar400_QuandoBodyForNulo()
    {
        // Arrange
        var request = new ApiGatewayProxyRequestBuilder()
            .Build(); // Sem body (null por padrão)
        
        var context = LambdaContextBuilder.Criar();

        // Act
        var response = await _fixture.LoginHandler.FunctionHandler(request, context);

        // Assert
        response.StatusCode.Should().Be(400);
        response.Headers.Should().ContainKey("Content-Type");
        response.Headers["Content-Type"].Should().Be("application/json");
        
        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(response.Body);
        errorResponse.Should().ContainKey("error");
        errorResponse!["error"].Should().Be("Request body é obrigatório");
    }

    [Fact(DisplayName = "FunctionHandler deve retornar 400 quando JSON do Body for inválido")]
    [Trait("Handler", "LoginHandler")]
    public async Task FunctionHandler_DeveRetornar400_QuandoJsonDoBodyForInvalido()
    {
        // Arrange
        var request = new ApiGatewayProxyRequestBuilder()
            .ComBody("{not json}")
            .Build();
        
        var context = LambdaContextBuilder.Criar();

        // Act
        var response = await _fixture.LoginHandler.FunctionHandler(request, context);

        // Assert
        response.StatusCode.Should().Be(400);
        response.Headers.Should().ContainKey("Content-Type");
        response.Headers["Content-Type"].Should().Be("application/json");
        
        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(response.Body);
        errorResponse.Should().ContainKey("error");
        errorResponse!["error"].Should().Be("Request inválido");
    }

    [Fact(DisplayName = "FunctionHandler deve retornar 400 quando TokenRequestDto não for desserializável")]
    [Trait("Handler", "LoginHandler")]
    public async Task FunctionHandler_DeveRetornar400_QuandoTokenRequestDtoNaoForDesserializavel()
    {
        // Arrange - body vazio retorna null na desserialização
        var request = new ApiGatewayProxyRequestBuilder()
            .ComBody("")
            .Build();
        
        var context = LambdaContextBuilder.Criar();

        // Act
        var response = await _fixture.LoginHandler.FunctionHandler(request, context);

        // Assert
        response.StatusCode.Should().Be(400);
        response.Headers.Should().ContainKey("Content-Type");
        response.Headers["Content-Type"].Should().Be("application/json");
        
        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(response.Body);
        errorResponse.Should().ContainKey("error");
        errorResponse!["error"].Should().Be("Request inválido");
    }

    [Fact(DisplayName = "FunctionHandler deve retornar 401 quando AuthenticationService lançar Unauthorized")]
    [Trait("Handler", "LoginHandler")]
    public async Task FunctionHandler_DeveRetornar401_QuandoAuthenticationServiceLancarUnauthorized()
    {
        // Arrange
        var tokenRequest = new TokenRequestDtoBuilder()
            .ComDocumento("12345678901")
            .ComSenha("senha-incorreta")
            .Build();

        var request = new ApiGatewayProxyRequestBuilder()
            .ComBody(JsonSerializer.Serialize(tokenRequest))
            .Build();
        
        var context = LambdaContextBuilder.Criar();

        _fixture.AuthenticationServiceMock
            .AoValidarCredenciais()
            .ComRequest(tokenRequest)
            .LancaDomainException(ErrorType.Unauthorized, "Credenciais inválidas");

        // Act
        var response = await _fixture.LoginHandler.FunctionHandler(request, context);

        // Assert
        response.StatusCode.Should().Be(401);
        response.Headers.Should().ContainKey("Content-Type");
        response.Headers["Content-Type"].Should().Be("application/json");
        
        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(response.Body);
        errorResponse.Should().ContainKey("error");
        errorResponse!["error"].Should().Be("Credenciais inválidas");
    }

    [Fact(DisplayName = "FunctionHandler deve retornar 400 quando AuthenticationService lançar InvalidInput")]
    [Trait("Handler", "LoginHandler")]
    public async Task FunctionHandler_DeveRetornar400_QuandoAuthenticationServiceLancarInvalidInput()
    {
        // Arrange
        var tokenRequest = new TokenRequestDtoBuilder()
            .ComDocumento("")
            .ComSenha("senha")
            .Build();

        var request = new ApiGatewayProxyRequestBuilder()
            .ComBody(JsonSerializer.Serialize(tokenRequest))
            .Build();
        
        var context = LambdaContextBuilder.Criar();

        _fixture.AuthenticationServiceMock
            .AoValidarCredenciais()
            .ComRequest(tokenRequest)
            .LancaDomainException(ErrorType.InvalidInput, "Documento identificador e senha são obrigatórios.");

        // Act
        var response = await _fixture.LoginHandler.FunctionHandler(request, context);

        // Assert
        response.StatusCode.Should().Be(400);
        response.Headers.Should().ContainKey("Content-Type");
        response.Headers["Content-Type"].Should().Be("application/json");
        
        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(response.Body);
        errorResponse.Should().ContainKey("error");
        errorResponse!["error"].Should().Be("Documento identificador e senha são obrigatórios.");
    }

    [Fact(DisplayName = "FunctionHandler deve retornar 500 quando ocorrer exceção inesperada")]
    [Trait("Handler", "LoginHandler")]
    public async Task FunctionHandler_DeveRetornar500_QuandoOcorrerExcecaoInesperada()
    {
        // Arrange
        var tokenRequest = new TokenRequestDtoBuilder()
            .ComDocumento("12345678901")
            .ComSenha("senha")
            .Build();

        var request = new ApiGatewayProxyRequestBuilder()
            .ComBody(JsonSerializer.Serialize(tokenRequest))
            .Build();
        
        var context = LambdaContextBuilder.Criar();

        _fixture.AuthenticationServiceMock
            .AoValidarCredenciais()
            .ComRequest(tokenRequest)
            .LancaExcecao(new InvalidOperationException("Erro inesperado"));

        // Act
        var response = await _fixture.LoginHandler.FunctionHandler(request, context);

        // Assert
        response.StatusCode.Should().Be(500);
        response.Headers.Should().ContainKey("Content-Type");
        response.Headers["Content-Type"].Should().Be("application/json");
        
        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(response.Body);
        errorResponse.Should().ContainKey("error");
        errorResponse!["error"].Should().Be("Erro interno no servidor");
    }

    [Fact(DisplayName = "FunctionHandler deve retornar 200 com TokenResponseDto quando sucesso")]
    [Trait("Handler", "LoginHandler")]
    public async Task FunctionHandler_DeveRetornar200_ComTokenResponseDto_QuandoSucesso()
    {
        // Arrange
        var tokenRequest = new TokenRequestDtoBuilder()
            .ComDocumento("52998224725")
            .ComSenha("Senha@123")
            .Build();

        var tokenResponse = new TokenResponseDto(
            Token: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.mock",
            TokenType: "Bearer",
            ExpiresIn: 3600
        );

        var request = new ApiGatewayProxyRequestBuilder()
            .ComBody(JsonSerializer.Serialize(tokenRequest))
            .Build();
        
        var context = LambdaContextBuilder.Criar();

        _fixture.AuthenticationServiceMock
            .AoValidarCredenciais()
            .ComRequest(tokenRequest)
            .Retorna(tokenResponse);

        // Act
        var response = await _fixture.LoginHandler.FunctionHandler(request, context);

        // Assert
        response.StatusCode.Should().Be(200);
        response.Headers.Should().ContainKey("Content-Type");
        response.Headers["Content-Type"].Should().Be("application/json");
        
        var resultTokenResponse = JsonSerializer.Deserialize<TokenResponseDto>(response.Body, JsonOptions);
        
        resultTokenResponse.Should().NotBeNull();
        resultTokenResponse!.Token.Should().NotBeNullOrEmpty();
        resultTokenResponse.Token.Should().Be(tokenResponse.Token);
        resultTokenResponse.TokenType.Should().Be("Bearer");
        resultTokenResponse.ExpiresIn.Should().Be(3600);
    }

    [Fact(DisplayName = "Deve construir LoginHandler com construtor padrão e inicializar DI")]
    [Trait("Handler", "LoginHandler")]
    public void DeveConstruirLoginHandlerComConstrutorPadraoEInicializarDI()
    {
        // Arrange
        var originalDirectory = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            // Act
            var handler = new AuthLambda.LoginHandler();

            // Assert
            handler.Should().NotBeNull();
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    [Fact(DisplayName = "FunctionHandler deve retornar 400 quando Body deserializar para null")]
    [Trait("Handler", "LoginHandler")]
    public async Task FunctionHandler_DeveRetornar400_QuandoBodyDeserializarParaNull()
    {
        // Arrange
        var request = new ApiGatewayProxyRequestBuilder()
            .ComBody("null") // JSON literal null
            .Build();
        
        var context = LambdaContextBuilder.Criar();

        // Act
        var response = await _fixture.LoginHandler.FunctionHandler(request, context);

        // Assert
        response.StatusCode.Should().Be(400);
        response.Headers.Should().ContainKey("Content-Type");
        response.Headers["Content-Type"].Should().Be("application/json");
        
        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(response.Body);
        errorResponse.Should().ContainKey("error");
        errorResponse!["error"].Should().Be("Request inválido");
    }

    [Fact(DisplayName = "FunctionHandler deve retornar 400 quando Request for null")]
    [Trait("Handler", "LoginHandler")]
    public async Task FunctionHandler_DeveRetornar400_QuandoRequestForNull()
    {
        // Arrange
        APIGatewayProxyRequest request = null!;
        var context = LambdaContextBuilder.Criar();

        // Act
        var response = await _fixture.LoginHandler.FunctionHandler(request, context);

        // Assert
        response.StatusCode.Should().Be(400);
        response.Headers.Should().ContainKey("Content-Type");
        response.Headers["Content-Type"].Should().Be("application/json");
        
        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(response.Body);
        errorResponse.Should().ContainKey("error");
        errorResponse!["error"].Should().Be("Request body é obrigatório");
    }
}
