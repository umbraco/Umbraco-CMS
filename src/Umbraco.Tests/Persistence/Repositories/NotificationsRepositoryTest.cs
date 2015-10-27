using System;
using System.Globalization;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class NotificationsRepositoryTest : BaseDatabaseFactoryTest
    {
        [Test]
        public void CreateNotification()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            var repo = new NotificationsRepository(unitOfWork);

            var node = new NodeDto {CreateDate = DateTime.Now, Level = 1, NodeObjectType = Guid.Parse(Constants.ObjectTypes.ContentItem), ParentId = -1, Path = "-1,123", SortOrder = 1, Text = "hello", Trashed = false, UniqueId = Guid.NewGuid(), UserId = 0};
            var result = unitOfWork.Database.Insert(node);
            var entity = Mock.Of<IEntity>(e => e.Id == node.NodeId);
            var user = Mock.Of<IUser>(e => e.Id == node.UserId);

            var notification = repo.CreateNotification(user, entity, "A");

            Assert.AreEqual("A", notification.Action);
            Assert.AreEqual(node.NodeId, notification.EntityId);
            Assert.AreEqual(node.NodeObjectType, notification.EntityType);
            Assert.AreEqual(node.UserId, notification.UserId);
        }

        [Test]
        public void GetUserNotifications()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            var repo = new NotificationsRepository(unitOfWork);

            var userDto = new UserDto { ContentStartId = -1, Email = "test" , Login = "test" , MediaStartId = -1, Password = "test" , Type = 1, UserName = "test" , UserLanguage = "en" };
            unitOfWork.Database.Insert(userDto);

            var userNew = Mock.Of<IUser>(e => e.Id == userDto.Id);
            var userAdmin = Mock.Of<IUser>(e => e.Id == 0);

            for (var i = 0; i < 10; i++)
            {
                var node = new NodeDto { CreateDate = DateTime.Now, Level = 1, NodeObjectType = Guid.Parse(Constants.ObjectTypes.ContentItem), ParentId = -1, Path = "-1," + i, SortOrder = 1, Text = "hello" + i, Trashed = false, UniqueId = Guid.NewGuid(), UserId = 0 };
                var result = unitOfWork.Database.Insert(node);
                var entity = Mock.Of<IEntity>(e => e.Id == node.NodeId);
                var notification = repo.CreateNotification((i % 2 == 0) ? userAdmin : userNew, entity, i.ToString(CultureInfo.InvariantCulture));    
            }

            var notifications = repo.GetUserNotifications(userAdmin);

            Assert.AreEqual(5, notifications.Count());            
        }

        [Test]
        public void GetEntityNotifications()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            var repo = new NotificationsRepository(unitOfWork);
            
            var node1 = new NodeDto { CreateDate = DateTime.Now, Level = 1, NodeObjectType = Guid.Parse(Constants.ObjectTypes.ContentItem), ParentId = -1, Path = "-1,1", SortOrder = 1, Text = "hello1", Trashed = false, UniqueId = Guid.NewGuid(), UserId = 0 };
            unitOfWork.Database.Insert(node1);
            var entity1 = Mock.Of<IEntity>(e => e.Id == node1.NodeId);
            var node2 = new NodeDto { CreateDate = DateTime.Now, Level = 1, NodeObjectType = Guid.Parse(Constants.ObjectTypes.ContentItem), ParentId = -1, Path = "-1,2", SortOrder = 1, Text = "hello2", Trashed = false, UniqueId = Guid.NewGuid(), UserId = 0 };
            unitOfWork.Database.Insert(node2);
            var entity2 = Mock.Of<IEntity>(e => e.Id == node2.NodeId);

            for (var i = 0; i < 10; i++)
            {
                var userDto = new UserDto { ContentStartId = -1, Email = "test" + i, Login = "test" + i, MediaStartId = -1, Password = "test", Type = 1, UserName = "test" + i, UserLanguage = "en" };
                unitOfWork.Database.Insert(userDto);
                var userNew = Mock.Of<IUser>(e => e.Id == userDto.Id);
                var notification = repo.CreateNotification(userNew, (i % 2 == 0) ? entity1 : entity2, i.ToString(CultureInfo.InvariantCulture));
            }

            var notifications = repo.GetEntityNotifications(entity1);

            Assert.AreEqual(5, notifications.Count());
        }

        [Test]
        public void Delete_By_Entity()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            var repo = new NotificationsRepository(unitOfWork);

            var node1 = new NodeDto { CreateDate = DateTime.Now, Level = 1, NodeObjectType = Guid.Parse(Constants.ObjectTypes.ContentItem), ParentId = -1, Path = "-1,1", SortOrder = 1, Text = "hello1", Trashed = false, UniqueId = Guid.NewGuid(), UserId = 0 };
            unitOfWork.Database.Insert(node1);
            var entity1 = Mock.Of<IEntity>(e => e.Id == node1.NodeId);
            var node2 = new NodeDto { CreateDate = DateTime.Now, Level = 1, NodeObjectType = Guid.Parse(Constants.ObjectTypes.ContentItem), ParentId = -1, Path = "-1,2", SortOrder = 1, Text = "hello2", Trashed = false, UniqueId = Guid.NewGuid(), UserId = 0 };
            unitOfWork.Database.Insert(node2);
            var entity2 = Mock.Of<IEntity>(e => e.Id == node2.NodeId);

            for (var i = 0; i < 10; i++)
            {
                var userDto = new UserDto { ContentStartId = -1, Email = "test" + i, Login = "test" + i, MediaStartId = -1, Password = "test", Type = 1, UserName = "test" + i, UserLanguage = "en" };
                unitOfWork.Database.Insert(userDto);
                var userNew = Mock.Of<IUser>(e => e.Id == userDto.Id);
                var notification = repo.CreateNotification(userNew, (i % 2 == 0) ? entity1 : entity2, i.ToString(CultureInfo.InvariantCulture));
            }

            var delCount = repo.DeleteNotifications(entity1);

            Assert.AreEqual(5, delCount);
        }

        [Test]
        public void Delete_By_User()
        {
            var provider = new PetaPocoUnitOfWorkProvider(Logger);
            var unitOfWork = provider.GetUnitOfWork();
            var repo = new NotificationsRepository(unitOfWork);

            var userDto = new UserDto { ContentStartId = -1, Email = "test", Login = "test", MediaStartId = -1, Password = "test", Type = 1, UserName = "test", UserLanguage = "en" };
            unitOfWork.Database.Insert(userDto);

            var userNew = Mock.Of<IUser>(e => e.Id == userDto.Id);
            var userAdmin = Mock.Of<IUser>(e => e.Id == 0);

            for (var i = 0; i < 10; i++)
            {
                var node = new NodeDto { CreateDate = DateTime.Now, Level = 1, NodeObjectType = Guid.Parse(Constants.ObjectTypes.ContentItem), ParentId = -1, Path = "-1," + i, SortOrder = 1, Text = "hello" + i, Trashed = false, UniqueId = Guid.NewGuid(), UserId = 0 };
                var result = unitOfWork.Database.Insert(node);
                var entity = Mock.Of<IEntity>(e => e.Id == node.NodeId);
                var notification = repo.CreateNotification((i % 2 == 0) ? userAdmin : userNew, entity, i.ToString(CultureInfo.InvariantCulture));
            }

            var delCount = repo.DeleteNotifications(userAdmin);

            Assert.AreEqual(5, delCount);
        }
    }
}