using FluentAssertions;
using Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tests.SharedHelpers.Builders;
using Tests.SharedHelpers.Lambda;

namespace Tests.Unit.Handlers;

public class AuthorizerHandlerTest
{
    [Fact(DisplayName = "FunctionHandler deve retornar não autorizado quando header Authorization estiver ausente")]
    [Trait("Handler", "AuthorizerHandler")]
    public void FunctionHandler_DeveRetornarNaoAutorizado_QuandoHeaderAuthorizationEstiverAusente()
    {
        // Arrange
        var configuration = ConfiguracaoBuilder.CriarConfiguracao();
        var handler = new AuthLambda.AuthorizerHandler(configuration);
        
        var request = new ApiGatewayHttpApiV2RequestBuilder()
            .Build(); // Sem header Authorization
        
        var context = LambdaContextBuilder.Criar();

        // Act
        var response = handler.FunctionHandler(request, context);

        // Assert
        response.Should().ContainKey("isAuthorized");
        response["isAuthorized"].Should().Be(false);
    }

    [Fact(DisplayName = "FunctionHandler deve retornar não autorizado quando header Authorization estiver vazio")]
    [Trait("Handler", "AuthorizerHandler")]
    public void FunctionHandler_DeveRetornarNaoAutorizado_QuandoHeaderAuthorizationEstiverVazio()
    {
        // Arrange
        var configuration = ConfiguracaoBuilder.CriarConfiguracao();
        var handler = new AuthLambda.AuthorizerHandler(configuration);
        
        var request = new ApiGatewayHttpApiV2RequestBuilder()
            .ComAuthorizationHeader("")
            .Build();
        
        var context = LambdaContextBuilder.Criar();

        // Act
        var response = handler.FunctionHandler(request, context);

        // Assert
        response.Should().ContainKey("isAuthorized");
        response["isAuthorized"].Should().Be(false);
    }

    [Fact(DisplayName = "FunctionHandler deve aceitar header Authorization com Bearer repetido e validar token")]
    [Trait("Handler", "AuthorizerHandler")]
    public void FunctionHandler_DeveAceitarHeaderAuthorizationComBearerRepetido_EValidarToken()
    {
        // Arrange
        var configuration = ConfiguracaoBuilder.CriarConfiguracao();
        var tokenService = new TokenService(configuration);
        var userId = Guid.NewGuid().ToString();
        var roles = new List<string> { "Administrador" };
        var token = tokenService.GenerateToken(userId, null, roles);
        
        var handler = new AuthLambda.AuthorizerHandler(configuration);
        
        var request = new ApiGatewayHttpApiV2RequestBuilder()
            .ComAuthorizationHeader($"Bearer Bearer {token}") // Bearer repetido
            .Build();
        
        var context = LambdaContextBuilder.Criar();

        // Act
        var response = handler.FunctionHandler(request, context);

        // Assert
        response["isAuthorized"].Should().Be(true);
        response.Should().ContainKey("context");
        var contextDict = response["context"] as Dictionary<string, object>;
        contextDict.Should().NotBeNull();
        contextDict!["sub"].Should().Be(userId);
        contextDict["role"].Should().Be("Administrador");
    }

    [Fact(DisplayName = "FunctionHandler deve ler header Authorization com chave lowercase ou uppercase")]
    [Trait("Handler", "AuthorizerHandler")]
    public void FunctionHandler_DeveLerHeaderAuthorization_ComChaveLowercaseOuUppercase()
    {
        // Arrange
        var configuration = ConfiguracaoBuilder.CriarConfiguracao();
        var tokenService = new TokenService(configuration);
        var userId = Guid.NewGuid().ToString();
        var roles = new List<string> { "Sistema" };
        var token = tokenService.GenerateToken(userId, null, roles);
        
        var handler = new AuthLambda.AuthorizerHandler(configuration);
        
        // Teste com lowercase "authorization"
        var requestLowercase = new ApiGatewayHttpApiV2RequestBuilder()
            .ComAuthorizationHeader($"Bearer {token}")
            .Build();
        
        // Teste com uppercase "Authorization" (criando manualmente)
        var requestUppercase = new ApiGatewayHttpApiV2RequestBuilder()
            .ComHeaders(new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" })
            .Build();
        
        var context = LambdaContextBuilder.Criar();

        // Act
        var responseLowercase = handler.FunctionHandler(requestLowercase, context);
        var responseUppercase = handler.FunctionHandler(requestUppercase, context);

        // Assert
        responseLowercase["isAuthorized"].Should().Be(true);
        responseUppercase["isAuthorized"].Should().Be(true);
        
        var contextLowercase = responseLowercase["context"] as Dictionary<string, object>;
        contextLowercase!["sub"].Should().Be(userId);
        
        var contextUppercase = responseUppercase["context"] as Dictionary<string, object>;
        contextUppercase!["sub"].Should().Be(userId);
    }

    [Fact(DisplayName = "FunctionHandler deve retornar não autorizado quando token for inválido ou expirado")]
    [Trait("Handler", "AuthorizerHandler")]
    public void FunctionHandler_DeveRetornarNaoAutorizado_QuandoTokenForInvalidoOuExpirado()
    {
        // Arrange
        var configuration = ConfiguracaoBuilder.CriarConfiguracao();
        var handler = new AuthLambda.AuthorizerHandler(configuration);
        
        // Criar token expirado além do ClockSkew (5 minutos)
        var tokenExpirado = CriarTokenComExpiracao(configuration, DateTime.UtcNow.AddMinutes(-10));
        
        var request = new ApiGatewayHttpApiV2RequestBuilder()
            .ComAuthorizationHeader($"Bearer {tokenExpirado}")
            .Build();
        
        var context = LambdaContextBuilder.Criar();

        // Act
        var response = handler.FunctionHandler(request, context);

        // Assert
        response.Should().ContainKey("isAuthorized");
        response["isAuthorized"].Should().Be(false);
    }

    [Fact(DisplayName = "FunctionHandler deve retornar autorizado com contexto sub e role quando token for válido")]
    [Trait("Handler", "AuthorizerHandler")]
    public void FunctionHandler_DeveRetornarAutorizado_ComContextoSubERole_QuandoTokenForValido()
    {
        // Arrange
        var configuration = ConfiguracaoBuilder.CriarConfiguracao();
        var tokenService = new TokenService(configuration);
        var userId = Guid.NewGuid().ToString();
        var roles = new List<string> { "Administrador", "Sistema" }; // Múltiplas roles
        var token = tokenService.GenerateToken(userId, null, roles);
        
        var handler = new AuthLambda.AuthorizerHandler(configuration);
        
        var request = new ApiGatewayHttpApiV2RequestBuilder()
            .ComAuthorizationHeader($"Bearer {token}")
            .Build();
        
        var context = LambdaContextBuilder.Criar();

        // Act
        var response = handler.FunctionHandler(request, context);

        // Assert
        response["isAuthorized"].Should().Be(true);
        response.Should().ContainKey("context");
        
        var contextDict = response["context"] as Dictionary<string, object>;
        contextDict.Should().NotBeNull();
        contextDict!["sub"].Should().Be(userId);
        contextDict["role"].Should().Be("Administrador"); // Primeira role
    }

    [Fact(DisplayName = "FunctionHandler deve retornar não autorizado quando configuração JWT estiver incompleta")]
    [Trait("Handler", "AuthorizerHandler")]
    public void FunctionHandler_DeveRetornarNaoAutorizado_QuandoConfiguracaoJwtEstiverIncompleta()
    {
        // Arrange - Configuração incompleta (sem Jwt:Key)
        var configIncompleta = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Issuer", "OficinaMecanicaApi" },
                { "Jwt:Audience", "AuthorizedServices" }
                // Jwt:Key ausente
            })
            .Build();
        
        var handler = new AuthLambda.AuthorizerHandler(configIncompleta);
        
        var request = new ApiGatewayHttpApiV2RequestBuilder()
            .ComAuthorizationHeader("Bearer qualquertoken123")
            .Build();
        
        var context = LambdaContextBuilder.Criar();

        // Act
        var response = handler.FunctionHandler(request, context);

        // Assert
        response.Should().ContainKey("isAuthorized");
        response["isAuthorized"].Should().Be(false);
    }

    // Helper para criar token com expiração customizada
    private static string CriarTokenComExpiracao(IConfiguration configuration, DateTime expiracao)
    {
        var key = configuration["Jwt:Key"];
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "TestRole")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiracao,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
