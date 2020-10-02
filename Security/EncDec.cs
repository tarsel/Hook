using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Hook.Security
{
    public class EncDec
    {
        private static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
        {
            MemoryStream memoryStream = new MemoryStream();
            Rijndael rijndael = Rijndael.Create();
            rijndael.Key = Key;
            rijndael.IV = IV;
            CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(clearData, 0, clearData.Length);
            cryptoStream.Close();
            return memoryStream.ToArray();
        }

        public static string Encrypt(string clearText, string Password)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(clearText);
            PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(Password, new byte[13]
            {
        (byte) 73,
        (byte) 118,
        (byte) 97,
        (byte) 110,
        (byte) 32,
        (byte) 77,
        (byte) 101,
        (byte) 100,
        (byte) 118,
        (byte) 101,
        (byte) 100,
        (byte) 101,
        (byte) 118
            });
            return Convert.ToBase64String(Encrypt(bytes, passwordDeriveBytes.GetBytes(32), passwordDeriveBytes.GetBytes(16)));
        }

        private static byte[] Encrypt(byte[] clearData, string Password)
        {
            PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(Password, new byte[13]
            {
        (byte) 73,
        (byte) 118,
        (byte) 97,
        (byte) 110,
        (byte) 32,
        (byte) 77,
        (byte) 101,
        (byte) 100,
        (byte) 118,
        (byte) 101,
        (byte) 100,
        (byte) 101,
        (byte) 118
            });
            return Encrypt(clearData, passwordDeriveBytes.GetBytes(32), passwordDeriveBytes.GetBytes(16));
        }

        private static void Encrypt(string fileIn, string fileOut, string Password)
        {
            FileStream fileStream1 = new FileStream(fileIn, FileMode.Open, FileAccess.Read);
            FileStream fileStream2 = new FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write);
            PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(Password, new byte[13]
            {
        (byte) 73,
        (byte) 118,
        (byte) 97,
        (byte) 110,
        (byte) 32,
        (byte) 77,
        (byte) 101,
        (byte) 100,
        (byte) 118,
        (byte) 101,
        (byte) 100,
        (byte) 101,
        (byte) 118
            });
            Rijndael rijndael = Rijndael.Create();
            rijndael.Key = passwordDeriveBytes.GetBytes(32);
            rijndael.IV = passwordDeriveBytes.GetBytes(16);
            CryptoStream cryptoStream = new CryptoStream(fileStream2, rijndael.CreateEncryptor(), CryptoStreamMode.Write);
            int count1 = 4096;
            byte[] buffer = new byte[0];
            int count2;
            do
            {
                count2 = fileStream1.Read(buffer, 0, count1);
                cryptoStream.Write(buffer, 0, count2);
            }
            while (count2 != 0);
            cryptoStream.Close();
            fileStream1.Close();
        }

        private static byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV)
        {
            MemoryStream memoryStream = new MemoryStream();
            Rijndael rijndael = Rijndael.Create();
            rijndael.Key = Key;
            rijndael.IV = IV;
            CryptoStream cryptoStream = new CryptoStream(memoryStream, rijndael.CreateDecryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(cipherData, 0, cipherData.Length);
            cryptoStream.Close();
            return memoryStream.ToArray();
        }

        public static string Decrypt(string cipherText, string Password)
        {
            byte[] cipherData = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(Password, new byte[13]
            {
        (byte) 73,
        (byte) 118,
        (byte) 97,
        (byte) 110,
        (byte) 32,
        (byte) 77,
        (byte) 101,
        (byte) 100,
        (byte) 118,
        (byte) 101,
        (byte) 100,
        (byte) 101,
        (byte) 118
            });
            return Encoding.Unicode.GetString(Decrypt(cipherData, passwordDeriveBytes.GetBytes(32), passwordDeriveBytes.GetBytes(16)));
        }

        private static byte[] Decrypt(byte[] cipherData, string Password)
        {
            PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(Password, new byte[13]
            {
        (byte) 73,
        (byte) 118,
        (byte) 97,
        (byte) 110,
        (byte) 32,
        (byte) 77,
        (byte) 101,
        (byte) 100,
        (byte) 118,
        (byte) 101,
        (byte) 100,
        (byte) 101,
        (byte) 118
            });
            return Decrypt(cipherData, passwordDeriveBytes.GetBytes(32), passwordDeriveBytes.GetBytes(16));
        }

        private static void Decrypt(string fileIn, string fileOut, string Password)
        {
            FileStream fileStream1 = new FileStream(fileIn, FileMode.Open, FileAccess.Read);
            FileStream fileStream2 = new FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write);
            PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(Password, new byte[13]
            {
        (byte) 73,
        (byte) 118,
        (byte) 97,
        (byte) 110,
        (byte) 32,
        (byte) 77,
        (byte) 101,
        (byte) 100,
        (byte) 118,
        (byte) 101,
        (byte) 100,
        (byte) 101,
        (byte) 118
            });
            Rijndael rijndael = Rijndael.Create();
            rijndael.Key = passwordDeriveBytes.GetBytes(32);
            rijndael.IV = passwordDeriveBytes.GetBytes(16);
            CryptoStream cryptoStream = new CryptoStream(fileStream2, rijndael.CreateDecryptor(), CryptoStreamMode.Write);
            int count1 = 4096;
            byte[] buffer = new byte[0];
            int count2;
            do
            {
                count2 = fileStream1.Read(buffer, 0, count1);
                cryptoStream.Write(buffer, 0, count2);
            }
            while (count2 != 0);
            cryptoStream.Close();
            fileStream1.Close();
        }
    }
}
