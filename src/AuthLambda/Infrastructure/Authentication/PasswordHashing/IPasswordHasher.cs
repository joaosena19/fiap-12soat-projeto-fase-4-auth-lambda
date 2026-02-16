namespace Infrastructure.Authentication.PasswordHashing
{
    public interface IPasswordHasher
    {
        bool Verify(string password, string hash);
    }
}