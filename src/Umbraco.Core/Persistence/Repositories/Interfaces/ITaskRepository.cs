using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface ITaskTypeRepository : IRepositoryQueryable<int, TaskType>
    {
        
    }

    public interface ITaskRepository : IRepositoryQueryable<int, Task>
    {
        IEnumerable<Task> GetTasks(int? itemId = null, int? assignedUser = null, int? ownerUser = null, string taskTypeAlias = null, bool includeClosed = false);
    }
}