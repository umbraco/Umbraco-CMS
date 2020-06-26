using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration;
using Umbraco.Web.BackOffice.Controllers;

namespace Umbraco.Tests.Common.AutoFixture
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
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
                        .Customize(new ConstructorCustomization(typeof(BackOfficeUserManager), new GreedyConstructorQuery()))
                        .Customize(new AutoMoqCustomization());

                    fixture.Customize<IUserStore<BackOfficeIdentityUser>>(cc => cc.FromFactory(() => Mock.Of<IUserLockoutStore<BackOfficeIdentityUser>>()));




                }
            }
        }
    }
}
