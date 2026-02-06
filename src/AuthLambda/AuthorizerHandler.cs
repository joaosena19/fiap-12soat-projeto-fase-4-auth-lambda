using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthLambda;

public class AuthorizerHandler
{
    private readonly IConfiguration _configuration;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public AuthorizerHandler()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public Dictionary<string, object> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation("Lambda Authorizer executado");

        try
        {
            // Para HTTP API v2, o token vem em request.Headers["authorization"] ou request.Headers["Authorization"]
            var authHeader = request.Headers?.ContainsKey("authorization") == true 
                ? request.Headers["authorization"] 
                : request.Headers?.ContainsKey("Authorization") == true 
                    ? request.Headers["Authorization"] 
                    : null;
            
            context.Logger.LogInformation($"Authorization header recebido: {(authHeader != null ? "[PRESENTE]" : "[AUSENTE]")}");
            
            // Debug: Log do header completo (mascarado)
            if (authHeader != null)
            {
                var maskedHeader = authHeader.Length > 20 
                    ? authHeader.Substring(0, 20) + "..." 
                    : authHeader;
                context.Logger.LogInformation($"Header value (primeiros 20 chars): {maskedHeader}");
            }
            
            var token = ExtractToken(authHeader);
            if (string.IsNullOrEmpty(token))
            {
                context.Logger.LogWarning("Token não fornecido ou inválido");
                return new Dictionary<string, object>
                {
                    ["isAuthorized"] = false
                };
            }

            // Debug: Log do token extraído (mascarado)
            var maskedToken = token.Length > 20 
                ? token.Substring(0, 20) + "..." 
                : token;
            context.Logger.LogInformation($"Token extraído (primeiros 20 chars): {maskedToken}");

            var claimsPrincipal = ValidateToken(token);
            var userId = claimsPrincipal.FindFirst("sub")?.Value ?? "user";
            var role = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value ?? "user";

            context.Logger.LogInformation($"Token válido para usuário: {userId}");

            return new Dictionary<string, object>
            {
                ["isAuthorized"] = true,
                ["context"] = new Dictionary<string, object>
                {
                    ["sub"] = userId,
                    ["role"] = role,
                    ["tokenValidated"] = "true"
                }
            };
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Erro na autorização: {ex.Message}");
            return new Dictionary<string, object>
            {
                ["isAuthorized"] = false
            };
        }
    }

    private string? ExtractToken(string? authorizationHeader)
    {
        if (string.IsNullOrEmpty(authorizationHeader))
            return null;

        // Remove todos os prefixos "Bearer " (pode ter mais de um devido a proxy/gateway)
        var token = authorizationHeader.Trim();
        while (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = token.Substring(7).Trim();
        }

        return string.IsNullOrEmpty(token) ? null : token;
    }

    private ClaimsPrincipal ValidateToken(string token)
    {
        var jwtKey = _configuration["Jwt:Key"];
        var jwtIssuer = _configuration["Jwt:Issuer"];
        var jwtAudience = _configuration["Jwt:Audience"];

        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
        {
            throw new InvalidOperationException("Configuração JWT incompleta");
        }

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var principal = _tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
        return principal;
    }
}