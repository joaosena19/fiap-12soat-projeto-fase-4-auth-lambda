namespace Infrastructure.Authentication
{
    public interface IAuthenticationService
    {
        Task<TokenResponseDto> ValidateCredentialsAndGenerateTokenAsync(TokenRequestDto request);
    }
}
