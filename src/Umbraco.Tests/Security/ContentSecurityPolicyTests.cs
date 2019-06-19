using NUnit.Framework;
using Umbraco.Core.Security;

namespace Umbraco.Tests.Security
{
    [TestFixture]
    public class NonceProviderTests
    {
        INonceProvider _service;
        [SetUp]
        public void Setup()
        {
            _service = new NonceProvider();
        }

        [Test]
        public void ScriptNonceRequestedMultipleTimesIsTheSame()
        {
            var nonce1 = _service.ScriptNonce;
            var nonce2 = _service.ScriptNonce;

            Assert.That(nonce1, Is.EqualTo(nonce2));
        }

        [Test]
        public void StyleNonceRequestedMultipleTimesIsTheSame()
        {
            var nonce1 = _service.StyleNonce;
            var nonce2 = _service.StyleNonce;

            Assert.That(nonce1, Is.EqualTo(nonce2));
        }

        [Test]
        public void StyleandScriptNonceAreDifferent()
        {
            var scriptNonce = _service.ScriptNonce;
            var styleNonce = _service.StyleNonce;

            Assert.That(styleNonce, Is.Not.EqualTo(scriptNonce));
        }
    }
}
