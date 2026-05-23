using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure;

[TestFixture]
public class AuditItemFactoryTests
{
    [Test]
    public void BuildEntity_Maps_All_Fields()
    {
        var dto = new LogDto
        {
            Id = 1,
            NodeId = 100,
            Header = "Publish",
            UserId = 5,
            EntityType = "Document",
            Comment = "#auditTrails_contentPublished",
            Parameters = "en-US,da-DK",
            Datestamp = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc),
            TriggerSource = "Core",
            TriggerOperation = "ScheduledPublish",
            LogTypeAlias = "Umb.Custom.Action",
        };

        IAuditItem entity = AuditItemFactory.BuildEntity(dto);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(100, entity.Id);
            Assert.AreEqual(AuditType.Publish, entity.AuditType);
            Assert.AreEqual(5, entity.UserId);
            Assert.AreEqual("Document", entity.EntityType);
            Assert.AreEqual("#auditTrails_contentPublished", entity.Comment);
            Assert.AreEqual("en-US,da-DK", entity.Parameters);
            Assert.AreEqual(DateTimeKind.Utc, entity.CreateDate.Kind);
            Assert.AreEqual("Core", entity.TriggerSource);
            Assert.AreEqual("ScheduledPublish", entity.TriggerOperation);
            Assert.AreEqual("Umb.Custom.Action", entity.TypeAlias);
        });
    }

    [Test]
    public void BuildEntity_Handles_Null_And_Unknown_Values()
    {
        var dto = new LogDto
        {
            Id = 2,
            NodeId = 200,
            Header = "NotARealType",
            UserId = null,
            EntityType = null,
            Comment = null,
            Parameters = null,
            Datestamp = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc),
            TriggerSource = null,
            TriggerOperation = null,
            LogTypeAlias = null,
        };

        IAuditItem entity = AuditItemFactory.BuildEntity(dto);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(AuditType.Custom, entity.AuditType);
            Assert.AreEqual(Constants.Security.UnknownUserId, entity.UserId);
            Assert.IsNull(entity.EntityType);
            Assert.IsNull(entity.Comment);
            Assert.IsNull(entity.Parameters);
            Assert.IsNull(entity.TriggerSource);
            Assert.IsNull(entity.TriggerOperation);
            Assert.IsNull(entity.TypeAlias);
        });
    }

    [Test]
    public void BuildDto_Maps_All_Fields()
    {
        var entity = new AuditItem(
            100,
            AuditType.Publish,
            5,
            "Document",
            "#auditTrails_contentPublished",
            "en-US,da-DK",
            null,
            triggerSource: "Core",
            triggerOperation: "ScheduledPublish",
            typeAlias: "Umb.Custom.Action");

        DateTime before = DateTime.UtcNow;
        LogDto dto = AuditItemFactory.BuildDto(entity);
        DateTime after = DateTime.UtcNow;

        Assert.Multiple(() =>
        {
            Assert.AreEqual(100, dto.NodeId);
            Assert.AreEqual("Publish", dto.Header);
            Assert.AreEqual(5, dto.UserId);
            Assert.AreEqual("Document", dto.EntityType);
            Assert.AreEqual("#auditTrails_contentPublished", dto.Comment);
            Assert.AreEqual("en-US,da-DK", dto.Parameters);
            Assert.That(dto.Datestamp, Is.InRange(before, after));
            Assert.AreEqual("Core", dto.TriggerSource);
            Assert.AreEqual("ScheduledPublish", dto.TriggerOperation);
            Assert.AreEqual("Umb.Custom.Action", dto.LogTypeAlias);
        });
    }

    [Test]
    public void BuildEntities_Maps_Collection()
    {
        var dtos = new[]
        {
            new LogDto { Id = 1, NodeId = 1, Header = "Save", Datestamp = DateTime.UtcNow },
            new LogDto { Id = 2, NodeId = 2, Header = "Publish", Datestamp = DateTime.UtcNow },
            new LogDto { Id = 3, NodeId = 3, Header = "Delete", Datestamp = DateTime.UtcNow },
        };

        var entities = AuditItemFactory.BuildEntities(dtos).ToList();

        Assert.AreEqual(3, entities.Count);
        Assert.AreEqual(AuditType.Save, entities[0].AuditType);
        Assert.AreEqual(AuditType.Publish, entities[1].AuditType);
        Assert.AreEqual(AuditType.Delete, entities[2].AuditType);
    }

    [Test]
    public void Round_Trip_Preserves_All_Fields()
    {
        var original = new LogDto
        {
            Id = 1,
            NodeId = 500,
            Header = "Custom",
            UserId = 3,
            EntityType = "Document",
            Comment = "#auditTrails_contentPublished",
            Parameters = "English",
            Datestamp = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc),
            TriggerSource = "Umbraco.Workflow",
            TriggerOperation = "Approve",
            LogTypeAlias = "Umb.Workflow.Approved",
        };

        IAuditItem entity = AuditItemFactory.BuildEntity(original);
        LogDto roundTripped = AuditItemFactory.BuildDto(entity);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(original.Header, roundTripped.Header);
            Assert.AreEqual(original.NodeId, roundTripped.NodeId);
            Assert.AreEqual(original.UserId, roundTripped.UserId);
            Assert.AreEqual(original.EntityType, roundTripped.EntityType);
            Assert.AreEqual(original.Comment, roundTripped.Comment);
            Assert.AreEqual(original.Parameters, roundTripped.Parameters);
            Assert.AreEqual(original.TriggerSource, roundTripped.TriggerSource);
            Assert.AreEqual(original.TriggerOperation, roundTripped.TriggerOperation);
            Assert.AreEqual(original.LogTypeAlias, roundTripped.LogTypeAlias);
        });
    }
}
