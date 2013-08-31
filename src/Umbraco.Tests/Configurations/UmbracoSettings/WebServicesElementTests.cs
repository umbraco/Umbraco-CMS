using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class WebServicesElementTests : UmbracoSettingsTests
    {
        [Test]
        public void Enabled()
        {
            Assert.IsTrue(Section.WebServices.Enabled == true);

        }
        [Test]
        public void FileServiceFolders()
        {
            Assert.IsTrue(Section.WebServices.FileServiceFolders.First() == "css");
            Assert.IsTrue(Section.WebServices.FileServiceFolders.Last() == "xslt");

        }
        [Test]
        public void DocumentServiceUsers()
        {
            Assert.IsTrue(Section.WebServices.DocumentServiceUsers.First() == "your-username1");

        }
        [Test]
        public void FileServiceUsers()
        {
            Assert.IsTrue(Section.WebServices.FileServiceUsers.First() == "your-username2");

        }
        [Test]
        public void StylesheetServiceUsers()
        {
            Assert.IsTrue(Section.WebServices.StylesheetServiceUsers.First() == "your-username3");

        }
        [Test]
        public void MemberServiceUsers()
        {
            Assert.IsTrue(Section.WebServices.MemberServiceUsers.First() == "your-username4");

        }
        [Test]
        public void TemplateServiceUsers()
        {
            Assert.IsTrue(Section.WebServices.TemplateServiceUsers.First() == "your-username5");

        }
        [Test]
        public void MediaServiceUsers()
        {
            Assert.IsTrue(Section.WebServices.MediaServiceUsers.First() == "your-username6");

        }
        [Test]
        public void MaintenanceServiceUsers()
        {
            Assert.IsTrue(Section.WebServices.MaintenanceServiceUsers.Any() == false);

        }
        

    }
}