using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Infrastructure.Authentication;
using Infrastructure.Authentication.PasswordHashing;
using Infrastructure.Gateways.Interfaces;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Enums;
using Shared.Exceptions;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AuthLambda;

public class LoginHandler
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IConfiguration _configuration;

    public LoginHandler()
    {
        // Construir configuração
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        // Configurar injeção de dependência
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        _authenticationService = serviceProvider.GetRequiredService<IAuthenticationService>();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(_configuration);
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IUsuarioGateway, UsuarioRepository>();
        
        // Configurar Argon2HashingOptions a partir do appsettings
        services.Configure<Argon2HashingOptions>(_configuration.GetSection("Argon2HashingOptions"));
        services.AddSingleton<IPasswordHasher>(provider =>
        {
            var argon2Options = provider.GetRequiredService<IOptions<Argon2HashingOptions>>().Value;
            return new PasswordHasher(argon2Options);
        });
        
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation($"Requisição de login recebida. Body: {request?.Body}");

        try
        {
            if (request?.Body == null)
            {
                return CreateErrorResponse(400, "Request body é obrigatório");
            }

            // Deserializar o body JSON
            var tokenRequest = JsonSerializer.Deserialize<TokenRequestDto>(request.Body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            context.Logger.LogInformation($"Documento recebido: {tokenRequest?.DocumentoIdentificadorUsuario}");

            if (tokenRequest == null)
            {
                return CreateErrorResponse(400, "Request inválido");
            }

            // Validar credenciais e gerar token
            var tokenResponse = await _authenticationService.ValidateCredentialsAndGenerateTokenAsync(tokenRequest);

            context.Logger.LogInformation("Token gerado com sucesso");
            
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Headers = new Dictionary<string, string>
                {
                    ["Content-Type"] = "application/json"
                },
                Body = JsonSerializer.Serialize(tokenResponse)
            };
        }
        catch (DomainException ex)
        {
            context.Logger.LogError($"Domain exception: {ex.Message} - Tipo: {ex.ErrorType}");
            
            // Mapear ErrorType para status code correto
            int statusCode = ex.ErrorType switch
            {
                ErrorType.InvalidInput => 400,      // Bad Request - dados ausentes/inválidos
                ErrorType.Unauthorized => 401,     // Unauthorized - credenciais incorretas
                _ => 400
            };
            
            return CreateErrorResponse(statusCode, ex.Message);
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Exception inesperada: {ex.Message}");
            return CreateErrorResponse(500, "Erro interno no servidor");
        }
    }

    private static APIGatewayProxyResponse CreateErrorResponse(int statusCode, string message)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = statusCode,
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json"
            },
            Body = JsonSerializer.Serialize(new { error = message })
        };
    }
}