using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Identity;
using Moq;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Core;
using Umbraco.Web.Common.Install;

namespace Umbraco.Tests.UnitTests.AutoFixture
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        /// <summary>
        /// Uses AutoFixture to automatically mock (using Moq) the injected types. E.g when injecting interfaces.
        /// AutoFixture is used to generate concrete types. If the concrete type required some types injected, the
        /// [Frozen] can be used to ensure the same variable is injected and available as parameter for the test
        /// </summary>
        public AutoMoqDataAttribute() : base(() => AutoMockCustomizations.Default)
        {
        }

        private static class AutoMockCustomizations
        {
            public static IFixture Default => new Fixture().Customize(new UmbracoCustomization());

            private class UmbracoCustomization : ICustomization
            {
                public void Customize(IFixture fixture)
                {
                    fixture.Customize<BackOfficeIdentityUser>(
                        u => u.FromFactory<string ,string, string>(
                            (a,b,c) => BackOfficeIdentityUser.CreateNew(Mock.Of<IGlobalSettings>(),a,b,c)));
                    fixture
                        .Customize(new ConstructorCustomization(typeof(UsersController), new GreedyConstructorQuery()))
                        .Customize(new ConstructorCustomization(typeof(InstallController), new GreedyConstructorQuery()))
                        .Customize(new ConstructorCustomization(typeof(PreviewController), new GreedyConstructorQuery()))
                        .Customize(new ConstructorCustomization(typeof(BackOfficeController), new GreedyConstructorQuery()))
                        .Customize(new ConstructorCustomization(typeof(BackOfficeUserManager), new GreedyConstructorQuery()))
                        .Customize(new AutoMoqCustomization());

                    // When requesting an IUserStore ensure we actually uses a IUserLockoutStore
                    fixture.Customize<IUserStore<BackOfficeIdentityUser>>(cc => cc.FromFactory(() => Mock.Of<IUserLockoutStore<BackOfficeIdentityUser>>()));

                    fixture.Customize<ConfigConnectionString>(
                        u => u.FromFactory<string, string, string>(
                            (a, b, c) => new ConfigConnectionString(a, b, c)));

                    fixture.Customize<IUmbracoVersion>(
                        u => u.FromFactory(
                            () => new UmbracoVersion()));

                    var connectionStrings = Mock.Of<IConnectionStrings>();
                    Mock.Get(connectionStrings).Setup(x => x[Constants.System.UmbracoConnectionName]).Returns((ConfigConnectionString)new ConfigConnectionString(string.Empty, string.Empty, string.Empty));
                    fixture.Customize<IConnectionStrings>(x => x.FromFactory(() => connectionStrings ));



                }
            }
        }
    }
}
