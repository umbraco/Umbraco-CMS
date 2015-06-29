using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

using umbraco.BusinessLogic;
using Umbraco.Core;
using umbraco.DataLayer;


namespace umbraco.cms.businesslogic.task
{
    [Obsolete("Use Umbraco.Core.Service.ITaskService instead")]
    public class TaskType
    {
        internal Umbraco.Core.Models.TaskType TaskTypeEntity;

        #region Public Properties

        public int Id
        {
            get { return TaskTypeEntity.Id; }
            set { TaskTypeEntity.Id = value; }
        }

        public string Alias
        {
            get { return TaskTypeEntity.Alias; }
            set { TaskTypeEntity.Alias = value; }
        }

        /// <summary>
        /// All tasks associated with this task type
        /// </summary>
        public Tasks Tasks
        {
            get
            {
                //lazy load the tasks
                if (_tasks == null)
                {
                    _tasks = Task.GetTasksByType(this.Id);
                }
                return _tasks;
            }
        }

        #endregion

        #region Private/protected members

        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        private Tasks _tasks;

        #endregion

        #region Constructors

        internal TaskType(Umbraco.Core.Models.TaskType tt)
        {
            TaskTypeEntity = tt;
        }
        
        public TaskType()
        {
        }

        public TaskType(string TypeAlias)
        {
            TaskTypeEntity = ApplicationContext.Current.Services.TaskService.GetTaskTypeByAlias(TypeAlias);
            if (TaskTypeEntity == null) throw new NullReferenceException("No task type found by alias " + TypeAlias);            
        }

        public TaskType(int TaskTypeId)
        {
            TaskTypeEntity = ApplicationContext.Current.Services.TaskService.GetTaskTypeById(TaskTypeId);
            if (TaskTypeEntity == null) throw new NullReferenceException("No task type found by alias " + TaskTypeId);            
        } 
        #endregion      

        #region Public methods

        public void Save()
        {
            ApplicationContext.Current.Services.TaskService.Save(TaskTypeEntity);
        }

        /// <summary>
        /// Deletes the current task type.
        /// This will remove all tasks associated with this type
        /// </summary>
        public void Delete()
        {
            ApplicationContext.Current.Services.TaskService.Delete(TaskTypeEntity);
        } 

        #endregion

        #region Static methods
        /// <summary>
        /// Returns all task types stored in the database
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<TaskType> GetAll()
        {
            return ApplicationContext.Current.Services.TaskService.GetAllTaskTypes()
                .Select(x => new TaskType(x));
        } 
        #endregion
	
    }
}
