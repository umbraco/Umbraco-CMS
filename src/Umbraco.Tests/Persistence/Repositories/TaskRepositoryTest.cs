using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class TaskRepositoryTest : TestWithDatabaseBase
    {
        [Test]
        public void Can_Delete()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repo = new TaskRepository((IScopeAccessor) provider, CacheHelper, Logger);

                var created = DateTime.Now;
                var task = new Task(new TaskType("asdfasdf"))
                {
                    AssigneeUserId = Constants.Security.SuperUserId,
                    Closed = false,
                    Comment = "hello world",
                    EntityId = -1,
                    OwnerUserId = Constants.Security.SuperUserId
                };
                repo.Save(task);
                

                repo.Delete(task);
                

                task = repo.Get(task.Id);
                Assert.IsNull(task);
            }
        }

        [Test]
        public void Can_Add()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repo = new TaskRepository((IScopeAccessor) provider, CacheHelper, Logger);

                var created = DateTime.Now;
                repo.Save(new Task(new TaskType("asdfasdf"))
                {
                    AssigneeUserId = Constants.Security.SuperUserId,
                    Closed = false,
                    Comment = "hello world",
                    EntityId = -1,
                    OwnerUserId = Constants.Security.SuperUserId
                });
                

                var found = repo.GetMany().ToArray();

                Assert.AreEqual(1, found.Length);
                Assert.AreEqual(Constants.Security.SuperUserId, found.First().AssigneeUserId);
                Assert.AreEqual(false, found.First().Closed);
                Assert.AreEqual("hello world", found.First().Comment);
                Assert.GreaterOrEqual(found.First().CreateDate.TruncateTo(DateTimeExtensions.DateTruncate.Second), created.TruncateTo(DateTimeExtensions.DateTruncate.Second));
                Assert.AreEqual(-1, found.First().EntityId);
                Assert.AreEqual(Constants.Security.SuperUserId, found.First().OwnerUserId);
                Assert.AreEqual(true, found.First().HasIdentity);
                Assert.AreEqual(true, found.First().TaskType.HasIdentity);
            }
        }

        [Test]
        public void Can_Update()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repo = new TaskRepository((IScopeAccessor) provider, CacheHelper, Logger);

                var task = new Task(new TaskType("asdfasdf"))
                {
                    AssigneeUserId = Constants.Security.SuperUserId,
                    Closed = false,
                    Comment = "hello world",
                    EntityId = -1,
                    OwnerUserId = Constants.Security.SuperUserId
                };

                repo.Save(task);
                

                //re-get
                task = repo.Get(task.Id);

                task.Comment = "blah";
                task.Closed = true;

                repo.Save(task);
                

                //re-get
                task = repo.Get(task.Id);

                Assert.AreEqual(true, task.Closed);
                Assert.AreEqual("blah", task.Comment);
            }
        }

        [Test]
        public void Get_By_Id()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repo = new TaskRepository((IScopeAccessor) provider, CacheHelper, Logger);

                var task = new Task(new TaskType("asdfasdf"))
                {
                    AssigneeUserId = Constants.Security.SuperUserId,
                    Closed = false,
                    Comment = "hello world",
                    EntityId = -1,
                    OwnerUserId = Constants.Security.SuperUserId
                };

                repo.Save(task);
                

                //re-get
                task = repo.Get(task.Id);

                Assert.IsNotNull(task);
            }
        }

        [Test]
        public void Get_All()
        {
            CreateTestData(false, 20);

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repo = new TaskRepository((IScopeAccessor) provider, CacheHelper, Logger);

                var found = repo.GetMany().ToArray();
                Assert.AreEqual(20, found.Count());
            }
        }

        [Test]
        public void Get_All_With_Closed()
        {
            CreateTestData(false, 10);
            CreateTestData(true, 5);

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repo = new TaskRepository((IScopeAccessor) provider, CacheHelper, Logger);

                var found = repo.GetTasks(includeClosed: true).ToArray();
                Assert.AreEqual(15, found.Count());
            }
        }

        [Test]
        public void Get_All_With_Node_Id()
        {
            CreateTestData(false, 10, -20);
            CreateTestData(false, 5, -21);

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repo = new TaskRepository((IScopeAccessor) provider, CacheHelper, Logger);

                var found = repo.GetTasks(itemId:-20).ToArray();
                Assert.AreEqual(10, found.Count());
            }
        }

        [Test]
        public void Get_All_Without_Closed()
        {
            CreateTestData(false, 10);
            CreateTestData(true, 5);

            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repo = new TaskRepository((IScopeAccessor) provider, CacheHelper, Logger);

                var found = repo.GetTasks(includeClosed: false);
                Assert.AreEqual(10, found.Count());
            }
        }

        private void CreateTestData(bool closed, int count, int entityId = -1)
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var repo = new TaskRepository((IScopeAccessor) provider, CacheHelper, Logger);

                for (int i = 0; i < count; i++)
                {
                    repo.Save(new Task(new TaskType("asdfasdf"))
                    {
                        AssigneeUserId = Constants.Security.SuperUserId,
                        Closed = closed,
                        Comment = "hello world " + i,
                        EntityId = entityId,
                        OwnerUserId = Constants.Security.SuperUserId
                    });
                }

                scope.Complete();
            }
        }
    }
}
