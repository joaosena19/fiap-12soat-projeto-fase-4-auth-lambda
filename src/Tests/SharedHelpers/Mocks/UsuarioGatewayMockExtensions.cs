using Infrastructure.Database.Models;
using Infrastructure.Gateways.Interfaces;
using Moq;

namespace Tests.SharedHelpers.Mocks;

public static class UsuarioGatewayMockExtensions
{
    public static DocumentoSetup AoObterUsuarioAtivo(this Mock<IUsuarioGateway> mock)
    {
        return new DocumentoSetup(mock);
    }

    public class DocumentoSetup
    {
        private readonly Mock<IUsuarioGateway> _mock;

        public DocumentoSetup(Mock<IUsuarioGateway> mock)
        {
            _mock = mock;
        }

        public RetornaSetup ComDocumento(string documento)
        {
            return new RetornaSetup(_mock, documento);
        }
    }

    public class RetornaSetup
    {
        private readonly Mock<IUsuarioGateway> _mock;
        private readonly string _documento;

        public RetornaSetup(Mock<IUsuarioGateway> mock, string documento)
        {
            _mock = mock;
            _documento = documento;
        }

        public void Retorna(UsuarioModel usuario)
        {
            _mock.Setup(x => x.ObterUsuarioAtivoAsync(_documento))
                .ReturnsAsync(usuario);
        }

        public void NaoRetornaNada()
        {
            _mock.Setup(x => x.ObterUsuarioAtivoAsync(_documento))
                .ReturnsAsync((UsuarioModel?)null);
        }
    }
}
