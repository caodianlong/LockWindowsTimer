namespace WinLockTimer;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// AES加密解密工具类
/// </summary>
public static class AesEncryption
{
    // 固定的密钥和IV（在实际应用中应该更安全地管理这些值）
    // AES-128需要16字节密钥，AES-192需要24字节，AES-256需要32字节
    private static readonly byte[] Key = Encoding.ASCII.GetBytes("WinLockTimer2024"); // 16字节密钥 (AES-128)
    private static readonly byte[] IV = Encoding.ASCII.GetBytes("1234567890123456");   // 16字节IV

    /// <summary>
    /// 加密字符串
    /// </summary>
    /// <param name="plainText">要加密的明文</param>
    /// <returns>Base64编码的加密字符串</returns>
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        try
        {
            using (Aes aesAlg = Aes.Create())
            {
                // 使用固定的密钥和IV
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                // 创建加密器
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // 创建内存流
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            // 写入要加密的数据
                            swEncrypt.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"加密失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 解密字符串
    /// </summary>
    /// <param name="cipherText">Base64编码的加密字符串</param>
    /// <returns>解密后的明文</returns>
    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        try
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                // 创建解密器
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // 创建内存流
                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // 读取解密的数据
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"解密失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 加密数据并保存到文件
    /// </summary>
    /// <param name="data">要保存的数据</param>
    /// <param name="filePath">文件路径</param>
    public static void SaveEncryptedData(string data, string filePath)
    {
        try
        {
            string encryptedData = Encrypt(data);
            File.WriteAllText(filePath, encryptedData);
        }
        catch (Exception ex)
        {
            throw new Exception($"保存加密数据失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 从文件读取并解密数据
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>解密后的数据</returns>
    public static string LoadEncryptedData(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return string.Empty;

            string encryptedData = File.ReadAllText(filePath);
            return Decrypt(encryptedData);
        }
        catch (Exception ex)
        {
            throw new Exception($"读取加密数据失败: {ex.Message}", ex);
        }
    }
}