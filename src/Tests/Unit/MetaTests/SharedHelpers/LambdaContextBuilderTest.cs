using Amazon.Lambda.Core;
using FluentAssertions;
using Tests.SharedHelpers.Lambda;

namespace Tests.Unit.MetaTests.SharedHelpers;

public class LambdaContextBuilderTest
{
    [Fact(DisplayName = "Criar deve retornar ILambdaContext não nulo")]
    public void Criar_DeveRetornarILambdaContextNaoNulo()
    {
        // Act
        var context = LambdaContextBuilder.Criar();

        // Assert
        context.Should().NotBeNull();
        context.Logger.Should().NotBeNull();
    }

    [Fact(DisplayName = "Logger deve ser funcional e não lançar exceção ao logar")]
    public void Logger_DeveSerFuncionalENaoLancarExcecao()
    {
        // Arrange
        var context = LambdaContextBuilder.Criar();

        // Act
        Action act = () =>
        {
            context.Logger.LogInformation("Teste de informação");
            context.Logger.LogWarning("Teste de warning");
            context.Logger.LogError("Teste de erro");
        };

        // Assert
        act.Should().NotThrow();
    }

    [Fact(DisplayName = "ComLogger deve permitir sobrescrever Logger")]
    public void ComLogger_DevePermitirSobrescreverLogger()
    {
        // Arrange
        var loggerEsperado = new LambdaLoggerSpy();

        // Act
        var context = new LambdaContextBuilder()
            .ComLogger(loggerEsperado)
            .Build();

        // Assert
        context.Logger.Should().BeSameAs(loggerEsperado);
    }
}
