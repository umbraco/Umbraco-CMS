using System.Collections.Generic;
using System.Linq;
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

        public TaskType GetTaskTypeByAlias(string taskTypeAlias)
        {
            using (var repo = RepositoryFactory.CreateTaskTypeRepository(UowProvider.GetUnitOfWork()))
            {
                return repo.GetByQuery(new Query<TaskType>().Where(type => type.Alias == taskTypeAlias)).FirstOrDefault();
            }
        }

        public TaskType GetTaskTypeById(int id)
        {
            using (var repo = RepositoryFactory.CreateTaskTypeRepository(UowProvider.GetUnitOfWork()))
            {
                return repo.Get(id);
            }
        }

        public void Save(TaskType taskType)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateTaskTypeRepository(uow))
            {
                repo.AddOrUpdate(taskType);
                uow.Commit();
            }
        }

        public void Delete(TaskType taskTypeEntity)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateTaskTypeRepository(uow))
            {
                repo.Delete(taskTypeEntity);
                uow.Commit();
            }
        }

        public IEnumerable<TaskType> GetAllTaskTypes()
        {
            using (var repo = RepositoryFactory.CreateTaskTypeRepository(UowProvider.GetUnitOfWork()))
            {
                return repo.GetAll();
            }
        }


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

        public void Delete(Task task)
        {
            var uow = UowProvider.GetUnitOfWork();
            using (var repo = RepositoryFactory.CreateTaskRepository(uow))
            {
                repo.Delete(task);
                uow.Commit();
            }
        }

        public Task GetTaskById(int id)
        {
            using (var repo = RepositoryFactory.CreateTaskRepository(UowProvider.GetUnitOfWork()))
            {
                return repo.Get(id);
            }
        }
    }
}