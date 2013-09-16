//using System.Linq;
//using NUnit.Framework;
//using Rhino.Mocks;
//using Umbraco.Core.Configuration;
//using Umbraco.Core.Configuration.UmbracoSettings;
//using Umbraco.Tests.TestHelpers;

//namespace Umbraco.Tests.Configurations.UmbracoSettings
//{
//    [TestFixture]
//    public class CanGenerateMockSettings
//    {
//        [Test]
//        public void Can_Mock_Multi_Levels()
//        {
//            var repos = MockRepository.GenerateStub<IRepository>();
//            repos.Stub(x => x.Name).Return("This is a test");
         
//            var settings = SettingsForTests.GetMockSettings();
//            settings.Stub(x => x.Content.UseLegacyXmlSchema).Return(true);
//            settings.Stub(x => x.PackageRepositories.Repositories).Return(new[] {repos});
//            SettingsForTests.ConfigureSettings(settings);

//            Assert.AreEqual(true, UmbracoConfiguration.Current.UmbracoSettings.Content.UseLegacyXmlSchema);
//            Assert.AreEqual("This is a test", UmbracoConfiguration.Current.UmbracoSettings.PackageRepositories.Repositories.Single().Name);
//        }
//    }
//}