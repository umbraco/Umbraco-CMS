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

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo ClosedSelector = ExpressionHelper.GetPropertyInfo<Task, bool>(x => x.Closed);
            public readonly PropertyInfo TaskTypeSelector = ExpressionHelper.GetPropertyInfo<Task, TaskType>(x => x.TaskType);
            public readonly PropertyInfo EntityIdSelector = ExpressionHelper.GetPropertyInfo<Task, int>(x => x.EntityId);
            public readonly PropertyInfo OwnerUserIdSelector = ExpressionHelper.GetPropertyInfo<Task, int>(x => x.OwnerUserId);
            public readonly PropertyInfo AssigneeUserIdSelector = ExpressionHelper.GetPropertyInfo<Task, int>(x => x.AssigneeUserId);
            public readonly PropertyInfo CommentSelector = ExpressionHelper.GetPropertyInfo<Task, string>(x => x.Comment);
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether the task is closed
        /// </summary>
        [DataMember]
        public bool Closed
        {
            get { return _closed; }
            set { SetPropertyValueAndDetectChanges(value, ref _closed, Ps.Value.ClosedSelector); }
        }

        /// <summary>
        /// Gets or sets the TaskType of the Task
        /// </summary>
        [DataMember]
        public TaskType TaskType
        {
            get { return _taskType; }
            set { SetPropertyValueAndDetectChanges(value, ref _taskType, Ps.Value.TaskTypeSelector); }
        }

        /// <summary>
        /// Gets or sets the Id of the entity, which this task is associated to
        /// </summary>
        [DataMember]
        public int EntityId
        {
            get { return _entityId; }
            set { SetPropertyValueAndDetectChanges(value, ref _entityId, Ps.Value.EntityIdSelector); }
        }

        /// <summary>
        /// Gets or sets the Id of the user, who owns this task
        /// </summary>
        [DataMember]
        public int OwnerUserId
        {
            get { return _ownerUserId; }
            set { SetPropertyValueAndDetectChanges(value, ref _ownerUserId, Ps.Value.OwnerUserIdSelector); }
        }

        /// <summary>
        /// Gets or sets the Id of the user, who is assigned to this task
        /// </summary>
        [DataMember]
        public int AssigneeUserId
        {
            get { return _assigneeUserId; }
            set { SetPropertyValueAndDetectChanges(value, ref _assigneeUserId, Ps.Value.AssigneeUserIdSelector); }
        }

        /// <summary>
        /// Gets or sets the Comment for the Task
        /// </summary>
        [DataMember]
        public string Comment
        {
            get { return _comment; }
            set { SetPropertyValueAndDetectChanges(value, ref _comment, Ps.Value.CommentSelector); }
        }

    }
}