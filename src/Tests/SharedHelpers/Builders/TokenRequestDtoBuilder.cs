using Bogus;
using Bogus.Extensions.Brazil;
using Infrastructure.Authentication;

namespace Tests.SharedHelpers.Builders;

public class TokenRequestDtoBuilder
{
    private string _documentoIdentificador;
    private string _senha;

    public TokenRequestDtoBuilder()
    {
        var faker = new Faker("pt_BR");
        _documentoIdentificador = faker.Person.Cpf(false); // CPF sem máscara
        _senha = "Senha@123";
    }

    public TokenRequestDtoBuilder ComDocumento(string documento)
    {
        _documentoIdentificador = documento;
        return this;
    }

    public TokenRequestDtoBuilder ComSenha(string senha)
    {
        _senha = senha;
        return this;
    }

    public TokenRequestDto Build()
    {
        return new TokenRequestDto(_documentoIdentificador, _senha);
    }
}
