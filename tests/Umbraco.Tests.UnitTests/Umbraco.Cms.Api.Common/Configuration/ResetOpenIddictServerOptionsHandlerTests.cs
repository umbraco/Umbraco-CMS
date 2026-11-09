using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OpenIddict.Server;
using OpenIddict.Validation;
using Umbraco.Cms.Api.Common.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.Configuration;

[TestFixture]
internal class ResetOpenIddictServerOptionsHandlerTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void Server_And_Validation_Options_Are_Rebuilt_Only_When_The_Runtime_Restarts(bool isRestarting)
    {
        var serverOptionsCache = new Mock<IOptionsMonitorCache<OpenIddictServerOptions>>();
        var validationOptionsCache = new Mock<IOptionsMonitorCache<OpenIddictValidationOptions>>();
        var sut = new ResetOpenIddictServerOptionsHandler(serverOptionsCache.Object, validationOptionsCache.Object);

        sut.Handle(new UmbracoApplicationStartingNotification(RuntimeLevel.Run, isRestarting));

        Times expected = isRestarting ? Times.Once() : Times.Never();
        serverOptionsCache.Verify(x => x.TryRemove(Options.DefaultName), expected);
        validationOptionsCache.Verify(x => x.TryRemove(Options.DefaultName), expected);
    }
}
