using System;
using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class PackageRepositoriesElementTests : UmbracoSettingsTests
    {
        [Test]
        public virtual void Repositories()
        {
            Assert.IsTrue(SettingsSection.PackageRepositories.Repositories.Count() == 2);
            Assert.IsTrue(SettingsSection.PackageRepositories.Repositories.ElementAt(0).Id == Guid.Parse("65194810-1f85-11dd-bd0b-0800200c9a66"));
            Assert.IsTrue(SettingsSection.PackageRepositories.Repositories.ElementAt(0).Name == "Umbraco package Repository");
            Assert.IsTrue(SettingsSection.PackageRepositories.Repositories.ElementAt(0).HasCustomWebServiceUrl == false);
            Assert.IsTrue(SettingsSection.PackageRepositories.Repositories.ElementAt(0).WebServiceUrl == "/umbraco/webservices/api/repository.asmx");
            Assert.IsTrue(SettingsSection.PackageRepositories.Repositories.ElementAt(0).RepositoryUrl == "http://packages.umbraco.org");
        
            Assert.IsTrue(SettingsSection.PackageRepositories.Repositories.ElementAt(1).Id == Guid.Parse("163245E0-CD22-44B6-841A-1B9B9D2E955F"));
            Assert.IsTrue(SettingsSection.PackageRepositories.Repositories.ElementAt(1).Name == "Test Repo");
            Assert.IsTrue(SettingsSection.PackageRepositories.Repositories.ElementAt(1).HasCustomWebServiceUrl == false);
            Assert.IsTrue(SettingsSection.PackageRepositories.Repositories.ElementAt(0).WebServiceUrl == "/umbraco/webservices/api/repository.asmx");
            Assert.IsTrue(SettingsSection.PackageRepositories.Repositories.ElementAt(0).RepositoryUrl == "http://packages.umbraco.org");
        }
    }
}