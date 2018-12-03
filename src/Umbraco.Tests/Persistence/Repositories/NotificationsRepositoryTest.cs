using System;
using System.Globalization;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class NotificationsRepositoryTest : TestWithDatabaseBase
    {
        [Test]
        public void CreateNotification()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {
                var repo = new NotificationsRepository((IScopeAccessor) provider);

                var node = new NodeDto // create bogus item so we can add a notification
                {
                    CreateDate = DateTime.Now,
                    Level = 1,
                    NodeObjectType = Constants.ObjectTypes.ContentItem,
                    ParentId = -1,
                    Path = "-1,123",
                    SortOrder = 1,
                    Text = "hello",
                    Trashed = false,
                    UniqueId = Guid.NewGuid(),
                    UserId = Constants.Security.SuperUserId
                };
                var result = scope.Database.Insert(node);
                var entity = Mock.Of<IEntity>(e => e.Id == node.NodeId);
                var user = Mock.Of<IUser>(e => e.Id == node.UserId);

                var notification = repo.CreateNotification(user, entity, "A");

                Assert.AreEqual("A", notification.Action);
                Assert.AreEqual(node.NodeId, notification.EntityId);
                Assert.AreEqual(node.NodeObjectType, notification.EntityType);
                Assert.AreEqual(node.UserId, notification.UserId);
            }
        }

        [Test]
        public void GetUserNotifications()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {

                var repo = new NotificationsRepository((IScopeAccessor) provider);

                var userDto = new UserDto { Email = "test", Login = "test", Password = "test", UserName = "test", UserLanguage = "en", CreateDate = DateTime.Now, UpdateDate = DateTime.Now };
                scope.Database.Insert(userDto);

                var userNew = Mock.Of<IUser>(e => e.Id == userDto.Id);
                var userAdmin = Mock.Of<IUser>(e => e.Id == Constants.Security.SuperUserId);

                for (var i = 0; i < 10; i++)
                {
                    var node = new NodeDto { CreateDate = DateTime.Now, Level = 1, NodeObjectType = Constants.ObjectTypes.ContentItem, ParentId = -1, Path = "-1," + i, SortOrder = 1, Text = "hello" + i, Trashed = false, UniqueId = Guid.NewGuid(), UserId = -1 };
                    var result = scope.Database.Insert(node);
                    var entity = Mock.Of<IEntity>(e => e.Id == node.NodeId);
                    var notification = repo.CreateNotification((i%2 == 0) ? userAdmin : userNew, entity, i.ToString(CultureInfo.InvariantCulture));
                }

                var notifications = repo.GetUserNotifications(userAdmin);

                Assert.AreEqual(5, notifications.Count());
            }
        }

        [Test]
        public void GetEntityNotifications()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {

                var repo = new NotificationsRepository((IScopeAccessor) provider);

                var node1 = new NodeDto { CreateDate = DateTime.Now, Level = 1, NodeObjectType = Constants.ObjectTypes.ContentItem, ParentId = -1, Path = "-1,1", SortOrder = 1, Text = "hello1", Trashed = false, UniqueId = Guid.NewGuid(), UserId = -1 };
                scope.Database.Insert(node1);
                var entity1 = Mock.Of<IEntity>(e => e.Id == node1.NodeId);
                var node2 = new NodeDto { CreateDate = DateTime.Now, Level = 1, NodeObjectType = Constants.ObjectTypes.ContentItem, ParentId = -1, Path = "-1,2", SortOrder = 1, Text = "hello2", Trashed = false, UniqueId = Guid.NewGuid(), UserId = -1 };
                scope.Database.Insert(node2);
                var entity2 = Mock.Of<IEntity>(e => e.Id == node2.NodeId);

                for (var i = 0; i < 10; i++)
                {
                    var userDto = new UserDto { Email = "test" + i, Login = "test" + i, Password = "test", UserName = "test" + i, UserLanguage = "en", CreateDate = DateTime.Now, UpdateDate = DateTime.Now };
                    scope.Database.Insert(userDto);
                    var userNew = Mock.Of<IUser>(e => e.Id == userDto.Id);
                    var notification = repo.CreateNotification(userNew, (i%2 == 0) ? entity1 : entity2, i.ToString(CultureInfo.InvariantCulture));
                }

                var notifications = repo.GetEntityNotifications(entity1);

                Assert.AreEqual(5, notifications.Count());
            }
        }

        [Test]
        public void Delete_By_Entity()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {

                var repo = new NotificationsRepository((IScopeAccessor) provider);

                var node1 = new NodeDto { CreateDate = DateTime.Now, Level = 1, NodeObjectType = Constants.ObjectTypes.ContentItem, ParentId = -1, Path = "-1,1", SortOrder = 1, Text = "hello1", Trashed = false, UniqueId = Guid.NewGuid(), UserId = -1 };
                scope.Database.Insert(node1);
                var entity1 = Mock.Of<IEntity>(e => e.Id == node1.NodeId);
                var node2 = new NodeDto { CreateDate = DateTime.Now, Level = 1, NodeObjectType = Constants.ObjectTypes.ContentItem, ParentId = -1, Path = "-1,2", SortOrder = 1, Text = "hello2", Trashed = false, UniqueId = Guid.NewGuid(), UserId = -1 };
                scope.Database.Insert(node2);
                var entity2 = Mock.Of<IEntity>(e => e.Id == node2.NodeId);

                for (var i = 0; i < 10; i++)
                {
                    var userDto = new UserDto { Email = "test" + i, Login = "test" + i, Password = "test", UserName = "test" + i, UserLanguage = "en", CreateDate = DateTime.Now, UpdateDate = DateTime.Now };
                    scope.Database.Insert(userDto);
                    var userNew = Mock.Of<IUser>(e => e.Id == userDto.Id);
                    var notification = repo.CreateNotification(userNew, (i%2 == 0) ? entity1 : entity2, i.ToString(CultureInfo.InvariantCulture));
                }

                var delCount = repo.DeleteNotifications(entity1);

                Assert.AreEqual(5, delCount);
            }
        }

        [Test]
        public void Delete_By_User()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = provider.CreateScope())
            {

                var repo = new NotificationsRepository((IScopeAccessor) provider);

                var userDto = new UserDto { Email = "test", Login = "test", Password = "test", UserName = "test", UserLanguage = "en", CreateDate = DateTime.Now, UpdateDate = DateTime.Now };
                scope.Database.Insert(userDto);

                var userNew = Mock.Of<IUser>(e => e.Id == userDto.Id);
                var userAdmin = Mock.Of<IUser>(e => e.Id == Constants.Security.SuperUserId);

                for (var i = 0; i < 10; i++)
                {
                    var node = new NodeDto { CreateDate = DateTime.Now, Level = 1, NodeObjectType = Constants.ObjectTypes.ContentItem, ParentId = -1, Path = "-1," + i, SortOrder = 1, Text = "hello" + i, Trashed = false, UniqueId = Guid.NewGuid(), UserId = -1 };
                    var result = scope.Database.Insert(node);
                    var entity = Mock.Of<IEntity>(e => e.Id == node.NodeId);
                    var notification = repo.CreateNotification((i%2 == 0) ? userAdmin : userNew, entity, i.ToString(CultureInfo.InvariantCulture));
                }

                var delCount = repo.DeleteNotifications(userAdmin);

                Assert.AreEqual(5, delCount);
            }
        }
    }
}
