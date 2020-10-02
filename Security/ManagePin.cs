using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Configuration;
using System.Data.SqlClient;

using Dapper;

using Hook.Helper;
using Hook.Models;

namespace Hook.Security
{
    public class ManagePin
    {
        private string sqlConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString.ToString();
        private RandomStringGenerator randomStringGenerator = new RandomStringGenerator();

        public long EncryptPin(string pin, bool useHash)
        {
            try
            {
                string SecurityCode = randomStringGenerator.NextString(512, true, true, true, true);
                string pinKey = randomStringGenerator.NextString(512, true, true, true, true);
                string str = EncryptPin(pin, pinKey, SecurityCode, useHash);
                PasswordHashKey entity = new PasswordHashKey();
                entity.PINHash = str;
                entity.PinKeyIV = pinKey;
                entity.SecurityCodeIV = SecurityCode;

                var affectedRows = 0;
             //   long passwordHashKeyId = 0;

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();
                    affectedRows = connection.Execute("Insert into PasswordHashKey (PINHash, PinKeyIV, SecurityCodeIV) values (@PINHash, @PinKeyIV, @SecurityCodeIV)", new { entity.PINHash, entity.PinKeyIV, entity.SecurityCodeIV });

                    connection.Close();
                }

                if (affectedRows == 1)
                {
                    using (var connection = new SqlConnection(sqlConnectionString))
                    {
                        entity = connection.Query<PasswordHashKey>("SELECT * FROM PasswordHashKey WHERE PINHash=@PINHash", new { entity.PINHash }).SingleOrDefault();
                    }
                }

                return entity.PasswordHashKeyId;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string EncryptPassPhrase(string password, bool useHash, out string cipherKeyIV, out string cryptographicSalt)
        {
            try
            {
                cipherKeyIV = randomStringGenerator.NextTokenString(512, true, true, true, true);
                cryptographicSalt = randomStringGenerator.NextTokenString(512, true, true, true, true);
                return EncryptPin(password, cipherKeyIV, cryptographicSalt, useHash);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private string EncryptPin(string pin, string pinKey, string SecurityCode, bool useHash)
        {
            try
            {
                string plainText = EncDec.Encrypt(EncDec.Encrypt(pin, SecurityCode), pinKey);
                if (!useHash)
                    return plainText;
                return new Hash(Hash.ServiceProviderEnum.SHA512)
                {
                    Salt = pinKey
                }.Encrypt(plainText);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool ValidatePin(long passwordHashKeyId, string pin)
        {
            try
            {
                PasswordHashKey passwordHashKey = new PasswordHashKey();

                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    passwordHashKey = connection.Query<PasswordHashKey>("SELECT * FROM PasswordHashKey WHERE PasswordHashKeyId=@PasswordHashKeyId", new { passwordHashKeyId }).SingleOrDefault();
                }

                return passwordHashKey != null && !(EncryptPin(pin, passwordHashKey.PinKeyIV, passwordHashKey.SecurityCodeIV, true) != passwordHashKey.PINHash);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string RetrieveAccountPin(long passwordHashKeyId)
        {
            PasswordHashKey passwordHashKey = new PasswordHashKey();

            using (var connection = new SqlConnection(sqlConnectionString))
            {
                passwordHashKey = connection.Query<PasswordHashKey>("SELECT * FROM PasswordHashKey WHERE PasswordHashKeyId=@PasswordHashKeyId", new { passwordHashKeyId }).SingleOrDefault();
            }

            if (passwordHashKey == null)
                throw new Exception("CA0010 - Customer Pin has not been set");
            try
            {
                return EncDec.Decrypt(EncDec.Decrypt(passwordHashKey.PINHash, passwordHashKey.PinKeyIV), passwordHashKey.SecurityCodeIV);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}