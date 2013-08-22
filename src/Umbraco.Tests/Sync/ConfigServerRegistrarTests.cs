using System.Linq;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Umbraco.Core.Sync;
using Umbraco.Tests.PartialTrust;

namespace Umbraco.Tests.Sync
{
    [TestFixture]
    public class ConfigServerRegistrarTests
    {

        [TestCase("<server>127.0.0.1</server>", "http://127.0.0.1")]
        [TestCase("<server forceProtocol='https'>www.somedomain.com</server>", "https://www.somedomain.com")]
        [TestCase("<server forcePortnumber='888'>another.domain.com.au</server>", "http://another.domain.com.au:888")]
        [TestCase("<server forcePortnumber='999' forceProtocol='https'>another.domain.com.au</server>", "https://another.domain.com.au:999")]
        public void Ensure_Correct_Format(string xml, string match)
        {
            var xDoc = new XmlDocument();
            xDoc.LoadXml(xml);
            var xNode = xDoc.FirstChild;
            var cReg = new ConfigServerAddress(xNode);

            Assert.AreEqual(match + "/umbraco/webservices/cacheRefresher.asmx", cReg.ServerAddress);
        }

        [Test]
        public void Ensure_Parses_Config_Block()
        {
            var xDoc = new XmlDocument();
            xDoc.LoadXml(@"<servers>
    <server>127.0.0.1</server>
    <server forceProtocol='https'>www.somedomain.com</server>
    <server forcePortnumber='888'>another.domain.com.au</server>
    <server forcePortnumber='999' forceProtocol='https'>another.domain.com.au</server>
</servers>");
            var xNode = xDoc.FirstChild;
            var cReg = new ConfigServerRegistrar(xNode);

            Assert.AreEqual(4, cReg.Registrations.Count());
        }

    }
}
