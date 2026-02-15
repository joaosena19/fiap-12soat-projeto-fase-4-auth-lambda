using Infrastructure.Database;
using Infrastructure.Database.Models;
using Moq;
using System.Data.Common;

namespace Tests.SharedHelpers.Mocks;

public static class DapperExecutorMockExtensions
{
    public static UsuarioQuerySetup AoConsultarUsuario(this Mock<IDapperExecutor> mock)
    {
        return new UsuarioQuerySetup(mock);
    }

    public static RolesQuerySetup AoConsultarRoles(this Mock<IDapperExecutor> mock)
    {
        return new RolesQuerySetup(mock);
    }

    public static void NaoDeveTerConsultadoRoles(this Mock<IDapperExecutor> mock)
    {
        mock.Verify(
            x => x.QueryAsync<int>(
                It.IsAny<DbConnection>(),
                It.IsAny<string>(),
                It.IsAny<object>()),
            Times.Never);
    }

    public class UsuarioQuerySetup
    {
        private readonly Mock<IDapperExecutor> _mock;

        public UsuarioQuerySetup(Mock<IDapperExecutor> mock)
        {
            _mock = mock;
        }

        public UsuarioQuerySetup Retorna(UsuarioModel? usuario)
        {
            _mock.Setup(x => x.QuerySingleOrDefaultAsync<UsuarioModel>(
                    It.IsAny<DbConnection>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(usuario);
            return this;
        }

        public UsuarioQuerySetup NaoRetornaNada()
        {
            _mock.Setup(x => x.QuerySingleOrDefaultAsync<UsuarioModel>(
                    It.IsAny<DbConnection>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync((UsuarioModel?)null);
            return this;
        }

        public UsuarioQuerySetup RetornaComCallback(UsuarioModel usuario, Action<DbConnection, string, object?> callback)
        {
            _mock.Setup(x => x.QuerySingleOrDefaultAsync<UsuarioModel>(
                    It.IsAny<DbConnection>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(usuario)
                .Callback(callback);
            return this;
        }
    }

    public class RolesQuerySetup
    {
        private readonly Mock<IDapperExecutor> _mock;

        public RolesQuerySetup(Mock<IDapperExecutor> mock)
        {
            _mock = mock;
        }

        public RolesQuerySetup Retorna(List<int> roleIds)
        {
            _mock.Setup(x => x.QueryAsync<int>(
                    It.IsAny<DbConnection>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(roleIds);
            return this;
        }

        public RolesQuerySetup RetornaComCallback(List<int> roleIds, Action<DbConnection, string, object?> callback)
        {
            _mock.Setup(x => x.QueryAsync<int>(
                    It.IsAny<DbConnection>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(roleIds)
                .Callback(callback);
            return this;
        }
    }
}
