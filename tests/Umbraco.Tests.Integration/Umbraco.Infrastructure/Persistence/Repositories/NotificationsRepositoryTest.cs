// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class NotificationsRepositoryTest : UmbracoIntegrationTest
    {
        [Test]
        public void CreateNotification()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                var repo = new NotificationsRepository((IScopeAccessor)provider);

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
                object result = ScopeAccessor.AmbientScope.Database.Insert(node);
                IEntity entity = Mock.Of<IEntity>(e => e.Id == node.NodeId);
                IUser user = Mock.Of<IUser>(e => e.Id == node.UserId);

                Notification notification = repo.CreateNotification(user, entity, "A");

                Assert.AreEqual("A", notification.Action);
                Assert.AreEqual(node.NodeId, notification.EntityId);
                Assert.AreEqual(node.NodeObjectType, notification.EntityType);
                Assert.AreEqual(node.UserId, notification.UserId);
            }
        }

        [Test]
        public void GetUserNotifications()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                var repo = new NotificationsRepository((IScopeAccessor)provider);

                var userDto = new UserDto { Email = "test", Login = "test", Password = "test", UserName = "test", UserLanguage = "en", CreateDate = DateTime.Now, UpdateDate = DateTime.Now };
                ScopeAccessor.AmbientScope.Database.Insert(userDto);

                IUser userNew = Mock.Of<IUser>(e => e.Id == userDto.Id);
                IUser userAdmin = Mock.Of<IUser>(e => e.Id == Constants.Security.SuperUserId);

                for (int i = 0; i < 10; i++)
                {
                    var node = new NodeDto { CreateDate = DateTime.Now, Level = 1, NodeObjectType = Constants.ObjectTypes.ContentItem, ParentId = -1, Path = "-1," + i, SortOrder = 1, Text = "hello" + i, Trashed = false, UniqueId = Guid.NewGuid(), UserId = -1 };
                    object result = ScopeAccessor.AmbientScope.Database.Insert(node);
                    IEntity entity = Mock.Of<IEntity>(e => e.Id == node.NodeId);
                    Notification notification = repo.CreateNotification((i % 2 == 0) ? userAdmin : userNew, entity, i.ToString(CultureInfo.InvariantCulture));
                }

                IEnumerable<Notification> notifications = repo.GetUserNotifications(userAdmin);

                Assert.AreEqual(5, notifications.Count());
            }
        }

        [Test]
        public void GetEntityNotifications()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                var repo = new NotificationsRepository((IScopeAccessor)provider);

                var node1 = new NodeDto { CreateDate = DateTime.Now, Level = 1, NodeObjectType = Constants.ObjectTypes.ContentItem, ParentId = -1, Path = "-1,1", SortOrder = 1, Text = "hello1", Trashed = false, UniqueId = Guid.NewGuid(), UserId = -1 };
                ScopeAccessor.AmbientScope.Database.Insert(node1);
                IEntity entity1 = Mock.Of<IEntity>(e => e.Id == node1.NodeId);
                var node2 = new NodeDto { CreateDate = DateTime.Now, Level = 1, NodeObjectType = Constants.ObjectTypes.ContentItem, ParentId = -1, Path = "-1,2", SortOrder = 1, Text = "hello2", Trashed = false, UniqueId = Guid.NewGuid(), UserId = -1 };
                ScopeAccessor.AmbientScope.Database.Insert(node2);
                IEntity entity2 = Mock.Of<IEntity>(e => e.Id == node2.NodeId);

                for (int i = 0; i < 10; i++)
                {
                    var userDto = new UserDto { Email = "test" + i, Login = "test" + i, Password = "test", UserName = "test" + i, UserLanguage = "en", CreateDate = DateTime.Now, UpdateDate = DateTime.Now };
                    ScopeAccessor.AmbientScope.Database.Insert(userDto);
                    IUser userNew = Mock.Of<IUser>(e => e.Id == userDto.Id);
                    Notification notification = repo.CreateNotification(userNew, (i % 2 == 0) ? entity1 : entity2, i.ToString(CultureInfo.InvariantCulture));
                }

                IEnumerable<Notification> notifications = repo.GetEntityNotifications(entity1);

                Assert.AreEqual(5, notifications.Count());
            }
        }

        [Test]
        public void Delete_By_Entity()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                var repo = new NotificationsRepository((IScopeAccessor)provider);

                var node1 = new NodeDto { CreateDate = DateTime.Now, Level = 1, NodeObjectType = Constants.ObjectTypes.ContentItem, ParentId = -1, Path = "-1,1", SortOrder = 1, Text = "hello1", Trashed = false, UniqueId = Guid.NewGuid(), UserId = -1 };
                ScopeAccessor.AmbientScope.Database.Insert(node1);
                IEntity entity1 = Mock.Of<IEntity>(e => e.Id == node1.NodeId);
                var node2 = new NodeDto { CreateDate = DateTime.Now, Level = 1, NodeObjectType = Constants.ObjectTypes.ContentItem, ParentId = -1, Path = "-1,2", SortOrder = 1, Text = "hello2", Trashed = false, UniqueId = Guid.NewGuid(), UserId = -1 };
                ScopeAccessor.AmbientScope.Database.Insert(node2);
                IEntity entity2 = Mock.Of<IEntity>(e => e.Id == node2.NodeId);

                for (int i = 0; i < 10; i++)
                {
                    var userDto = new UserDto { Email = "test" + i, Login = "test" + i, Password = "test", UserName = "test" + i, UserLanguage = "en", CreateDate = DateTime.Now, UpdateDate = DateTime.Now };
                    ScopeAccessor.AmbientScope.Database.Insert(userDto);
                    IUser userNew = Mock.Of<IUser>(e => e.Id == userDto.Id);
                    Notification notification = repo.CreateNotification(userNew, (i % 2 == 0) ? entity1 : entity2, i.ToString(CultureInfo.InvariantCulture));
                }

                int delCount = repo.DeleteNotifications(entity1);

                Assert.AreEqual(5, delCount);
            }
        }

        [Test]
        public void Delete_By_User()
        {
            IScopeProvider provider = ScopeProvider;
            using (IScope scope = provider.CreateScope())
            {
                var repo = new NotificationsRepository((IScopeAccessor)provider);

                var userDto = new UserDto { Email = "test", Login = "test", Password = "test", UserName = "test", UserLanguage = "en", CreateDate = DateTime.Now, UpdateDate = DateTime.Now };
                ScopeAccessor.AmbientScope.Database.Insert(userDto);

                IUser userNew = Mock.Of<IUser>(e => e.Id == userDto.Id);
                IUser userAdmin = Mock.Of<IUser>(e => e.Id == Constants.Security.SuperUserId);

                for (int i = 0; i < 10; i++)
                {
                    var node = new NodeDto { CreateDate = DateTime.Now, Level = 1, NodeObjectType = Constants.ObjectTypes.ContentItem, ParentId = -1, Path = "-1," + i, SortOrder = 1, Text = "hello" + i, Trashed = false, UniqueId = Guid.NewGuid(), UserId = -1 };
                    object result = ScopeAccessor.AmbientScope.Database.Insert(node);
                    IEntity entity = Mock.Of<IEntity>(e => e.Id == node.NodeId);
                    Notification notification = repo.CreateNotification((i % 2 == 0) ? userAdmin : userNew, entity, i.ToString(CultureInfo.InvariantCulture));
                }

                int delCount = repo.DeleteNotifications(userAdmin);

                Assert.AreEqual(5, delCount);
            }
        }
    }
}
