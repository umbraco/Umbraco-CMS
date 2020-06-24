using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration;

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
                    fixture.Customize(new AutoMoqCustomization());
                    fixture.Customize<BindingInfo>(c => c.OmitAutoProperties());
                    fixture.Customize<BackOfficeIdentityUser>(
                        u => u.FromFactory<string ,string, string>(
                            (a,b,c) => BackOfficeIdentityUser.CreateNew(Mock.Of<IGlobalSettings>(),a,b,c)));
                }
            }
        }
    }
}
