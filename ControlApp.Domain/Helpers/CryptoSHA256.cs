using System;
using System.Security.Cryptography;
using System.Text;

public class CryptoSHA256
{
    public string HashPassword(string senha)
    {
        if (string.IsNullOrEmpty(senha))
        {
            throw new ArgumentException("A senha não pode ser nula ou vazia.");
        }

        byte[] salt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt); // Gera um salt aleatório
        }

        byte[] bytes = Encoding.UTF8.GetBytes(senha + Convert.ToBase64String(salt)); // Concatenando senha + salt

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashedBytes = sha256.ComputeHash(bytes); // Gerando o hash da senha concatenada

            StringBuilder builder = new StringBuilder();
            foreach (byte b in hashedBytes)
            {
                builder.Append(b.ToString("x2"));
            }

            // Retorna o hash concatenado com o salt em base64 (usando ":" como delimitador)
            return builder.ToString() + ":" + Convert.ToBase64String(salt);
        }
    }
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
        {
            return false;
        }

        // Divida o hash e o salt usando ":" como separador
        string[] partes = hash.Split(':');
        if (partes.Length != 2)
        {
            return false; // Formato inválido
        }

        string hashHexArmazenado = partes[0];   // O hash armazenado
        string saltBase64Armazenado = partes[1]; // O salt armazenado em base64

        // Converte o salt armazenado de Base64 para byte[]
        byte[] saltArmazenado = Convert.FromBase64String(saltBase64Armazenado);

        // Gera o hash da senha fornecida concatenada com o salt
        byte[] senhaComSalte = Encoding.UTF8.GetBytes(password + saltBase64Armazenado);
        byte[] hashBytesCalculado = SHA256.HashData(senhaComSalte);
        string hashHexCalculado = Convert.ToHexString(hashBytesCalculado).ToLower();

        // Compara o hash calculado com o hash armazenado
        return string.Equals(hashHexCalculado, hashHexArmazenado, StringComparison.OrdinalIgnoreCase);
    }


}