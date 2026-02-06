namespace Infrastructure.Authentication
{
    public interface ITokenService
    {
        string GenerateToken(string userId, Guid? clienteId, List<string> roles);
    }
}
