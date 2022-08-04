using System;
using System.Security.Cryptography;

namespace Umbraco.Core
{
    public static class CryptoServiceProviderExtensions
    {
        /// <summary>
        /// Generates a random int withing a specified range.
        /// </summary>
        /// <param name="provider">Random bytes provider.</param>
        /// <param name="minValue">The minimum value of the resulting int.</param>
        /// <param name="maxValue">The maximum value of the resulting int.</param>
        /// <returns>A random integer that falls withing the specified range.</returns>
        public static int GetInt32(this RNGCryptoServiceProvider provider, int minValue, int maxValue)
        {
            var randomBytes = new byte[4];
            provider.GetBytes(randomBytes);

            var randomInt = Math.Abs(BitConverter.ToInt32(randomBytes, 0));
            // We call do mod to ensure that the value is within the specified range.
            return randomInt % (maxValue - minValue + 1) + minValue;
        }
    }
}


