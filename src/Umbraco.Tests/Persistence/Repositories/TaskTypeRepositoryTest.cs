using System;
using System.Linq;
using NUnit.Framework;
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
    public class TaskTypeRepositoryTest : TestWithDatabaseBase
    {
        [Test]
        public void Can_Delete()
        {
            var provider = TestObjects.GetScopeProvider(Logger);
            using (var scope = ScopeProvider.CreateScope())
            {
                var taskType = new TaskType("asdfasdf");
                var repo = new TaskRepository((IScopeAccessor) provider, CacheHelper, Logger);
                var taskTypeRepo = new TaskTypeRepository((IScopeAccessor) provider, CacheHelper, Logger);

                var created = DateTime.Now;
                var task = new Task(taskType)
                {
                    AssigneeUserId = 0,
                    Closed = false,
                    Comment = "hello world",
                    EntityId = -1,
                    OwnerUserId = 0
                };
                repo.Save(task);
                
                var alltasktypes = taskTypeRepo.GetMany();

                taskTypeRepo.Delete(taskType);

                Assert.AreEqual(alltasktypes.Count() - 1, taskTypeRepo.GetMany().Count());
                Assert.AreEqual(0, repo.GetMany().Count());
            }


        }
    }
}
