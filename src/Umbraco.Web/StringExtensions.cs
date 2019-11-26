using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Security;

namespace Umbraco.Web
{
    public static class StringExtensions
    {
        /// <summary>
        /// Encrypt the string using the MachineKey in medium trust
        /// </summary>
        /// <param name="value">The string value to be encrypted.</param>
        /// <returns>The encrypted string.</returns>
        public static string EncryptWithMachineKey(this string value)
        {
            if (value == null)
                return null;

            string valueToEncrypt = value;
            List<string> parts = new List<string>();

            const int EncrpytBlockSize = 500;

            while (valueToEncrypt.Length > EncrpytBlockSize)
            {
                parts.Add(valueToEncrypt.Substring(0, EncrpytBlockSize));
                valueToEncrypt = valueToEncrypt.Remove(0, EncrpytBlockSize);
            }

            if (valueToEncrypt.Length > 0)
            {
                parts.Add(valueToEncrypt);
            }

            StringBuilder encrpytedValue = new StringBuilder();

            foreach (var part in parts)
            {
                var encrpytedBlock = FormsAuthentication.Encrypt(new FormsAuthenticationTicket(0, string.Empty, DateTime.Now, DateTime.MaxValue, false, part));
                encrpytedValue.AppendLine(encrpytedBlock);
            }

            return encrpytedValue.ToString().TrimEnd();
        }

        /// <summary>
        /// Decrypt the encrypted string using the Machine key in medium trust
        /// </summary>
        /// <param name="value">The string value to be decrypted</param>
        /// <returns>The decrypted string.</returns>
        public static string DecryptWithMachineKey(this string value)
        {
            if (value == null)
                return null;

            string[] parts = value.Split('\n');

            StringBuilder decryptedValue = new StringBuilder();

            foreach (var part in parts)
            {
                decryptedValue.Append(FormsAuthentication.Decrypt(part.TrimEnd()).UserData);
            }

            return decryptedValue.ToString();
        }
    }
}
