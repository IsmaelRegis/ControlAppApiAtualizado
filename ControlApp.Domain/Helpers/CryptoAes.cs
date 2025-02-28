using System;
using System.Security.Cryptography;
using System.Text;

public class CryptoAes
{
    private readonly byte[] Key; 
    private readonly byte[] IV;  

    public CryptoAes()
    {
        Key = GenerateKey(32);
        IV = GenerateIV(16);   
    }

    private static byte[] GenerateKey(int size)
    {
        byte[] key = new byte[size];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }
        return key;
    }

    private static byte[] GenerateIV(int size)
    {
        byte[] iv = new byte[size];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(iv);
        }
        return iv;
    }

    public string Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = IV;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] encrypted;

            using (var ms = new System.IO.MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                    cs.Write(inputBytes, 0, inputBytes.Length);
                }
                encrypted = ms.ToArray();
            }

            return Convert.ToBase64String(encrypted);
        }
    }

    public string Decrypt(string cipherText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = IV;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (var ms = new System.IO.MemoryStream())
            {
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                }
                byte[] decrypted = ms.ToArray();
                return Encoding.UTF8.GetString(decrypted);
            }
        }
    }

    public byte[] GetKey() => Key;
    public byte[] GetIV() => IV;
}