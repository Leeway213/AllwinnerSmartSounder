using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace UWPHelpers
{
    public class CryptographyHelper
    {
        public const string SECURITY_KEY = "PfbpQ1dwJC8pWbFBpITt7d8YOmeDv/NZQlmF/ShzbU9F8ezuZsKcmhhQ3w9q7IKeZdQC6iJF/LYiVTv+9fB4rg==";

        public static byte[] Encrypt(byte[] data, string keyString)
        {

            //初始化加密算法provider，使用DES_ECB_PKCS7算法
            SymmetricKeyAlgorithmProvider symmetricProvider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.Rc4);

            //根据keystring生成key
            IBuffer key = CryptographicBuffer.DecodeFromBase64String(keyString);
            CryptographicKey cryKey = symmetricProvider.CreateSymmetricKey(key);

            //加密数据
            IBuffer buffer = CryptographicBuffer.CreateFromByteArray(data);
            try
            {
                var encrypted = CryptographicEngine.Encrypt(cryKey, buffer, null);
                buffer.AsStream().Dispose();
                return encrypted.ToArray();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static IBuffer Decrypt(byte[] data, string keyString)
        {
            SymmetricKeyAlgorithmProvider symmetricProvider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.Rc4);
            IBuffer key = CryptographicBuffer.DecodeFromBase64String(keyString);
            CryptographicKey cryKey = symmetricProvider.CreateSymmetricKey(key);
            IBuffer buffer = CryptographicBuffer.CreateFromByteArray(data);
            try
            {
                var decrypted = CryptographicEngine.Decrypt(cryKey, buffer, null);
                buffer.AsStream().Dispose();
                return decrypted;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
