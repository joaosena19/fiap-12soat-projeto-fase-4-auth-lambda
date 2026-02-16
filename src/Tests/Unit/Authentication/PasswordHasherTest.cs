using FluentAssertions;
using Infrastructure.Authentication.PasswordHashing;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace Tests.Unit.Authentication;

public class PasswordHasherTest
{
    [Fact(DisplayName = "Deve retornar false quando password for vazio")]
    [Trait("Service", "PasswordHasher")]
    public void Verify_DeveRetornarFalse_QuandoPasswordForVazio()
    {
        // Arrange
        var options = new Argon2HashingOptions
        {
            SaltSize = 16,
            HashSize = 32,
            Iterations = 4,
            MemorySize = 65536,
            DegreeOfParallelism = 1
        };
        var passwordHasher = new PasswordHasher(options);
        var hashQualquer = "dGVzdGVoYXNoYmFzZTY0dGVzdGVoYXNoYmFzZTY0dGVzdGVoYXNoYmFzZTY0";

        // Act
        var resultado = passwordHasher.Verify("", hashQualquer);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact(DisplayName = "Deve retornar false quando password for nulo")]
    [Trait("Service", "PasswordHasher")]
    public void Verify_DeveRetornarFalse_QuandoPasswordForNulo()
    {
        // Arrange
        var options = new Argon2HashingOptions
        {
            SaltSize = 16,
            HashSize = 32,
            Iterations = 4,
            MemorySize = 65536,
            DegreeOfParallelism = 1
        };
        var passwordHasher = new PasswordHasher(options);
        var hashQualquer = "dGVzdGVoYXNoYmFzZTY0dGVzdGVoYXNoYmFzZTY0dGVzdGVoYXNoYmFzZTY0";

        // Act
        var resultado = passwordHasher.Verify(null!, hashQualquer);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact(DisplayName = "Deve retornar false quando hash for vazio")]
    [Trait("Service", "PasswordHasher")]
    public void Verify_DeveRetornarFalse_QuandoHashForVazio()
    {
        // Arrange
        var options = new Argon2HashingOptions
        {
            SaltSize = 16,
            HashSize = 32,
            Iterations = 4,
            MemorySize = 65536,
            DegreeOfParallelism = 1
        };
        var passwordHasher = new PasswordHasher(options);
        var senhaQualquer = "SenhaQualquer@123";

        // Act
        var resultado = passwordHasher.Verify(senhaQualquer, "");

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact(DisplayName = "Deve retornar false quando hash for nulo")]
    [Trait("Service", "PasswordHasher")]
    public void Verify_DeveRetornarFalse_QuandoHashForNulo()
    {
        // Arrange
        var options = new Argon2HashingOptions
        {
            SaltSize = 16,
            HashSize = 32,
            Iterations = 4,
            MemorySize = 65536,
            DegreeOfParallelism = 1
        };
        var passwordHasher = new PasswordHasher(options);
        var senhaQualquer = "SenhaQualquer@123";

        // Act
        var resultado = passwordHasher.Verify(senhaQualquer, null!);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact(DisplayName = "Deve retornar false quando hash não for base64 válido")]
    [Trait("Service", "PasswordHasher")]
    public void Verify_DeveRetornarFalse_QuandoHashNaoForBase64Valido()
    {
        // Arrange
        var options = new Argon2HashingOptions
        {
            SaltSize = 16,
            HashSize = 32,
            Iterations = 4,
            MemorySize = 65536,
            DegreeOfParallelism = 1
        };
        var passwordHasher = new PasswordHasher(options);
        var senhaQualquer = "SenhaQualquer@123";
        var hashInvalido = "nao-base64!!!@#$%";

        // Act
        var resultado = passwordHasher.Verify(senhaQualquer, hashInvalido);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact(DisplayName = "Deve retornar false quando tamanho do hash for inválido")]
    [Trait("Service", "PasswordHasher")]
    public void Verify_DeveRetornarFalse_QuandoTamanhoDoHashForInvalido()
    {
        // Arrange
        var options = new Argon2HashingOptions
        {
            SaltSize = 16,
            HashSize = 32,
            Iterations = 4,
            MemorySize = 65536,
            DegreeOfParallelism = 1
        };
        var passwordHasher = new PasswordHasher(options);
        var senhaQualquer = "SenhaQualquer@123";
        
        // Gerar base64 com tamanho errado (esperado: 16 + 32 = 48 bytes)
        var bytesComTamanhoErrado = new byte[20]; // Tamanho diferente do esperado
        var hashComTamanhoErrado = Convert.ToBase64String(bytesComTamanhoErrado);

        // Act
        var resultado = passwordHasher.Verify(senhaQualquer, hashComTamanhoErrado);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact(DisplayName = "Deve retornar true quando password e hash forem válidos")]
    [Trait("Service", "PasswordHasher")]
    public void Verify_DeveRetornarTrue_QuandoPasswordEHashForemValidos()
    {
        // Arrange
        var options = new Argon2HashingOptions
        {
            SaltSize = 16,
            HashSize = 32,
            Iterations = 4,
            MemorySize = 65536,
            DegreeOfParallelism = 1
        };

        var password = "SenhaCorreta@123";
        var salt = new byte[options.SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Gerar hash com Argon2id
        byte[] hash;
        using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
        {
            argon2.Salt = salt;
            argon2.DegreeOfParallelism = options.DegreeOfParallelism;
            argon2.Iterations = options.Iterations;
            argon2.MemorySize = options.MemorySize;
            hash = argon2.GetBytes(options.HashSize);
        }

        // Combinar salt + hash e converter para base64
        var combinedBytes = new byte[salt.Length + hash.Length];
        Buffer.BlockCopy(salt, 0, combinedBytes, 0, salt.Length);
        Buffer.BlockCopy(hash, 0, combinedBytes, salt.Length, hash.Length);
        var hashBase64 = Convert.ToBase64String(combinedBytes);

        // Act
        var passwordHasher = new PasswordHasher(options);
        var resultado = passwordHasher.Verify(password, hashBase64);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact(DisplayName = "Deve retornar false quando password for incorreto")]
    [Trait("Service", "PasswordHasher")]
    public void Verify_DeveRetornarFalse_QuandoPasswordForIncorreto()
    {
        // Arrange
        var options = new Argon2HashingOptions
        {
            SaltSize = 16,
            HashSize = 32,
            Iterations = 4,
            MemorySize = 65536,
            DegreeOfParallelism = 1
        };

        var passwordCorreta = "SenhaCorreta@123";
        var passwordIncorreta = "SenhaErrada@456";
        var salt = new byte[options.SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Gerar hash com Argon2id usando a senha correta
        byte[] hash;
        using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(passwordCorreta)))
        {
            argon2.Salt = salt;
            argon2.DegreeOfParallelism = options.DegreeOfParallelism;
            argon2.Iterations = options.Iterations;
            argon2.MemorySize = options.MemorySize;
            hash = argon2.GetBytes(options.HashSize);
        }

        // Combinar salt + hash e converter para base64
        var combinedBytes = new byte[salt.Length + hash.Length];
        Buffer.BlockCopy(salt, 0, combinedBytes, 0, salt.Length);
        Buffer.BlockCopy(hash, 0, combinedBytes, salt.Length, hash.Length);
        var hashBase64 = Convert.ToBase64String(combinedBytes);

        // Act - testar com senha incorreta
        var passwordHasher = new PasswordHasher(options);
        var resultado = passwordHasher.Verify(passwordIncorreta, hashBase64);

        // Assert
        resultado.Should().BeFalse();
    }
}
