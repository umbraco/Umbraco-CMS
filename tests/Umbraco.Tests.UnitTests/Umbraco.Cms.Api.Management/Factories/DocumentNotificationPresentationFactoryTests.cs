using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Factories;

[TestFixture]
public class DocumentNotificationPresentationFactoryTests
{
    private DocumentNotificationPresentationFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        var actionCollection = new ActionCollection(() =>
        [
            new TestNotifiableAction(),
            new TestNonNotifiableAction(),
        ]);

        var notificationService = new Mock<INotificationService>();
        notificationService
            .Setup(x => x.GetUserNotifications(It.IsAny<IUser>(), It.IsAny<string>()))
            .Returns((IEnumerable<Notification>?)null);

        _factory = new DocumentNotificationPresentationFactory(
            actionCollection,
            notificationService.Object,
            Mock.Of<IBackOfficeSecurityAccessor>());
    }

    [Test]
    public async Task CreateNotificationModelsAsync_Includes_Only_Actions_That_Show_In_Notifier()
    {
        IEnumerable<DocumentNotificationResponseModel> models = await _factory.CreateNotificationModelsAsync(CreateContent());

        var aliases = models.Select(x => x.Alias).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(aliases, Does.Contain(TestNotifiableAction.ActionAlias));
            Assert.That(aliases, Does.Not.Contain(TestNonNotifiableAction.ActionAlias));
        });
    }

    private static IContent CreateContent()
    {
        var content = new Mock<IContent>();
        content.SetupGet(x => x.Path).Returns("-1,1000");
        return content.Object;
    }

    private sealed class TestNotifiableAction : IAction
    {
        public const string ActionAlias = "testnotifiable";

        public string Letter => "Umb.TestNotifiable";

        public string Alias => ActionAlias;

        public bool ShowInNotifier => true;

        public bool CanBePermissionAssigned => false;
    }

    private sealed class TestNonNotifiableAction : IAction
    {
        public const string ActionAlias = "testnonnotifiable";

        public string Letter => "Umb.TestNonNotifiable";

        public string Alias => ActionAlias;

        public bool ShowInNotifier => false;

        public bool CanBePermissionAssigned => true;
    }
}
