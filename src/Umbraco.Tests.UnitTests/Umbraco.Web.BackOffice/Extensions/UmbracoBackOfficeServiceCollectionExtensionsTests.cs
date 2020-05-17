using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Extensions;
using Umbraco.Core.BackOffice;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.BackOffice.Extensions
{
    [TestFixture]
    public class UmbracoBackOfficeServiceCollectionExtensionsTests
    {
        /*[Test]
        public void AddUmbracoBackOfficeIdentity_ExpectBackOfficeUserStoreResolvable()
        {
            var services = new ServiceCollection();
            
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            
            services.AddUmbracoCore(mockEnvironment.Object);
            services.AddUmbracoBackOfficeIdentity();

            var serviceProvider = services.BuildServiceProvider();

            var userStore = serviceProvider.GetService<IUserStore<BackOfficeIdentityUser>>();
            Assert.AreEqual(typeof(BackOfficeUserStore), userStore.GetType());
        }*/
    }
}
