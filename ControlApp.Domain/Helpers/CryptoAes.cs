using System;
using System.Security.Cryptography;
using System.Text;

public class CryptoAes
{
    private readonly byte[] Key;
    private readonly byte[] IV;

    #region Construtor
    public CryptoAes()
    {
        Key = GenerateKey(32); // Gera uma chave de 32 bytes para criptografia
        IV = GenerateIV(16);   // Gera um vetor de inicialização de 16 bytes pra reforçar a segurança
    }
    #endregion

    #region Métodos de Geração
    private static byte[] GenerateKey(int size)
    {
        byte[] key = new byte[size]; // Reserva espaço pra chave
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key); // Preenche o espaço com bytes aleatórios e seguros
        }
        return key; // Retorna a chave pronta
    }

    private static byte[] GenerateIV(int size)
    {
        byte[] iv = new byte[size]; // Reserva espaço pro vetor de inicialização
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(iv); // Preenche com bytes aleatórios pra garantir segurança
        }
        return iv; // Retorna o IV pronto
    }
    #endregion

    #region Métodos de Criptografia
    public string Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key; // Define a chave no AES
            aes.IV = IV;   // Define o vetor de inicialização no AES

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV); // Prepara o objeto que vai criptografar
            byte[] encrypted;

            using (var ms = new System.IO.MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(plainText); // Converte o texto em bytes
                    cs.Write(inputBytes, 0, inputBytes.Length); // Passa os bytes pra criptografia
                }
                encrypted = ms.ToArray(); // Pega o resultado criptografado
            }

            return Convert.ToBase64String(encrypted); // Transforma em string legível e retorna
        }
    }

    public string Decrypt(string cipherText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key; // Usa a mesma chave pra descriptografar
            aes.IV = IV;   // Usa o mesmo IV pra tudo funcionar direitinho

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV); // Prepara o objeto que vai descriptografar
            byte[] cipherBytes = Convert.FromBase64String(cipherText); // Converte a string criptografada em bytes

            using (var ms = new System.IO.MemoryStream())
            {
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length); // Descriptografa os bytes
                }
                byte[] decrypted = ms.ToArray(); // Pega o resultado em bytes
                return Encoding.UTF8.GetString(decrypted); // Converte pra texto e retorna
            }
        }
    }
    #endregion

    #region Métodos de Acesso
    public byte[] GetKey() => Key; // Devolve a chave quando chamada
    public byte[] GetIV() => IV;   // Devolve o vetor de inicialização quando chamado
    #endregion
}