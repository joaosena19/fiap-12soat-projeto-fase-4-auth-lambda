using FluentAssertions;
using Tests.SharedHelpers.Builders;

namespace Tests.Unit.MetaTests.SharedHelpers;

public class ApiGatewayHttpApiV2RequestBuilderTest
{
    [Fact(DisplayName = "Build deve retornar APIGatewayHttpApiV2ProxyRequest não nulo")]
    public void Build_DeveRetornarAPIGatewayHttpApiV2ProxyRequestNaoNulo()
    {
        // Act
        var request = new ApiGatewayHttpApiV2RequestBuilder().Build();

        // Assert
        request.Should().NotBeNull();
        request.Headers.Should().NotBeNull();
    }

    [Fact(DisplayName = "ComHeaders deve permitir sobrescrever Headers")]
    public void ComHeaders_DevePermitirSobrescreverHeaders()
    {
        // Arrange
        var headersEsperados = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" },
            { "authorization", "Bearer token123" }
        };

        // Act
        var request = new ApiGatewayHttpApiV2RequestBuilder()
            .ComHeaders(headersEsperados)
            .Build();

        // Assert
        request.Headers.Should().BeEquivalentTo(headersEsperados);
    }

    [Fact(DisplayName = "ComAuthorizationHeader deve adicionar header de autorização")]
    public void ComAuthorizationHeader_DeveAdicionarHeaderDeAutorizacao()
    {
        // Arrange
        const string token = "Bearer meu.token.jwt";

        // Act
        var request = new ApiGatewayHttpApiV2RequestBuilder()
            .ComAuthorizationHeader(token)
            .Build();

        // Assert
        request.Headers.Should().ContainKey("authorization");
        request.Headers["authorization"].Should().Be(token);
    }
}
