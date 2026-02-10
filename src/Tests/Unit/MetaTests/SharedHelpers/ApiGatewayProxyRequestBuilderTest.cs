using FluentAssertions;
using Tests.SharedHelpers.Builders;

namespace Tests.Unit.MetaTests.SharedHelpers;

public class ApiGatewayProxyRequestBuilderTest
{
    [Fact(DisplayName = "Build deve retornar APIGatewayProxyRequest não nulo")]
    public void Build_DeveRetornarAPIGatewayProxyRequestNaoNulo()
    {
        // Act
        var request = new ApiGatewayProxyRequestBuilder().Build();

        // Assert
        request.Should().NotBeNull();
        request.Headers.Should().NotBeNull();
    }

    [Fact(DisplayName = "ComBody deve permitir sobrescrever Body")]
    public void ComBody_DevePermitirSobrescreverBody()
    {
        // Arrange
        const string bodyEsperado = "{\"test\":\"value\"}";

        // Act
        var request = new ApiGatewayProxyRequestBuilder()
            .ComBody(bodyEsperado)
            .Build();

        // Assert
        request.Body.Should().Be(bodyEsperado);
    }

    [Fact(DisplayName = "ComHeaders deve permitir sobrescrever Headers")]
    public void ComHeaders_DevePermitirSobrescreverHeaders()
    {
        // Arrange
        var headersEsperados = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" },
            { "Authorization", "Bearer token123" }
        };

        // Act
        var request = new ApiGatewayProxyRequestBuilder()
            .ComHeaders(headersEsperados)
            .Build();

        // Assert
        request.Headers.Should().BeEquivalentTo(headersEsperados);
    }
}
