using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Events;

[TestFixture]
public class RelateOnTrashNotificationHandlerTests
{
    [Test]
    public void Content_Moved_Removes_Restore_Relation_Only_For_Items_That_Left_The_Recycle_Bin()
    {
        IRelationType relationType = Mock.Of<IRelationType>(x =>
            x.Alias == Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias);
        IRelation restoredItemRelation = Mock.Of<IRelation>(x => x.RelationType == relationType);
        IRelation leftBehindItemRelation = Mock.Of<IRelation>(x => x.RelationType == relationType);

        var relationService = new Mock<IRelationService>();
        relationService.Setup(x => x.GetByChildId(1001)).Returns([restoredItemRelation]);
        relationService.Setup(x => x.GetByChildId(1002)).Returns([leftBehindItemRelation]);

        RelateOnTrashNotificationHandler handler = CreateHandler(relationService.Object);

        // 1001 has genuinely left the bin (its new path is outside the bin); 1002 was re-homed under the
        // recycle bin root but is still trashed, so its restore relation must be preserved.
        var restoredItem = new MoveEventInfo<IContent>(
            Mock.Of<IContent>(x => x.Id == 1001 && x.Path == "-1,1001"),
            $"{Constants.System.RecycleBinContentPathPrefix}1001",
            Constants.System.Root);
        var leftBehindItem = new MoveEventInfo<IContent>(
            Mock.Of<IContent>(x => x.Id == 1002 && x.Path == $"{Constants.System.RecycleBinContentPathPrefix}1002"),
            $"{Constants.System.RecycleBinContentPathPrefix}1001,1002",
            Constants.System.RecycleBinContent);

        handler.Handle(new ContentMovedNotification([restoredItem, leftBehindItem], new EventMessages()));

        relationService.Verify(x => x.Delete(restoredItemRelation), Times.Once);
        relationService.Verify(x => x.Delete(leftBehindItemRelation), Times.Never);
        relationService.Verify(x => x.GetByChildId(1002), Times.Never);
    }

    [Test]
    public void Media_Moved_Removes_Restore_Relation_Only_For_Items_That_Left_The_Recycle_Bin()
    {
        IRelationType relationType = Mock.Of<IRelationType>(x =>
            x.Alias == Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias);
        IRelation restoredItemRelation = Mock.Of<IRelation>(x => x.RelationType == relationType);
        IRelation leftBehindItemRelation = Mock.Of<IRelation>(x => x.RelationType == relationType);

        var relationService = new Mock<IRelationService>();
        relationService.Setup(x => x.GetByChildId(2001)).Returns([restoredItemRelation]);
        relationService.Setup(x => x.GetByChildId(2002)).Returns([leftBehindItemRelation]);

        RelateOnTrashNotificationHandler handler = CreateHandler(relationService.Object);

        var restoredItem = new MoveEventInfo<IMedia>(
            Mock.Of<IMedia>(x => x.Id == 2001 && x.Path == "-1,2001"),
            $"{Constants.System.RecycleBinMediaPathPrefix}2001",
            Constants.System.Root);
        var leftBehindItem = new MoveEventInfo<IMedia>(
            Mock.Of<IMedia>(x => x.Id == 2002 && x.Path == $"{Constants.System.RecycleBinMediaPathPrefix}2002"),
            $"{Constants.System.RecycleBinMediaPathPrefix}2001,2002",
            Constants.System.RecycleBinMedia);

        handler.Handle(new MediaMovedNotification([restoredItem, leftBehindItem], new EventMessages()));

        relationService.Verify(x => x.Delete(restoredItemRelation), Times.Once);
        relationService.Verify(x => x.Delete(leftBehindItemRelation), Times.Never);
        relationService.Verify(x => x.GetByChildId(2002), Times.Never);
    }

    private static RelateOnTrashNotificationHandler CreateHandler(IRelationService relationService)
        => new(
            relationService,
            Mock.Of<IEntityService>(),
            Mock.Of<ILocalizedTextService>(),
            Mock.Of<IAuditService>(),
#pragma warning disable CS0618 // Type or member is obsolete
            Mock.Of<IScopeProvider>(),
#pragma warning restore CS0618 // Type or member is obsolete
            Mock.Of<IBackOfficeSecurityAccessor>(),
            Mock.Of<IUserIdKeyResolver>());
}
