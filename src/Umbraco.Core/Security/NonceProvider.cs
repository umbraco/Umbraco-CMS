using System;
using System.Security.Cryptography;

namespace Umbraco.Core.Security
{
    internal class NonceProvider : INonceProvider
    {
        private string _scriptNonce;
        private string _styleNonce;
        private readonly RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();
        private readonly object _lock = new object();

        private string GenerateRandomBase64String()
        {
            var nonceBytes = new byte[32];
            _rng.GetBytes(nonceBytes);
            return Convert.ToBase64String(nonceBytes);

        }

        public string StyleNonce
        {
            get
            {
                if (string.IsNullOrEmpty(_scriptNonce))
                {
                    lock (_lock)
                    {

                        if (string.IsNullOrEmpty(_scriptNonce))
                        {
                            _styleNonce = GenerateRandomBase64String();
                        }
                    }
                }
                return _styleNonce;
            }
        }
        public string ScriptNonce
        {
            get
            {
                if (string.IsNullOrEmpty(_scriptNonce))
                {
                    lock (_lock)
                    {

                        if (string.IsNullOrEmpty(_scriptNonce))
                        {
                            _scriptNonce = GenerateRandomBase64String();
                        }
                    }
                }
                return _scriptNonce;
            }
        }
    }
}
