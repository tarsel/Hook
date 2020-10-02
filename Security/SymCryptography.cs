using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Hook.Security
{
    public class SymCryptography
    {
        private string mKey = string.Empty;
        private string mSalt = string.Empty;
        private ServiceProviderEnum mAlgorithm;
        private SymmetricAlgorithm mCryptoService;

        public string Key
        {
            get
            {
                return this.mKey;
            }
            set
            {
                this.mKey = value;
            }
        }

        public string Salt
        {
            get
            {
                return this.mSalt;
            }
            set
            {
                this.mSalt = value;
            }
        }

        public SymCryptography()
        {
            this.mCryptoService = new RijndaelManaged();
            this.mCryptoService.Mode = CipherMode.CBC;
            this.mAlgorithm = ServiceProviderEnum.Rijndael;
        }

        public SymCryptography(ServiceProviderEnum serviceProvider)
        {
            switch (serviceProvider)
            {
                case ServiceProviderEnum.Rijndael:
                    this.mCryptoService = new RijndaelManaged();
                    this.mAlgorithm = ServiceProviderEnum.Rijndael;
                    break;
                case ServiceProviderEnum.RC2:
                    this.mCryptoService = new RC2CryptoServiceProvider();
                    this.mAlgorithm = ServiceProviderEnum.RC2;
                    break;
                case ServiceProviderEnum.DES:
                    this.mCryptoService = new DESCryptoServiceProvider();
                    this.mAlgorithm = ServiceProviderEnum.DES;
                    break;
                case ServiceProviderEnum.TripleDES:
                    this.mCryptoService = new TripleDESCryptoServiceProvider();
                    this.mAlgorithm = ServiceProviderEnum.TripleDES;
                    break;
            }
            this.mCryptoService.Mode = CipherMode.CBC;
        }

        public SymCryptography(string serviceProviderName)
        {
            try
            {
                switch (serviceProviderName.ToLower())
                {
                    case "rijndael":
                        serviceProviderName = "Rijndael";
                        this.mAlgorithm = ServiceProviderEnum.Rijndael;
                        break;
                    case "rc2":
                        serviceProviderName = "RC2";
                        this.mAlgorithm = ServiceProviderEnum.RC2;
                        break;
                    case "des":
                        serviceProviderName = "DES";
                        this.mAlgorithm = ServiceProviderEnum.DES;
                        break;
                    case "tripledes":
                        serviceProviderName = "TripleDES";
                        this.mAlgorithm = ServiceProviderEnum.TripleDES;
                        break;
                }
                this.mCryptoService = (SymmetricAlgorithm)CryptoConfig.CreateFromName(serviceProviderName);
                this.mCryptoService.Mode = CipherMode.CBC;
            }
            catch
            {
                throw;
            }
        }

        private void SetLegalIV()
        {
            if (this.mAlgorithm == ServiceProviderEnum.Rijndael)
                this.mCryptoService.IV = new byte[16]
                {
          (byte) 15,
          (byte) 111,
          (byte) 19,
          (byte) 46,
          (byte) 53,
          (byte) 194,
          (byte) 205,
          (byte) 249,
          (byte) 5,
          (byte) 70,
          (byte) 156,
          (byte) 234,
          (byte) 168,
          (byte) 75,
          (byte) 115,
          (byte) 204
                };
            else
                this.mCryptoService.IV = new byte[8]
                {
          (byte) 15,
          (byte) 111,
          (byte) 19,
          (byte) 46,
          (byte) 53,
          (byte) 194,
          (byte) 205,
          (byte) 249
                };
        }

        public virtual byte[] GetLegalKey()
        {
            if (this.mCryptoService.LegalKeySizes.Length > 0)
            {
                int num1 = this.mKey.Length * 8;
                int minSize = this.mCryptoService.LegalKeySizes[0].MinSize;
                int maxSize = this.mCryptoService.LegalKeySizes[0].MaxSize;
                int skipSize = this.mCryptoService.LegalKeySizes[0].SkipSize;
                if (num1 > maxSize)
                    this.mKey = this.mKey.Substring(0, maxSize / 8);
                else if (num1 < maxSize)
                {
                    int num2 = num1 <= minSize ? minSize : num1 - num1 % skipSize + skipSize;
                    if (num1 < num2)
                        this.mKey = this.mKey.PadRight(num2 / 8, '*');
                }
            }
            return new PasswordDeriveBytes(this.mKey, Encoding.ASCII.GetBytes(this.mSalt)).GetBytes(this.mKey.Length);
        }

        public virtual string Encrypt(string plainText)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(plainText);
            this.mCryptoService.Key = this.GetLegalKey();
            this.SetLegalIV();
            ICryptoTransform encryptor = this.mCryptoService.CreateEncryptor();
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(bytes, 0, bytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] inArray = memoryStream.ToArray();
            return Convert.ToBase64String(inArray, 0, inArray.GetLength(0));
        }

        public virtual string Decrypt(string cryptoText)
        {
            byte[] buffer = Convert.FromBase64String(cryptoText);
            this.mCryptoService.Key = this.GetLegalKey();
            this.SetLegalIV();
            ICryptoTransform decryptor = this.mCryptoService.CreateDecryptor();
            try
            {
                return new StreamReader(new CryptoStream(new MemoryStream(buffer, 0, buffer.Length), decryptor, CryptoStreamMode.Read)).ReadToEnd();
            }
            catch
            {
                return (string)null;
            }
        }

        public enum ServiceProviderEnum
        {
            Rijndael,
            RC2,
            DES,
            TripleDES,
        }
    }
}