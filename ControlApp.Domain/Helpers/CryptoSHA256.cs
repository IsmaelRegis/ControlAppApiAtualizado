using System;
using System.Security.Cryptography;
using System.Text;

public class CryptoSHA256
{
    #region Métodos de Hash
    public string HashPassword(string senha)
    {
        if (string.IsNullOrEmpty(senha))
        {
            throw new ArgumentException("A senha não pode ser nula ou vazia."); // Lança erro se a senha estiver vazia
        }

        byte[] salt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt); // Cria um salt aleatório pra deixar o hash mais seguro
        }

        byte[] bytes = Encoding.UTF8.GetBytes(senha + Convert.ToBase64String(salt)); // Junta a senha com o salt em bytes

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashedBytes = sha256.ComputeHash(bytes); // Calcula o hash da senha com o salt

            StringBuilder builder = new StringBuilder();
            foreach (byte b in hashedBytes)
            {
                builder.Append(b.ToString("x2")); // Monta o hash em formato hexadecimal
            }

            // Junta o hash com o salt (separado por ":") e retorna
            return builder.ToString() + ":" + Convert.ToBase64String(salt);
        }
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
        {
            return false; // Retorna falso se a senha ou o hash estiverem vazios
        }

        // Separa o hash e o salt usando ":" como divisor
        string[] partes = hash.Split(':');
        if (partes.Length != 2)
        {
            return false; // Retorna falso se o formato estiver errado
        }

        string hashHexArmazenado = partes[0];   // Pega o hash guardado
        string saltBase64Armazenado = partes[1]; // Pega o salt guardado em base64

        // Transforma o salt de Base64 pra bytes
        byte[] saltArmazenado = Convert.FromBase64String(saltBase64Armazenado);

        // Junta a senha fornecida com o salt e calcula o hash
        byte[] senhaComSalte = Encoding.UTF8.GetBytes(password + saltBase64Armazenado);
        byte[] hashBytesCalculado = SHA256.HashData(senhaComSalte);
        string hashHexCalculado = Convert.ToHexString(hashBytesCalculado).ToLower();

        // Compara o hash calculado com o hash guardado pra ver se batem
        return string.Equals(hashHexCalculado, hashHexArmazenado, StringComparison.OrdinalIgnoreCase);
    }
    #endregion
}