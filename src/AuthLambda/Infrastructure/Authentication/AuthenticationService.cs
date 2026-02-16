using Microsoft.Extensions.Configuration;
using Shared.Exceptions;
using Shared.Enums;
using Infrastructure.Gateways.Interfaces;
using Infrastructure.Authentication.PasswordHashing;

namespace Infrastructure.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly IUsuarioGateway _usuarioGateway;
        private readonly IPasswordHasher _passwordHasher;

        public AuthenticationService(IConfiguration configuration, ITokenService tokenService, IUsuarioGateway usuarioGateway, IPasswordHasher passwordHasher)
        {
            _configuration = configuration;
            _tokenService = tokenService;
            _usuarioGateway = usuarioGateway;
            _passwordHasher = passwordHasher;
        }

        public async Task<TokenResponseDto> ValidateCredentialsAndGenerateTokenAsync(TokenRequestDto request)
        {
            if (string.IsNullOrEmpty(request.DocumentoIdentificadorUsuario) || string.IsNullOrEmpty(request.Senha))
                throw new DomainException("Documento identificador e senha são obrigatórios.", ErrorType.InvalidInput);

            // Primeira validação: verificar se é um CPF ou CNPJ válido
            if (!ValidarDocumentoIdentificador(request.DocumentoIdentificadorUsuario))
                throw new DomainException("Documento identificador inválido.", ErrorType.Unauthorized);

            // Segunda validação: verificar se usuário existe e está ativo
            var usuario = await _usuarioGateway.ObterUsuarioAtivoAsync(request.DocumentoIdentificadorUsuario);
            if (usuario == null)
                throw new DomainException("Usuário não encontrado ou inativo.", ErrorType.Unauthorized);

            // Terceira validação: verificar se a senha está correta
            if (!_passwordHasher.Verify(request.Senha, usuario.SenhaHash))
                throw new DomainException("Senha incorreta.", ErrorType.Unauthorized);

            // Gerar token com ID do usuário, ClienteId (se existir) e roles
            var token = _tokenService.GenerateToken(usuario.Id.ToString(), usuario.ClienteId, usuario.Roles);
            return new TokenResponseDto(token);
        }

        private static bool ValidarDocumentoIdentificador(string documento)
        {
            var documentoLimpo = LimparDocumento(documento);
            
            return documentoLimpo.Length switch
            {
                11 => ValidarCpf(documentoLimpo),
                14 => ValidarCnpj(documentoLimpo),
                _ => false
            };
        }

        private static string LimparDocumento(string documento)
        {
            return string.Join("", documento.Where(char.IsDigit));
        }

        private static bool ValidarCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            if (cpf.Length != 11)
                return false;

            if (cpf.All(c => c == cpf[0]))
                return false;

            int[] multiplier1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplier2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf[..9];
            int sum = 0;

            for (int i = 0; i < 9; i++)
                sum += int.Parse(tempCpf[i].ToString()) * multiplier1[i];

            int remainder = sum % 11;
            remainder = remainder < 2 ? 0 : 11 - remainder;

            string digit = remainder.ToString();
            tempCpf += digit;

            sum = 0;
            for (int i = 0; i < 10; i++)
                sum += int.Parse(tempCpf[i].ToString()) * multiplier2[i];

            remainder = sum % 11;
            remainder = remainder < 2 ? 0 : 11 - remainder;

            digit += remainder.ToString();

            return cpf.EndsWith(digit);
        }

        private static bool ValidarCnpj(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return false;

            if (cnpj.Length != 14)
                return false;

            if (cnpj.All(c => c == cnpj[0]))
                return false;

            // Validação do primeiro dígito verificador
            int[] multiplier1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCnpj = cnpj[..12];
            int sum = 0;

            for (int i = 0; i < 12; i++)
                sum += int.Parse(tempCnpj[i].ToString()) * multiplier1[i];

            int remainder = sum % 11;
            remainder = remainder < 2 ? 0 : 11 - remainder;

            string digit = remainder.ToString();
            tempCnpj += digit;

            // Validação do segundo dígito verificador
            int[] multiplier2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            sum = 0;

            for (int i = 0; i < 13; i++)
                sum += int.Parse(tempCnpj[i].ToString()) * multiplier2[i];

            remainder = sum % 11;
            remainder = remainder < 2 ? 0 : 11 - remainder;

            digit += remainder.ToString();

            return cnpj.EndsWith(digit);
        }
    }
}
