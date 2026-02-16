namespace Infrastructure.Database.Models
{
    public class UsuarioModel
    {
        public Guid Id { get; set; }
        public string DocumentoIdentificador { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid? ClienteId { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}