using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public class TaskService : ScopeRepositoryService, ITaskService
    {
        public TaskService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, repositoryFactory, logger, eventMessagesFactory)
        {
        }

        public TaskType GetTaskTypeByAlias(string taskTypeAlias)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = RepositoryFactory.CreateTaskTypeRepository(uow);
                var ret =repo.GetByQuery(new Query<TaskType>().Where(type => type.Alias == taskTypeAlias)).FirstOrDefault();
                uow.Commit();
                return ret;
            }
        }

        public TaskType GetTaskTypeById(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = RepositoryFactory.CreateTaskTypeRepository(uow);
                var ret = repo.Get(id);
                uow.Commit();
                return ret;
            }
        }

        public void Save(TaskType taskType)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = RepositoryFactory.CreateTaskTypeRepository(uow);
                repo.AddOrUpdate(taskType);
                uow.Commit();
            }
        }

        public void Delete(TaskType taskTypeEntity)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = RepositoryFactory.CreateTaskTypeRepository(uow);
                repo.Delete(taskTypeEntity);
                uow.Commit();
            }
        }

        public IEnumerable<TaskType> GetAllTaskTypes()
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = RepositoryFactory.CreateTaskTypeRepository(uow);
                var ret = repo.GetAll();
                uow.Commit();
                return ret;
            }
        }


        public IEnumerable<Task> GetTasks(int? itemId = null, int? assignedUser = null, int? ownerUser = null, string taskTypeAlias = null, bool includeClosed = false)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = RepositoryFactory.CreateTaskRepository(uow);
                var ret = repo.GetTasks(itemId, assignedUser, ownerUser, taskTypeAlias);
                uow.Commit();
                return ret;
            }
        }

        /// <summary>
        /// Saves a task
        /// </summary>
        /// <param name="task"></param>
        public void Save(Task task)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = RepositoryFactory.CreateTaskRepository(uow);
                repo.AddOrUpdate(task);
                uow.Commit();
            }
        }

        public void Delete(Task task)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = RepositoryFactory.CreateTaskRepository(uow);
                repo.Delete(task);
                uow.Commit();
            }
        }

        public Task GetTaskById(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = RepositoryFactory.CreateTaskRepository(uow);
                var ret = repo.Get(id);
                uow.Commit();
                return ret;
            }
        }
    }
}