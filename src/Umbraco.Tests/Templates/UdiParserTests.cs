using NUnit.Framework;
using Umbraco.Web.Templates;
using System.Linq;
using Umbraco.Core;

namespace Umbraco.Tests.Templates
{
    [TestFixture]
    public class UdiParserTests
    {
        [Test]
        public void Returns_Udi_From_Data_Udi_Html_Attributes()
        {
            var input = @"<p>
    <div>
        <img src='/media/12312.jpg' data-udi='umb://media/D4B18427A1544721B09AC7692F35C264' />
    </div>
</p><p><img src='/media/234234.jpg' data-udi=""umb://media-type/B726D735E4C446D58F703F3FBCFC97A5"" /></p>";

            var parser = new UdiParser();
            var result = parser.ParseUdisFromDataAttributes(input).ToList();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(Udi.Parse("umb://media/D4B18427A1544721B09AC7692F35C264"), result[0]);
            Assert.AreEqual(Udi.Parse("umb://media-type/B726D735E4C446D58F703F3FBCFC97A5"), result[1]);
        }
        
    }
}
