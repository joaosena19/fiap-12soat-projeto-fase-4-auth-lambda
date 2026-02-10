using Bogus;
using Bogus.Extensions.Brazil;
using Infrastructure.Database.Models;

namespace Tests.SharedHelpers.Builders;

public class UsuarioModelBuilder
{
    private Guid _id;
    private string _documentoIdentificador;
    private string _senhaHash;
    private string _status;
    private Guid? _clienteId;
    private List<string> _roles;

    public UsuarioModelBuilder()
    {
        var faker = new Faker("pt_BR");
        _id = Guid.NewGuid();
        _documentoIdentificador = faker.Person.Cpf(false); // CPF sem máscara
        _senhaHash = Convert.ToBase64String(new byte[48]); // Simula hash+salt válido
        _status = "ativo";
        _clienteId = null;
        _roles = new List<string> { "Cliente" };
    }

    public UsuarioModelBuilder ComId(Guid id)
    {
        _id = id;
        return this;
    }

    public UsuarioModelBuilder ComDocumento(string documento)
    {
        _documentoIdentificador = documento;
        return this;
    }

    public UsuarioModelBuilder ComSenhaHash(string senhaHash)
    {
        _senhaHash = senhaHash;
        return this;
    }

    public UsuarioModelBuilder ComStatus(string status)
    {
        _status = status;
        return this;
    }

    public UsuarioModelBuilder ComClienteId(Guid? clienteId)
    {
        _clienteId = clienteId;
        return this;
    }

    public UsuarioModelBuilder ComRoles(List<string> roles)
    {
        _roles = roles;
        return this;
    }

    public UsuarioModel Build()
    {
        return new UsuarioModel
        {
            Id = _id,
            DocumentoIdentificador = _documentoIdentificador,
            SenhaHash = _senhaHash,
            Status = _status,
            ClienteId = _clienteId,
            Roles = _roles
        };
    }
}
