using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Repositories
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class TaskTypeRepositoryTest : BaseDatabaseFactoryTest
    {
        [Test]
        public void Can_Delete()
        {
            var provider = TestObjects.GetDatabaseUnitOfWorkProvider(Logger);
            using (var unitOfWork = provider.CreateUnitOfWork())
            {
                var taskType = new TaskType("asdfasdf");
                var repo = new TaskRepository(unitOfWork, CacheHelper, Logger, MappingResolver);
                var taskTypeRepo = new TaskTypeRepository(unitOfWork, CacheHelper, Logger, MappingResolver);

                var created = DateTime.Now;
                var task = new Task(taskType)
                {
                    AssigneeUserId = 0,
                    Closed = false,
                    Comment = "hello world",
                    EntityId = -1,
                    OwnerUserId = 0
                };
                repo.AddOrUpdate(task);
                unitOfWork.Flush();

                var alltasktypes = taskTypeRepo.GetAll();

                taskTypeRepo.Delete(taskType);
                unitOfWork.Flush();

                Assert.AreEqual(alltasktypes.Count() - 1, taskTypeRepo.GetAll().Count());
                Assert.AreEqual(0, repo.GetAll().Count());
            }

            
        }
    }
}