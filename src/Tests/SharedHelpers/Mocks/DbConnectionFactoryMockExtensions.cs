using Infrastructure.Database;
using Moq;
using System.Data.Common;

namespace Tests.SharedHelpers.Mocks;

public static class DbConnectionFactoryMockExtensions
{
    public static void AoCriarConexao(this Mock<IDbConnectionFactory> mock, DbConnection conexao)
    {
        mock.Setup(x => x.CreateOpenConnectionAsync())
            .ReturnsAsync(conexao);
    }
}
