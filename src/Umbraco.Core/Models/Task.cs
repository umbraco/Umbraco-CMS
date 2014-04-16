using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Task
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Task : Entity, IAggregateRoot
    {
        private bool _closed;
        private TaskType _taskType;
        private int _entityId;
        private int _ownerUserId;
        private int _assigneeUserId;
        private string _comment;

        public Task(TaskType taskType)
        {
            _taskType = taskType;
        }

        private static readonly PropertyInfo ClosedSelector = ExpressionHelper.GetPropertyInfo<Task, bool>(x => x.Closed);
        private static readonly PropertyInfo TaskTypeSelector = ExpressionHelper.GetPropertyInfo<Task, TaskType>(x => x.TaskType);
        private static readonly PropertyInfo EntityIdSelector = ExpressionHelper.GetPropertyInfo<Task, int>(x => x.EntityId);
        private static readonly PropertyInfo OwnerUserIdSelector = ExpressionHelper.GetPropertyInfo<Task, int>(x => x.OwnerUserId);
        private static readonly PropertyInfo AssigneeUserIdSelector = ExpressionHelper.GetPropertyInfo<Task, int>(x => x.AssigneeUserId);
        private static readonly PropertyInfo CommentSelector = ExpressionHelper.GetPropertyInfo<Task, string>(x => x.Comment);

        /// <summary>
        /// Gets or sets a boolean indicating whether the task is closed
        /// </summary>
        [DataMember]
        public bool Closed
        {
            get { return _closed; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _closed = value;
                    return _closed;
                }, _closed, ClosedSelector);
            }
        }

        /// <summary>
        /// Gets or sets the TaskType of the Task
        /// </summary>
        [DataMember]
        public TaskType TaskType
        {
            get { return _taskType; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _taskType = value;
                    return _taskType;
                }, _taskType, TaskTypeSelector);
            }
        }

        /// <summary>
        /// Gets or sets the Id of the entity, which this task is associated to
        /// </summary>
        [DataMember]
        public int EntityId
        {
            get { return _entityId; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _entityId = value;
                    return _entityId;
                }, _entityId, EntityIdSelector);                
            }
        }

        /// <summary>
        /// Gets or sets the Id of the user, who owns this task
        /// </summary>
        [DataMember]
        public int OwnerUserId
        {
            get { return _ownerUserId; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _ownerUserId = value;
                    return _ownerUserId;
                }, _ownerUserId, OwnerUserIdSelector);    
            }
        }

        /// <summary>
        /// Gets or sets the Id of the user, who is assigned to this task
        /// </summary>
        [DataMember]
        public int AssigneeUserId
        {
            get { return _assigneeUserId; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _assigneeUserId = value;
                    return _assigneeUserId;
                }, _assigneeUserId, AssigneeUserIdSelector);    
            }
        }

        /// <summary>
        /// Gets or sets the Comment for the Task
        /// </summary>
        [DataMember]
        public string Comment
        {
            get { return _comment; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _comment = value;
                    return _comment;
                }, _comment, CommentSelector);    
            }
        }

    }
}