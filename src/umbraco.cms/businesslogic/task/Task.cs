using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Runtime.CompilerServices;

using umbraco.BusinessLogic;
using Umbraco.Core;
using umbraco.DataLayer;

namespace umbraco.cms.businesslogic.task
{
    /// <summary>
    /// An umbraco task is currently only used with the translation workflow in umbraco. But is extendable to cover other taskbased system as well.
    /// A task represent a simple job, it will always be assigned to a user, related to a node, and contain a comment about the task.
    /// The user attached to the task can complete the task, and the author of the task can reopen tasks that are not complete correct.
    /// 
    /// Tasks can in umbraco be used for setting up simple workflows, and contains basic controls structures to determine if the task is completed or not.
    /// </summary>
    [Obsolete("Use Umbraco.Core.Service.ITaskService instead")]
    public class Task
    {

        internal Umbraco.Core.Models.Task TaskEntity;

        #region Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public int Id
        {
            get { return TaskEntity.Id; }
            set { TaskEntity.Id = value; }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Task"/> is closed.
        /// </summary>
        /// <value><c>true</c> if closed; otherwise, <c>false</c>.</value>
        public bool Closed
        {
            get { return TaskEntity.Closed; }
            set { TaskEntity.Closed = value; }
        }
	

        private CMSNode _node;

        /// <summary>
        /// Gets or sets the node.
        /// </summary>
        /// <value>The node.</value>
        public CMSNode Node
        {
            get { return _node ?? (_node = new CMSNode(TaskEntity.EntityId)); }
            set
            {
                _node = value;
                TaskEntity.EntityId = value.Id;
            }
        }

        private TaskType _type;

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public TaskType Type
        {
            get { return _type ?? (_type = new TaskType(TaskEntity.TaskType)); }
            set
            {
                _type = value;
                TaskEntity.TaskType = new Umbraco.Core.Models.TaskType(_type.Alias)
                {
                    Id = _type.Id
                };
            }
        }
	

        private User _parentUser;

        /// <summary>
        /// Gets or sets the parent user.
        /// </summary>
        /// <value>The parent user.</value>
        public User ParentUser
        {
            get { return _parentUser ?? (_parentUser = new User(TaskEntity.OwnerUserId)); }
            set
            {
                _parentUser = value;
                TaskEntity.OwnerUserId = _parentUser.Id;
            }
        }
        
        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        /// <value>The comment.</value>
        public string Comment
        {
            get { return TaskEntity.Comment; }
            set { TaskEntity.Comment = value; }
        }
	
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>The date.</value>
        public DateTime Date
        {
            get { return TaskEntity.CreateDate; }
            set { TaskEntity.CreateDate = value; }
        }
	
        private User _user;

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        /// <value>The user.</value>
        public User User
        {
            get { return _user ?? (_user = new User(TaskEntity.AssigneeUserId)); }
            set
            {
                _user = value;
                TaskEntity.OwnerUserId = _user.Id;
            }
        }

        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Task"/> class.
        /// </summary>
        public Task()
        {
        }

        internal Task(Umbraco.Core.Models.Task task)
        {
            TaskEntity = task;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Task"/> class.
        /// </summary>
        /// <param name="TaskId">The task id.</param>
        public Task(int TaskId)
        {
            TaskEntity = ApplicationContext.Current.Services.TaskService.GetTaskById(TaskId);
            if (TaskEntity == null) throw new NullReferenceException("No task found with id " + TaskId);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Deletes the current task.
        /// Generally tasks should not be deleted and closed instead.
        /// </summary>
        public void Delete()
        {
            ApplicationContext.Current.Services.TaskService.Delete(TaskEntity);
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save()
        {
            ApplicationContext.Current.Services.TaskService.Save(TaskEntity);
        }

        #endregion

        #region static methods

        /// <summary>
        /// Returns all tasks by type
        /// </summary>
        /// <param name="taskType"></param>
        /// <returns></returns>
        public static Tasks GetTasksByType(int taskType)
        {
            var foundTaskType = ApplicationContext.Current.Services.TaskService.GetTaskTypeById(taskType);
            if (foundTaskType == null) return null;

            var result = new Tasks();
            var tasks = ApplicationContext.Current.Services.TaskService.GetTasks(taskTypeAlias: foundTaskType.Alias);
            foreach (var task in tasks)
            {
                result.Add(new Task(task));
            }

            return result;
        }

        /// <summary>
        /// Get all tasks assigned to a node
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public static Tasks GetTasks(int nodeId)
        {
            var result = new Tasks();
            var tasks = ApplicationContext.Current.Services.TaskService.GetTasks(itemId:nodeId);
            foreach (var task in tasks)
            {
                result.Add(new Task(task));
            }

            return result;
        }

        /// <summary>
        /// Retrieves a collection of open tasks assigned to the user
        /// </summary>
        /// <param name="User">The User who have the tasks assigned</param>
        /// <param name="IncludeClosed">If true both open and closed tasks will be returned</param>
        /// <returns>A collections of tasks</returns>
        public static Tasks GetTasks(User User, bool IncludeClosed) 
        {
            var result = new Tasks();
            var tasks = ApplicationContext.Current.Services.TaskService.GetTasks(assignedUser:User.Id, includeClosed:IncludeClosed);
            foreach (var task in tasks)
            {
                result.Add(new Task(task));
            }

            return result;
        }

        /// <summary>
        /// Retrieves a collection of open tasks assigned to the user
        /// </summary>
        /// <param name="User">The User who have the tasks assigned</param>
        /// <param name="IncludeClosed">If true both open and closed tasks will be returned</param>
        /// <returns>A collections of tasks</returns>
        public static Tasks GetOwnedTasks(User User, bool IncludeClosed) 
        {
            var result = new Tasks();
            var tasks = ApplicationContext.Current.Services.TaskService.GetTasks(ownerUser:User.Id, includeClosed: IncludeClosed);
            foreach (var task in tasks)
            {
                result.Add(new Task(task));
            }
            return result;
        }

        #endregion
    }
}
