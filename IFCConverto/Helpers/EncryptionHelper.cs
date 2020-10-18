using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace IFCConverto.Helpers
{
    /// <summary>
    /// This class will help in encrypting the username and password that  the utiltiy require from the user for accessing the API
    /// </summary>
    public class EncryptionHelper
    {
        // This will be password that will be used to create the keys for encoding and decoding for consistency
        private const string EncryptPassword = "uC@SLz9u/awSz2Ch`R3~!";

        public static string EncryptString(string originalString)
        {
            try
            {                        
                // Create Aes that generates a new key and initialization vector (IV).    
                // Same key must be used in encryption and decryption    
                using (AesManaged aes = new AesManaged())
                {
                    byte[][] keys = GetHashKeys(EncryptPassword);

                    // Encrypt string    
                    byte[] encryptedData = Encrypt(originalString, keys[0], keys[1]);
                    
                    return Convert.ToBase64String(encryptedData);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }            
        }

        public static string DecryptString(string encryptedString)
        {
            try
            {
                // Create Aes that generates a new key and initialization vector (IV).    
                // Same key must be used in encryption and decryption    
                using (AesManaged aes = new AesManaged())
                {
                    byte[][] keys = GetHashKeys(EncryptPassword);
                    // Decrypt the bytes to a string.    
                    string decryptedString = Decrypt(Convert.FromBase64String(encryptedString), keys[0], keys[1]);
                    return decryptedString;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Private method for encrypting the string
        /// </summary>
        /// <param name="plainText">raw string</param>        
        /// <returns>array of encryupted </returns>
        private static byte[] Encrypt(string plainText, byte[] Key, byte[] IV)
        {
            byte[] encrypted;

            // Create a new AesManaged.    
            using (AesManaged aes = new AesManaged())
            {
                // Create encryptor    
                ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);

                // Create MemoryStream    
                using (MemoryStream ms = new MemoryStream())
                {
                    // Create crypto stream using the CryptoStream class. This class is the key to encryption    
                    // and encrypts and decrypts data from any given stream. In this case, we will pass a memory stream    
                    // to encrypt    
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        // Create StreamWriter and write data to a stream    
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }

                        encrypted = ms.ToArray();
                    }
                }
            }

            // Return encrypted data    
            return encrypted;
        }

        /// <summary>
        /// Private method for decrypting the string
        /// </summary>
        /// <param name="cipherText">encrypted text array</param>        
        /// <returns>decrypted string</returns>
        private static string Decrypt(byte[] cipherText, byte[] Key, byte[] IV)
        {
            string plaintext = null;

            // Create AesManaged    
            using (AesManaged aes = new AesManaged())
            {
                // Create a decryptor    
                ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);

                // Create the streams used for decryption.    
                using (MemoryStream ms = new MemoryStream(cipherText))
                {
                    // Create crypto stream    
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        // Read crypto stream    
                        using (StreamReader reader = new StreamReader(cs))
                        {
                            plaintext = reader.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        /// <summary>
        /// This method is used to generate the Keys to encode and decode the string with. 
        /// This will ensure we are getting same keys for decoding and encoding, thus preventing errors
        /// </summary>
        /// <param name="key">String that is to be encoded</param>
        /// <returns>returns the keys</returns>
        private static byte[][] GetHashKeys(string key)
        {
            byte[][] result = new byte[2][];
            Encoding enc = Encoding.UTF8;

            SHA256 sha2 = new SHA256CryptoServiceProvider();

            byte[] rawKey = enc.GetBytes(key);
            byte[] rawIV = enc.GetBytes(key);

            byte[] hashKey = sha2.ComputeHash(rawKey);
            byte[] hashIV = sha2.ComputeHash(rawIV);

            Array.Resize(ref hashIV, 16);

            result[0] = hashKey;
            result[1] = hashIV;

            return result;
        }
    }
}
