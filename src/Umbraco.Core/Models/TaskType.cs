using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Task Type
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class TaskType : Entity, IAggregateRoot
    {
        private string _alias;

        public TaskType(string @alias)
        {
            _alias = alias;
        }

        private static readonly PropertyInfo AliasSelector = ExpressionHelper.GetPropertyInfo<TaskType, string>(x => x.Alias);

        /// <summary>
        /// Gets or sets the Alias of the TaskType
        /// </summary>
        [DataMember]
        public string Alias
        {
            get { return _alias; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _alias = value;
                    return _alias;
                }, _alias, AliasSelector);    
            }
        }
    }
}