using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface ITaskService : IService
    {
        TaskType GetTaskTypeByAlias(string taskTypeAlias);
        TaskType GetTaskTypeById(int id);
        void Save(TaskType taskType);
        void Delete(TaskType taskTypeEntity);

        //IEnumerable<Task> GetTasks(Guid? itemId = null, int? assignedUser = null, int? ownerUser = null, string taskTypeAlias = null, bool includeClosed = false);
        IEnumerable<Task> GetTasks(int? itemId = null, int? assignedUser = null, int? ownerUser = null, string taskTypeAlias = null, bool includeClosed = false);

        /// <summary>
        /// Saves a task
        /// </summary>
        /// <param name="task"></param>
        void Save(Task task);
        void Delete(Task task);

        IEnumerable<TaskType> GetAllTaskTypes();
        Task GetTaskById(int id);
    }
}