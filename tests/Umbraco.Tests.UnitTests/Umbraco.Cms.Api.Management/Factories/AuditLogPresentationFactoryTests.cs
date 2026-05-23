using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Factories;

[TestFixture]
public class AuditLogPresentationFactoryTests
{
    [Test]
    public void CreateAuditLogViewModel_Maps_All_Fields()
    {
        var userKey = Guid.NewGuid();
        var userIdKeyResolverMock = new Mock<IUserIdKeyResolver>();
        userIdKeyResolverMock
            .Setup(x => x.GetAsync(5))
            .ReturnsAsync(userKey);

        var factory = new AuditLogPresentationFactory(
            Mock.Of<IUserService>(),
            userIdKeyResolverMock.Object);

        var createDate = new DateTime(2026, 3, 15, 10, 30, 0, DateTimeKind.Utc);
        var auditItem = new AuditItem(
            100,
            AuditType.Publish,
            5,
            "Document",
            "#auditTrails_contentPublished",
            "en-US,da-DK",
            createDate,
            triggerSource: "Core",
            triggerOperation: "ScheduledPublish",
            typeAlias: "Umb.Custom.Action");

        var results = factory.CreateAuditLogViewModel(new[] { auditItem }).ToList();

        Assert.AreEqual(1, results.Count);

        var model = results[0];
        Assert.Multiple(() =>
        {
            Assert.AreEqual(AuditType.Publish, model.LogType);
            Assert.AreEqual("#auditTrails_contentPublished", model.Comment);
            Assert.AreEqual("en-US,da-DK", model.Parameters);
            Assert.AreEqual(new DateTimeOffset(createDate), model.Timestamp);
            Assert.AreEqual(userKey, model.User.Id);
            Assert.AreEqual("Core", model.TriggerSource);
            Assert.AreEqual("ScheduledPublish", model.TriggerOperation);
            Assert.AreEqual("Umb.Custom.Action", model.TypeAlias);
        });
    }

    [Test]
    public void CreateAuditLogViewModel_Maps_Null_Fields_And_Unknown_User()
    {
        var factory = new AuditLogPresentationFactory(
            Mock.Of<IUserService>(),
            Mock.Of<IUserIdKeyResolver>());

        var auditItem = new AuditItem(
            200,
            AuditType.Save,
            Constants.Security.UnknownUserId,
            "Document",
            "Saved",
            null,
            DateTime.UtcNow,
            triggerSource: null,
            triggerOperation: null,
            typeAlias: null);

        var results = factory.CreateAuditLogViewModel([auditItem]).ToList();

        Assert.AreEqual(1, results.Count);

        var model = results[0];
        Assert.Multiple(() =>
        {
            Assert.AreEqual(AuditType.Save, model.LogType);
            Assert.AreEqual(Guid.Empty, model.User.Id);
            Assert.IsNull(model.TriggerSource);
            Assert.IsNull(model.TriggerOperation);
            Assert.IsNull(model.TypeAlias);
            Assert.IsNull(model.Parameters);
        });
    }
}
