using Infrastructure.Authentication;
using Infrastructure.Authentication.PasswordHashing;
using Infrastructure.Gateways.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Tests.SharedHelpers.Fixtures;

public class AuthenticationServiceTestFixture
{
    public Mock<ITokenService> TokenServiceMock { get; }
    public Mock<IUsuarioGateway> UsuarioGatewayMock { get; }
    public Mock<IPasswordHasher> PasswordHasherMock { get; }
    public AuthenticationService AuthenticationService { get; }

    public AuthenticationServiceTestFixture()
    {
        TokenServiceMock = new Mock<ITokenService>();
        UsuarioGatewayMock = new Mock<IUsuarioGateway>();
        PasswordHasherMock = new Mock<IPasswordHasher>();

        AuthenticationService = new AuthenticationService(TokenServiceMock.Object, UsuarioGatewayMock.Object, PasswordHasherMock.Object);
    }
}
