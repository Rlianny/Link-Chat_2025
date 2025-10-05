using System.Security.Cryptography;
using System.Text;
namespace LinkChat.Core.Tools;
public static class AesEncryptor
{
    // WARNING: For demonstration only. In production, keys should be managed securely.
    // 16 bytes Key (AES-128)
    private static readonly byte[] SHARED_SECRET_KEY =
    [
        0x49, 0x77, 0x9E, 0x89, 0x2D, 0x5F, 0x23, 0x74, 0x6D, 0x47, 0x45, 0xE6, 0x78, 0xB2, 0xCA, 0x9C
    ];

    // Initialization vector with 16 bytes
    // IMPORTANT: For maximum security, the IV value must be unique for each message
    // To use an static one here es a design simplification
    private static readonly byte[] IV_STATIC =
    [
        0xCD, 0x84, 0xEF, 0xB2, 0xD7, 0xF3, 0x65, 0xC8, 0x08, 0x16, 0xC7, 0xF1, 0x38, 0x7D, 0x17, 0x32
    ];


    public static byte[] Encrypt(byte[] data)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = SHARED_SECRET_KEY;
            aesAlg.IV = IV_STATIC;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(data, 0, data.Length);
                }
                return msEncrypt.ToArray();
            }
        }
    }

    public static byte[] Decrypt(byte[] data)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = SHARED_SECRET_KEY;
            aesAlg.IV = IV_STATIC;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(data))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (MemoryStream msOutput = new MemoryStream())
                    {
                        csDecrypt.CopyTo(msOutput);
                        return msOutput.ToArray();
                    }
                }
            }
        }
    }
}