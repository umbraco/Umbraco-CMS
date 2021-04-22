using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// Used to generate a machine key
    /// </summary>
    internal class MachineKeyGenerator
    {
        /// <summary>
        /// Generates the string to be stored in the web.config
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Machine key details are here: https://msdn.microsoft.com/en-us/library/vstudio/w8h3skw9%28v=vs.100%29.aspx?f=255&MSPPError=-2147217396
        /// </remarks>
        public string GenerateConfigurationBlock()
        {
            var c = @"<machineKey validationKey=""{0}""
    decryptionKey=""{1}""
    validation=""HMACSHA256"" decryption=""AES""
    />";

            return string.Format(c, GenerateAESDecryptionKey(), GenerateHMACSHA256ValidationKey());
        }

        public string GenerateHMACSHA256ValidationKey()
        {
            //See: https://msdn.microsoft.com/en-us/library/vstudio/w8h3skw9%28v=vs.100%29.aspx?f=255&MSPPError=-2147217396
            //See: https://msdn.microsoft.com/en-us/library/ff649308.aspx?f=255&MSPPError=-2147217396
            /*
                key value  Specifies a manually assigned key.
                The validationKey value must be manually set to a string of hexadecimal
                characters to ensure consistent configuration across all servers in a Web farm.
                The length of the key depends on the hash algorithm that is used:

                AES requires a 256-bit key (64 hexadecimal characters).
                MD5 requires a 128-bit key (32 hexadecimal characters).
                SHA1 requires a 160-bit key (40 hexadecimal characters).
                3DES requires a 192-bit key (48 hexadecimal characters).
                HMACSHA256 requires a 256-bit key (64 hexadecimal characters) == DEFAULT
                HMACSHA384 requires a 384-bit key (96 hexadecimal characters).
                HMACSHA512 requires a 512-bit key (128 hexadecimal characters).
            */

            //64 in length = 256 bits
            return GenerateKey(64);
        }

        public string GenerateAESDecryptionKey()
        {
            //See: //See: https://msdn.microsoft.com/en-us/library/vstudio/w8h3skw9%28v=vs.100%29.aspx?f=255&MSPPError=-2147217396
            /*
            key value  Specifies a manually assigned key.
            The decryptionKey value must be manually set to a string of
            hexadecimal characters to ensure consistent configuration across all servers in a Web farm.
            The key should be 64 bits (16 hexadecimal characters) long for DES encryption, or 192 bits
            (48 hexadecimal characters) long for 3DES. For AES, the key can be 128 bits (32 characters),
            192 bits (48 characters), or 256 bits (64 characters) long.
            */

            //64 in length = 256 bits
            return GenerateKey(64);
        }

        private string GenerateKey(int len = 64)
        {
            var buff = new byte[len / 2];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(buff);
                var sb = new StringBuilder(len);

                for (int i = 0; i < buff.Length; i++)
                    sb.Append(string.Format("{0:X2}", buff[i]));

                return sb.ToString();
            }
        }
    }
}
