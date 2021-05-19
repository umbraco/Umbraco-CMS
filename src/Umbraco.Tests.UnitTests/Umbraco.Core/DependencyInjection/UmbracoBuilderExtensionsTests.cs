using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DependencyInjection
{
    [TestFixture]
    public class UmbracoBuilderExtensionsTests
    {
        [Test]
        public void CanRegisterAndRetrieveNotificationHandlers()
        {
            IServiceCollection container = TestHelper.GetServiceCollection();
            TypeLoader typeLoader = TestHelper.GetMockedTypeLoader();

            var composition = new UmbracoBuilder(container, Mock.Of<IConfiguration>(), typeLoader);
            composition.AddNotificationHandler<TestNotification, TestNotificationHandler>();

            var result = composition.IsNotificationHandlerRegistered<TestNotification>();
            var result2 = composition.IsNotificationHandlerRegistered<TestNotification2>();

            Assert.IsTrue(result);
            Assert.IsFalse(result2);
        }

        private class TestNotification : INotification
        {
        }

        private class TestNotification2 : INotification
        {
        }

        private class TestNotificationHandler : INotificationHandler<TestNotification>, INotificationHandler<TestNotification2>
        {
            public void Handle(TestNotification notification) => throw new NotImplementedException();

            public void Handle(TestNotification2 notification) => throw new NotImplementedException();
        }
    }
}
