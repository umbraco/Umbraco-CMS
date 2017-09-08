using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class InternalSearchElementTests : UmbracoSettingsTests
    {
        [Test]
        public void Content_Fields_To_Search_Set()
        {
            int contentFieldCount = SettingsSection.InternalSearch.ContentSearchFields.Split(new char[] {','}).Length;

            Assert.AreEqual(2,contentFieldCount);

        }
        
    }
}