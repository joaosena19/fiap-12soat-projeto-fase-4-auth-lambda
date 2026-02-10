using FluentAssertions;
using Tests.SharedHelpers.Builders;

namespace Tests.Unit.MetaTests.SharedHelpers;

public class ConfiguracaoBuilderTest
{
    [Fact(DisplayName = "CriarConfiguracao deve retornar IConfiguration não nulo")]
    public void CriarConfiguracao_DeveRetornarIConfigurationNaoNulo()
    {
        // Act
        var configuration = ConfiguracaoBuilder.CriarConfiguracao();

        // Assert
        configuration.Should().NotBeNull();
    }

    [Fact(DisplayName = "CriarConfiguracao deve conter configurações JWT padrão")]
    public void CriarConfiguracao_DeveConterConfiguracoesJwtPadrao()
    {
        // Act
        var configuration = ConfiguracaoBuilder.CriarConfiguracao();

        // Assert
        configuration["Jwt:Key"].Should().NotBeNullOrWhiteSpace();
        configuration["Jwt:Issuer"].Should().Be("OficinaMecanicaApi");
        configuration["Jwt:Audience"].Should().Be("AuthorizedServices");
    }

    [Fact(DisplayName = "CriarConfiguracao deve conter configurações Argon2 padrão")]
    public void CriarConfiguracao_DeveConterConfiguracoesArgon2Padrao()
    {
        // Act
        var configuration = ConfiguracaoBuilder.CriarConfiguracao();

        // Assert
        configuration["Argon2HashingOptions:SaltSize"].Should().Be("16");
        configuration["Argon2HashingOptions:HashSize"].Should().Be("32");
        configuration["Argon2HashingOptions:Iterations"].Should().Be("4");
        configuration["Argon2HashingOptions:MemorySize"].Should().Be("65536");
        configuration["Argon2HashingOptions:DegreeOfParallelism"].Should().Be("1");
    }

    [Fact(DisplayName = "ComJwtKey deve sobrescrever JwtKey")]
    public void ComJwtKey_DeveSobrescreverJwtKey()
    {
        // Arrange
        const string jwtKeyEsperada = "nova_chave_jwt_256_bits_minimo_aqui";

        // Act
        var configuration = ConfiguracaoBuilder.CriarConfiguracao()
            .ComJwtKey(jwtKeyEsperada);

        // Assert
        configuration["Jwt:Key"].Should().Be(jwtKeyEsperada);
    }

    [Fact(DisplayName = "ComJwtIssuer deve sobrescrever JwtIssuer")]
    public void ComJwtIssuer_DeveSobrescreverJwtIssuer()
    {
        // Arrange
        const string issuerEsperado = "NovoIssuer";

        // Act
        var configuration = ConfiguracaoBuilder.CriarConfiguracao()
            .ComJwtIssuer(issuerEsperado);

        // Assert
        configuration["Jwt:Issuer"].Should().Be(issuerEsperado);
    }

    [Fact(DisplayName = "ComJwtAudience deve sobrescrever JwtAudience")]
    public void ComJwtAudience_DeveSobrescreverJwtAudience()
    {
        // Arrange
        const string audienceEsperada = "NovoAudience";

        // Act
        var configuration = ConfiguracaoBuilder.CriarConfiguracao()
            .ComJwtAudience(audienceEsperada);

        // Assert
        configuration["Jwt:Audience"].Should().Be(audienceEsperada);
    }
}
