using Infrastructure.Database.Models;

namespace Infrastructure.Gateways.Interfaces
{
    public interface IUsuarioGateway
    {
        Task<UsuarioModel?> ObterUsuarioAtivoAsync(string documentoIdentificador);
    }
}