using Infrastructure.Authentication;
using Moq;

namespace Tests.SharedHelpers.Mocks;

public static class TokenServiceMockExtensions
{
    public static TokenServiceBuilder AoGerarToken(this Mock<ITokenService> mock)
    {
        return new TokenServiceBuilder(mock);
    }

    public class TokenServiceBuilder
    {
        private readonly Mock<ITokenService> _mock;

        public TokenServiceBuilder(Mock<ITokenService> mock)
        {
            _mock = mock;
        }

        public TokenServiceBuilder Retorna(string token)
        {
            _mock.Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<List<string>>()))
                .Returns(token);
            return this;
        }

        public TokenServiceBuilder ComCallback(Action<string, Guid?, List<string>> callback)
        {
            _mock.Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<List<string>>()))
                .Returns("mock-token")
                .Callback(callback);
            return this;
        }
    }
}
