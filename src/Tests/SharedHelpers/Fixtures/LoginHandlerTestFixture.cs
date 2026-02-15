using AuthLambda;
using Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Tests.SharedHelpers.Fixtures;

public class LoginHandlerTestFixture
{
    public Mock<IAuthenticationService> AuthenticationServiceMock { get; }
    public Mock<IConfiguration> ConfigurationMock { get; }
    public LoginHandler LoginHandler { get; }

    public LoginHandlerTestFixture()
    {
        AuthenticationServiceMock = new Mock<IAuthenticationService>();
        ConfigurationMock = new Mock<IConfiguration>();

        LoginHandler = new LoginHandler(AuthenticationServiceMock.Object, ConfigurationMock.Object);
    }
}
