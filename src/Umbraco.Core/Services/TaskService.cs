using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public class TaskService : RepositoryService, ITaskService
    {
        public TaskService(IDatabaseUnitOfWorkProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory)
            : base(provider, logger, eventMessagesFactory)
        {
        }

        public TaskType GetTaskTypeByAlias(string taskTypeAlias)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = uow.CreateRepository<ITaskTypeRepository>();
                return repo.GetByQuery(repo.Query.Where(type => type.Alias == taskTypeAlias)).FirstOrDefault();
            }
        }

        public TaskType GetTaskTypeById(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = uow.CreateRepository<ITaskTypeRepository>();
                return repo.Get(id);
            }
        }

        public void Save(TaskType taskType)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = uow.CreateRepository<ITaskTypeRepository>();
                repo.AddOrUpdate(taskType);
                uow.Complete();
            }
        }

        public void Delete(TaskType taskTypeEntity)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = uow.CreateRepository<ITaskTypeRepository>();
                repo.Delete(taskTypeEntity);
                uow.Complete();
            }
        }

        public IEnumerable<TaskType> GetAllTaskTypes()
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = uow.CreateRepository<ITaskTypeRepository>();
                return repo.GetAll();
            }
        }


        public IEnumerable<Task> GetTasks(int? itemId = null, int? assignedUser = null, int? ownerUser = null, string taskTypeAlias = null, bool includeClosed = false)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = uow.CreateRepository<ITaskRepository>();
                return repo.GetTasks(itemId, assignedUser, ownerUser, taskTypeAlias);
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
                var repo = uow.CreateRepository<ITaskRepository>();
                repo.AddOrUpdate(task);
                uow.Complete();
            }
        }

        public void Delete(Task task)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = uow.CreateRepository<ITaskRepository>();
                repo.Delete(task);
                uow.Complete();
            }
        }

        public Task GetTaskById(int id)
        {
            using (var uow = UowProvider.GetUnitOfWork())
            {
                var repo = uow.CreateRepository<ITaskRepository>();
                return repo.Get(id);
            }
        }
    }
}