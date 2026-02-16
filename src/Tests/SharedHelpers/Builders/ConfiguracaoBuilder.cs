using Microsoft.Extensions.Configuration;

namespace Tests.SharedHelpers.Builders;

public static class ConfiguracaoBuilder
{
    public static IConfiguration CriarConfiguracao()
    {
        var valores = new Dictionary<string, string?>
        {
            { "Jwt:Key", "xmvsLe9QxIR3BWAWJW4wL+5ZfZrYaohxUaRYSkxteiAn5qEAKDd3xCMn1Bk46ndy6sl4gkVXXvEP/1JowbBp/g==" },
            { "Jwt:Issuer", "OficinaMecanicaApi" },
            { "Jwt:Audience", "AuthorizedServices" },
            { "Argon2HashingOptions:SaltSize", "16" },
            { "Argon2HashingOptions:HashSize", "32" },
            { "Argon2HashingOptions:Iterations", "4" },
            { "Argon2HashingOptions:MemorySize", "65536" },
            { "Argon2HashingOptions:DegreeOfParallelism", "1" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(valores)
            .Build();
    }

    public static IConfiguration ComJwtKey(this IConfiguration configuration, string jwtKey)
    {
        configuration["Jwt:Key"] = jwtKey;
        return configuration;
    }

    public static IConfiguration ComJwtIssuer(this IConfiguration configuration, string issuer)
    {
        configuration["Jwt:Issuer"] = issuer;
        return configuration;
    }

    public static IConfiguration ComJwtAudience(this IConfiguration configuration, string audience)
    {
        configuration["Jwt:Audience"] = audience;
        return configuration;
    }
}
