using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Implements <see cref="IKeyValue"/>.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class KeyValue : EntityBase, IKeyValue
    {
        private string _identifier;
        private string _value;
        private DateTime _updateDate;

        /// <inheritdoc />
        public string Identifier
        {
            get => _identifier;
            set => SetPropertyValueAndDetectChanges(value, ref _identifier, nameof(Identifier));
        }

        /// <inheritdoc />
        public string Value
        {
            get => _value;
            set => SetPropertyValueAndDetectChanges(value, ref _value, nameof(Value));
        }

        /// <inheritdoc />
        public DateTime UpdateDate
        {
            get => _updateDate;
            set => SetPropertyValueAndDetectChanges(value, ref _updateDate, nameof(UpdateDate));
        }
    }
}
