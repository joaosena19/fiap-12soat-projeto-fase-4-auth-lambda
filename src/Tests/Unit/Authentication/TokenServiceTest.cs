using FluentAssertions;
using Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Tests.SharedHelpers.Builders;

namespace Tests.Unit.Authentication;

public class TokenServiceTest
{
    [Fact(DisplayName = "Deve lançar InvalidOperationException quando Jwt:Key estiver ausente")]
    [Trait("Service", "TokenService")]
    public void GenerateToken_DeveLancarInvalidOperationException_QuandoJwtKeyEstiverAusente()
    {
        // Arrange
        var valores = new Dictionary<string, string?>
        {
            { "Jwt:Issuer", "OficinaMecanicaApi" },
            { "Jwt:Audience", "AuthorizedServices" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(valores)
            .Build();

        var service = new TokenService(configuration);

        // Act
        var act = () => service.GenerateToken("user123", null, new List<string>());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Configuração JWT está ausente");
    }

    [Fact(DisplayName = "Deve lançar InvalidOperationException quando Jwt:Issuer estiver ausente")]
    [Trait("Service", "TokenService")]
    public void GenerateToken_DeveLancarInvalidOperationException_QuandoJwtIssuerEstiverAusente()
    {
        // Arrange
        var valores = new Dictionary<string, string?>
        {
            { "Jwt:Key", "xmvsLe9QxIR3BWAWJW4wL+5ZfZrYaohxUaRYSkxteiAn5qEAKDd3xCMn1Bk46ndy6sl4gkVXXvEP/1JowbBp/g==" },
            { "Jwt:Audience", "AuthorizedServices" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(valores)
            .Build();

        var service = new TokenService(configuration);

        // Act
        var act = () => service.GenerateToken("user123", null, new List<string>());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Configuração JWT está ausente");
    }

    [Fact(DisplayName = "Deve lançar InvalidOperationException quando Jwt:Audience estiver ausente")]
    [Trait("Service", "TokenService")]
    public void GenerateToken_DeveLancarInvalidOperationException_QuandoJwtAudienceEstiverAusente()
    {
        // Arrange
        var valores = new Dictionary<string, string?>
        {
            { "Jwt:Key", "xmvsLe9QxIR3BWAWJW4wL+5ZfZrYaohxUaRYSkxteiAn5qEAKDd3xCMn1Bk46ndy6sl4gkVXXvEP/1JowbBp/g==" },
            { "Jwt:Issuer", "OficinaMecanicaApi" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(valores)
            .Build();

        var service = new TokenService(configuration);

        // Act
        var act = () => service.GenerateToken("user123", null, new List<string>());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Configuração JWT está ausente");
    }

    [Fact(DisplayName = "Deve gerar JWT com claims sub e userId")]
    [Trait("Service", "TokenService")]
    public void GenerateToken_DeveGerarJwtComSubEUserId()
    {
        // Arrange
        var configuration = ConfiguracaoBuilder.CriarConfiguracao();
        var service = new TokenService(configuration);
        var userId = "user123";

        // Act
        var token = service.GenerateToken(userId, null, new List<string>());

        // Assert
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId);
        jwtToken.Claims.Should().Contain(c => c.Type == "userId" && c.Value == userId);
    }

    [Fact(DisplayName = "Deve incluir clienteId quando for informado")]
    [Trait("Service", "TokenService")]
    public void GenerateToken_DeveIncluirClienteId_QuandoForInformado()
    {
        // Arrange
        var configuration = ConfiguracaoBuilder.CriarConfiguracao();
        var service = new TokenService(configuration);
        var userId = "user123";
        var clienteId = Guid.NewGuid();

        // Act
        var token = service.GenerateToken(userId, clienteId, new List<string>());

        // Assert
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwtToken.Claims.Should().Contain(c => c.Type == "clienteId" && c.Value == clienteId.ToString());
    }

    [Fact(DisplayName = "Não deve incluir clienteId quando for nulo")]
    [Trait("Service", "TokenService")]
    public void GenerateToken_NaoDeveIncluirClienteId_QuandoForNulo()
    {
        // Arrange
        var configuration = ConfiguracaoBuilder.CriarConfiguracao();
        var service = new TokenService(configuration);
        var userId = "user123";

        // Act
        var token = service.GenerateToken(userId, null, new List<string>());

        // Assert
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwtToken.Claims.Should().NotContain(c => c.Type == "clienteId");
    }

    [Fact(DisplayName = "Deve incluir roles como claims")]
    [Trait("Service", "TokenService")]
    public void GenerateToken_DeveIncluirRolesComoClaims()
    {
        // Arrange
        var configuration = ConfiguracaoBuilder.CriarConfiguracao();
        var service = new TokenService(configuration);
        var userId = "user123";
        var roles = new List<string> { "Administrador", "Cliente" };

        // Act
        var token = service.GenerateToken(userId, null, roles);

        // Assert
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Administrador");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Cliente");
    }

    [Fact(DisplayName = "Deve expirar em aproximadamente uma hora")]
    [Trait("Service", "TokenService")]
    public void GenerateToken_DeveExpirarEmAproximadamenteUmaHora()
    {
        // Arrange
        var configuration = ConfiguracaoBuilder.CriarConfiguracao();
        var service = new TokenService(configuration);
        var userId = "user123";

        // Act
        var token = service.GenerateToken(userId, null, new List<string>());

        // Assert
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var expectedExpiration = DateTime.UtcNow.AddHours(1);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(2));
    }
}
