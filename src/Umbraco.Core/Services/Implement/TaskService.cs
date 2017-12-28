using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    public class TaskService : RepositoryService, ITaskService
    {
        private readonly ITaskTypeRepository _taskTypeRepository;
        private readonly ITaskRepository _taskRepository;

        public TaskService(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory,
            ITaskTypeRepository taskTypeRepository, ITaskRepository taskRepository)
            : base(provider, logger, eventMessagesFactory)
        {
            _taskTypeRepository = taskTypeRepository;
            _taskRepository = taskRepository;
        }

        public TaskType GetTaskTypeByAlias(string taskTypeAlias)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _taskTypeRepository.Get(Query<TaskType>().Where(x => x.Alias == taskTypeAlias)).FirstOrDefault();
            }
        }

        public TaskType GetTaskTypeById(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _taskTypeRepository.Get(id);
            }
        }

        public void Save(TaskType taskType)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _taskTypeRepository.Save(taskType);
                scope.Complete();
            }
        }

        public void Delete(TaskType taskTypeEntity)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _taskTypeRepository.Delete(taskTypeEntity);
                scope.Complete();
            }
        }

        public IEnumerable<TaskType> GetAllTaskTypes()
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _taskTypeRepository.GetMany();
            }
        }


        public IEnumerable<Task> GetTasks(int? itemId = null, int? assignedUser = null, int? ownerUser = null, string taskTypeAlias = null, bool includeClosed = false)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _taskRepository.GetTasks(itemId, assignedUser, ownerUser, taskTypeAlias);
            }
        }

        /// <summary>
        /// Saves a task
        /// </summary>
        /// <param name="task"></param>
        public void Save(Task task)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _taskRepository.Save(task);
                scope.Complete();
            }
        }

        public void Delete(Task task)
        {
            using (var scope = ScopeProvider.CreateScope())
            {
                _taskRepository.Delete(task);
                scope.Complete();
            }
        }

        public Task GetTaskById(int id)
        {
            using (var scope = ScopeProvider.CreateScope(autoComplete: true))
            {
                return _taskRepository.Get(id);
            }
        }
    }
}
