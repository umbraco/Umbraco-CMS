using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public class TaskService : RepositoryService, ITaskService
    {
        public TaskService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger)
            : base(provider, repositoryFactory, logger)
        {
        }

        //public IEnumerable<Task> GetTasks(Guid? itemId = null, int? assignedUser = null, int? ownerUser = null, string taskTypeAlias = null, bool includeClosed = false)
        //{
        //    using (var repo = RepositoryFactory.CreateTaskRepository(UowProvider.GetUnitOfWork()))
        //    {
        //        return repo.GetTasks(itemId, assignedUser, ownerUser, taskTypeAlias);
        //    }
        //}

        public IEnumerable<Task> GetTasks(int? itemId = null, int? assignedUser = null, int? ownerUser = null, string taskTypeAlias = null, bool includeClosed = false)
        {
            using (var repo = RepositoryFactory.CreateTaskRepository(UowProvider.GetUnitOfWork()))
            {
                return repo.GetTasks(itemId, assignedUser, ownerUser, taskTypeAlias);
            }
        }

        /// <summary>
        /// Saves a task
        /// </summary>
        /// <param name="task"></param>
        public void Save(Task task)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateTaskRepository(uow))
            {
                repo.AddOrUpdate(task);
                uow.Commit();
            }
        }
    }
}