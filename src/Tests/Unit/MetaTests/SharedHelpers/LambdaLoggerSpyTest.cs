using FluentAssertions;
using Tests.SharedHelpers.Lambda;

namespace Tests.Unit.MetaTests.SharedHelpers;

public class LambdaLoggerSpyTest
{
    [Fact(DisplayName = "LogInformation deve capturar mensagem")]
    public void LogInformation_DeveCapturarMensagem()
    {
        // Arrange
        var logger = new LambdaLoggerSpy();
        const string mensagem = "Mensagem de informação";

        // Act
        logger.LogInformation(mensagem);

        // Assert
        logger.InformationLogs.Should().ContainSingle().Which.Should().Be(mensagem);
    }

    [Fact(DisplayName = "LogWarning deve capturar mensagem")]
    public void LogWarning_DeveCapturarMensagem()
    {
        // Arrange
        var logger = new LambdaLoggerSpy();
        const string mensagem = "Mensagem de warning";

        // Act
        logger.LogWarning(mensagem);

        // Assert
        logger.WarningLogs.Should().ContainSingle().Which.Should().Be(mensagem);
    }

    [Fact(DisplayName = "LogError deve capturar mensagem")]
    public void LogError_DeveCapturarMensagem()
    {
        // Arrange
        var logger = new LambdaLoggerSpy();
        const string mensagem = "Mensagem de erro";

        // Act
        logger.LogError(mensagem);

        // Assert
        logger.ErrorLogs.Should().ContainSingle().Which.Should().Be(mensagem);
    }

    [Fact(DisplayName = "Deve capturar múltiplas mensagens de cada tipo")]
    public void DeveCapturarMultiplasMensagensDeCadaTipo()
    {
        // Arrange
        var logger = new LambdaLoggerSpy();

        // Act
        logger.LogInformation("Info 1");
        logger.LogInformation("Info 2");
        logger.LogWarning("Warning 1");
        logger.LogError("Error 1");
        logger.LogError("Error 2");
        logger.LogError("Error 3");

        // Assert
        logger.InformationLogs.Should().HaveCount(2);
        logger.WarningLogs.Should().HaveCount(1);
        logger.ErrorLogs.Should().HaveCount(3);
    }
}
