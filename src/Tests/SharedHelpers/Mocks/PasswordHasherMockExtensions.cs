using Infrastructure.Authentication.PasswordHashing;
using Moq;

namespace Tests.SharedHelpers.Mocks;

public static class PasswordHasherMockExtensions
{
    public static PasswordHasherBuilder AoVerificarSenha(this Mock<IPasswordHasher> mock)
    {
        return new PasswordHasherBuilder(mock);
    }

    public class PasswordHasherBuilder
    {
        private readonly Mock<IPasswordHasher> _mock;

        public PasswordHasherBuilder(Mock<IPasswordHasher> mock)
        {
            _mock = mock;
        }

        public PasswordHasherBuilder ComSenhaEHash(string senha, string hash, bool resultado)
        {
            _mock.Setup(x => x.Verify(senha, hash))
                .Returns(resultado);
            return this;
        }
    }
}
