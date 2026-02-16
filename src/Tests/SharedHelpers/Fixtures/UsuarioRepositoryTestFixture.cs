using Infrastructure.Database;
using Infrastructure.Repositories;
using Moq;
using System.Data.Common;
using Tests.SharedHelpers.Mocks;

namespace Tests.SharedHelpers.Fixtures;

public class UsuarioRepositoryTestFixture
{
    public Mock<IDbConnectionFactory> ConnectionFactoryMock { get; }
    public Mock<IDapperExecutor> DapperExecutorMock { get; }
    public Mock<DbConnection> ConnectionMock { get; }
    public UsuarioRepository UsuarioRepository { get; }

    public UsuarioRepositoryTestFixture()
    {
        ConnectionFactoryMock = new Mock<IDbConnectionFactory>();
        DapperExecutorMock = new Mock<IDapperExecutor>();
        ConnectionMock = new Mock<DbConnection>();

        ConnectionFactoryMock.AoCriarConexao(ConnectionMock.Object);

        UsuarioRepository = new UsuarioRepository(ConnectionFactoryMock.Object, DapperExecutorMock.Object);
    }
}
