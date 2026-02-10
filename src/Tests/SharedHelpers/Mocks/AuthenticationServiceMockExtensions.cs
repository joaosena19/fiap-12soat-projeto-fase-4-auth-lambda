using Infrastructure.Authentication;
using Moq;
using Shared.Enums;
using Shared.Exceptions;

namespace Tests.SharedHelpers.Mocks;

public static class AuthenticationServiceMockExtensions
{
    public static AuthenticationServiceBuilder AoValidarCredenciais(this Mock<IAuthenticationService> mock)
    {
        return new AuthenticationServiceBuilder(mock);
    }

    public class AuthenticationServiceBuilder
    {
        private readonly Mock<IAuthenticationService> _mock;

        public AuthenticationServiceBuilder(Mock<IAuthenticationService> mock)
        {
            _mock = mock;
        }

        public RequestSetup ComRequest(TokenRequestDto request)
        {
            return new RequestSetup(_mock, request);
        }
    }

    public class RequestSetup
    {
        private readonly Mock<IAuthenticationService> _mock;
        private readonly TokenRequestDto _request;

        public RequestSetup(Mock<IAuthenticationService> mock, TokenRequestDto request)
        {
            _mock = mock;
            _request = request;
        }

        public RequestSetup Retorna(TokenResponseDto response)
        {
            _mock.Setup(x => x.ValidateCredentialsAndGenerateTokenAsync(_request))
                .ReturnsAsync(response);
            return this;
        }

        public RequestSetup LancaDomainException(ErrorType errorType, string mensagem = "Erro de domínio")
        {
            _mock.Setup(x => x.ValidateCredentialsAndGenerateTokenAsync(_request))
                .ThrowsAsync(new DomainException(mensagem, errorType));
            return this;
        }

        public RequestSetup LancaExcecao(Exception exception)
        {
            _mock.Setup(x => x.ValidateCredentialsAndGenerateTokenAsync(_request))
                .ThrowsAsync(exception);
            return this;
        }
    }
}
