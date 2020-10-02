using System;
using System.Security.Cryptography;
using System.Text;

namespace Hook.Security
{
    public class Hash
    {
        private HashAlgorithm mCryptoService;

        public string Salt { get; set; }

        public Hash()
        {
            this.mCryptoService = new SHA1Managed();
        }

        public Hash(ServiceProviderEnum serviceProvider)
        {
            switch (serviceProvider)
            {
                case ServiceProviderEnum.SHA1:
                    mCryptoService = new SHA1Managed();
                    break;
                case ServiceProviderEnum.SHA256:
                    mCryptoService = new SHA256Managed();
                    break;
                case ServiceProviderEnum.SHA384:
                    mCryptoService = new SHA384Managed();
                    break;
                case ServiceProviderEnum.SHA512:
                    mCryptoService = new SHA512Managed();
                    break;
                case ServiceProviderEnum.MD5:
                    mCryptoService = new MD5CryptoServiceProvider();
                    break;
            }
        }

        public Hash(string serviceProviderName)
        {
            try
            {
                mCryptoService = (HashAlgorithm)CryptoConfig.CreateFromName(serviceProviderName.ToUpper());
            }
            catch
            {
                throw;
            }
        }

        public virtual string Encrypt(string plainText)
        {
            byte[] hash = mCryptoService.ComputeHash(Encoding.ASCII.GetBytes(plainText + Salt));
            return Convert.ToBase64String(hash, 0, hash.Length);
        }

        public virtual string Encrypt(byte[] byteArr)
        {
            return Convert.ToBase64String(mCryptoService.ComputeHash(byteArr));
        }

        public enum ServiceProviderEnum
        {
            SHA1,
            SHA256,
            SHA384,
            SHA512,
            MD5,
        }
    }
}