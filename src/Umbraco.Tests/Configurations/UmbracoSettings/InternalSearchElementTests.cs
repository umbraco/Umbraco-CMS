using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class InternalSearchElementTests : UmbracoSettingsTests
    {
        [Test]
        public void Can_Read_Content_Fields_To_Search_From_InternalSearch_ContentSearchFields_Node()
        {
            int contentFieldCount = SettingsSection.InternalSearch.ContentSearchFields.Count();

            Assert.AreEqual(2,contentFieldCount);

        }

        [Test]
        public void Can_Read_Media_Fields_To_Search_From_InternalSearch_MediaSearchFields_Node()
        {
            int mediaFieldCount = SettingsSection.InternalSearch.MediaSearchFields.Count();

            Assert.AreEqual(2,mediaFieldCount);
        }

    }
}